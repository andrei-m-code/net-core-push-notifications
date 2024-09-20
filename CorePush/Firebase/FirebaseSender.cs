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
/// Firebase message sender
/// </summary>
public class FirebaseSender : IFirebaseSender
{
    private readonly HttpClient http;
    private readonly FirebaseSettings settings;
    private readonly IJsonSerializer serializer;

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
    /// Initialize FirebaseSender
    /// </summary>
    /// <param name="settings">Firebase Service Account Key JSON file settings. FirebaseSettings record can be used as a target of deserialization
    /// of the Firebase SDK key file e.g. myproject-12345-abc123123.json</param>
    /// <param name="http">HTTP client</param>
    /// <param name="serializer">Customized JSON serializer</param>
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

    /// <summary>
    /// Send firebase notification. Token must be present in order to send direct push notification.
    /// Please check out payload formats:
    /// https://firebase.google.com/docs/cloud-messaging/concept-options#notifications
    /// https://firebase.google.com/docs/cloud-messaging/send-message
    /// </summary>
    /// <param name="payload">Notification payload that will be serialized using Newtonsoft.Json package</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="HttpRequestException">Throws exception when not successful</exception>
    public async Task<CodePushResponse> SendAsync(object payload, CancellationToken cancellationToken = default)
    {
        var json = serializer.Serialize(payload);

        using var message = new HttpRequestMessage(
            HttpMethod.Post, 
            $"https://fcm.googleapis.com/v1/projects/{settings.ProjectId}/messages:send");

        var token = await GetJwtTokenAsync();
            
        message.Headers.Add("Authorization", $"Bearer {token}");
        message.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await http.SendAsync(message, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        var firebaseResponse = serializer.Deserialize<FirebaseResponse>(responseString);
        
        return new CodePushResponse((int) response.StatusCode,
            response.IsSuccessStatusCode,
            firebaseResponse.Name ?? firebaseResponse.Error?.Message,
            responseString);
    }

    private async Task<string> GetJwtTokenAsync()
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

        using var response = await http.SendAsync(message);
        var content = await response.Content.ReadAsStringAsync();
            
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
        
        var headerBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(header));
        var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
        var unsignedJwtData = $"{headerBase64}.{payloadBase64}";
        var unsignedJwtBytes = Encoding.UTF8.GetBytes(unsignedJwtData);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(settings.PrivateKey.ToCharArray());

        var signature = rsa.SignData(unsignedJwtBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var signatureBase64 = Convert.ToBase64String(signature);

        return $"{unsignedJwtData}.{signatureBase64}";
    }
}