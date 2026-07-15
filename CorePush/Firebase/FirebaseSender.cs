using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CorePush.Interfaces;
using CorePush.Models;
using CorePush.Serialization;
using CorePush.Utils;

namespace CorePush.Firebase;

/// <summary>
/// Sends push notifications to Android, iOS and Web clients through the Firebase Cloud Messaging (FCM)
/// HTTP v1 API.
/// </summary>
/// <remarks>
/// <para>
/// The sender exchanges the service-account credentials for a Google OAuth2 access token, caches it,
/// and refreshes it automatically shortly before it expires. Payloads are serialized to JSON and posted
/// verbatim to the FCM <c>messages:send</c> endpoint, so their shape is entirely up to the caller.
/// </para>
/// <para>
/// This type is thread safe and is meant to be long-lived: register it as a singleton (for example
/// with <c>AddHttpClient&lt;IFirebaseSender, FirebaseSender&gt;()</c>). Its <see cref="HttpClient"/> may be
/// shared with other code, since the sender uses absolute URLs and does not modify the client.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var serviceAccountJson = await File.ReadAllTextAsync("service-account.json");
/// var fcm = new FirebaseSender(serviceAccountJson, httpClient);
///
/// var message = new
/// {
///     message = new
///     {
///         token = deviceToken,
///         notification = new { title = "Hi", body = "Hello" }
///     }
/// };
///
/// var result = await fcm.SendAsync(message);
/// if (!result.IsSuccessStatusCode)
/// {
///     Console.WriteLine($"{result.Error}: {result.Message}");
/// }
/// </code>
/// </example>
public class FirebaseSender : IFirebaseSender
{
    private readonly HttpClient http;
    private readonly FirebaseSettings settings;
    private readonly IJsonSerializer serializer;
    private readonly SemaphoreSlim tokenLock = new(1, 1);

    private DateTime? firebaseTokenExpiration;
    private FirebaseTokenResponse firebaseToken;

    /// <summary>
    /// Initialize FirebaseSender
    /// </summary>
    /// <param name="serviceAccountFileJson">Service Account Key JSON file contents.
    /// The file would have a name like: myproject-12345-abc123123.json</param>
    /// <param name="http">HTTP client</param>
    public FirebaseSender(string serviceAccountFileJson, HttpClient http): this(serviceAccountFileJson, http, new DefaultCorePushJsonSerializer())
    {
    }

    /// <summary>
    /// Initialize FirebaseSender
    /// </summary>
    /// <param name="serviceAccountFileJson">Service Account Key JSON file contents.
    /// The file would have a name like: myproject-12345-abc123123.json</param>
    /// <param name="http">HTTP client</param>
    /// <param name="serializer">Customized JSON serializer</param>
    public FirebaseSender(string serviceAccountFileJson, HttpClient http, IJsonSerializer serializer)
        : this(serializer.Deserialize<FirebaseSettings>(serviceAccountFileJson), http, serializer)
    {
    }

    /// <summary>
    /// Initialize FirebaseSender
    /// </summary>
    /// <param name="settings">Firebase Service Account Key JSON file settings. FirebaseSettings record can be used as a target of deserialization
    /// of the Firebase SDK key file e.g. myproject-12345-abc123123.json</param>
    /// <param name="http">HTTP client</param>
    public FirebaseSender(FirebaseSettings settings, HttpClient http) : this(settings, http, new DefaultCorePushJsonSerializer())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="FirebaseSender"/> from parsed Firebase service account settings, using a custom JSON serializer.
    /// </summary>
    /// <param name="settings">Firebase service account settings. <see cref="FirebaseSettings"/> can be used as a deserialization target for the Firebase service account key file, e.g. myproject-12345-abc123123.json.</param>
    /// <param name="http">The <see cref="HttpClient"/> used to call the Google OAuth2 and FCM endpoints. Because the sender uses absolute URLs and does not modify the client, a shared instance is fine.</param>
    /// <param name="serializer">The JSON serializer used to serialize message payloads and deserialize FCM responses.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/>, <paramref name="http"/>, or <paramref name="serializer"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when required <paramref name="settings"/> values (client email, private key, project ID, or token URI) are missing.</exception>
    public FirebaseSender(FirebaseSettings settings, HttpClient http, IJsonSerializer serializer)
    {
        this.http = http ?? throw new ArgumentNullException(nameof(http));
        this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

        if (string.IsNullOrWhiteSpace(settings.ClientEmail) ||
            string.IsNullOrWhiteSpace(settings.PrivateKey) ||
            string.IsNullOrWhiteSpace(settings.ProjectId) ||
            string.IsNullOrWhiteSpace(settings.TokenUri))
        {
            throw new ArgumentException("Some settings are not defined", nameof(settings));
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// The payload must contain a target (e.g. a device <c>token</c>) as described in the FCM payload formats:
    /// <see href="https://firebase.google.com/docs/cloud-messaging/concept-options#notifications">notification options</see> and
    /// <see href="https://firebase.google.com/docs/cloud-messaging/send-message">send a message</see>.
    /// <para>
    /// This method does not throw for HTTP error responses from FCM; inspect the returned <see cref="PushResult"/> instead.
    /// </para>
    /// </remarks>
    /// <exception cref="HttpRequestException">Thrown when the OAuth2 access-token request to Google fails, or on a network/transport-level failure.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the OAuth2 access-token response cannot be read.</exception>
    public async Task<PushResult> SendAsync(object payload, CancellationToken cancellationToken = default)
    {
        var json = serializer.Serialize(payload);

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://fcm.googleapis.com/v1/projects/{settings.ProjectId}/messages:send");

        var token = await GetJwtTokenAsync(cancellationToken);

        message.Headers.Add("Authorization", $"Bearer {token}");
        message.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await http.SendAsync(message, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        var firebaseResponse = serializer.Deserialize<FirebaseResponse>(responseString);

        return new PushResult((int) response.StatusCode,
            response.IsSuccessStatusCode,
            firebaseResponse.Name ?? firebaseResponse.Error?.Message,
            firebaseResponse.Error?.Status);
    }

    private async Task<string> GetJwtTokenAsync(CancellationToken cancellationToken)
    {
        if (firebaseToken != null && firebaseTokenExpiration > DateTime.UtcNow)
        {
            return firebaseToken.AccessToken;
        }

        await tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (firebaseToken != null && firebaseTokenExpiration > DateTime.UtcNow)
            {
                return firebaseToken.AccessToken;
            }

            using var message = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");
            using var form = new MultipartFormDataContent();
            var authToken = GetMasterToken();
            form.Add(new StringContent(authToken), "assertion");
            form.Add(new StringContent("urn:ietf:params:oauth:grant-type:jwt-bearer"), "grant_type");
            message.Content = form;

            using var response = await http.SendAsync(message, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Firebase error when creating JWT token: " + content);
            }

            firebaseToken = serializer.Deserialize<FirebaseTokenResponse>(content);
            firebaseTokenExpiration = DateTime.UtcNow.AddSeconds(firebaseToken.ExpiresIn - 10);

            if (string.IsNullOrWhiteSpace(firebaseToken.AccessToken) || firebaseTokenExpiration < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Couldn't deserialize firebase token response");
            }

            return firebaseToken.AccessToken;
        }
        finally
        {
            tokenLock.Release();
        }
    }

    private string GetMasterToken()
    {
        var header = serializer.Serialize(new { alg = "RS256", typ = "JWT" });
        var payload = serializer.Serialize(new
        {
            iss = settings.ClientEmail,
            aud = settings.TokenUri,
            scope = "https://www.googleapis.com/auth/firebase.messaging",
            iat = CryptoHelper.GetEpochTimestamp(),
            exp = CryptoHelper.GetEpochTimestamp() + 3600 /* has to be short lived */
        });

        var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(header));
        var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
        var unsignedJwtData = $"{headerBase64}.{payloadBase64}";
        var unsignedJwtBytes = Encoding.UTF8.GetBytes(unsignedJwtData);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(settings.PrivateKey);

        var signature = rsa.SignData(unsignedJwtBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var signatureBase64 = Base64UrlEncode(signature);

        return $"{unsignedJwtData}.{signatureBase64}";
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
