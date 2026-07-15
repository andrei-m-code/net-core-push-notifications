using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CorePush.Interfaces;
using CorePush.Models;
using CorePush.Utils;
using CorePush.Serialization;

namespace CorePush.Apple;

/// <summary>
/// Sends push notifications to Apple devices through the APNs HTTP/2 provider API using
/// token-based (.p8 / JWT) authentication.
/// </summary>
/// <remarks>
/// <para>
/// The sender signs a short-lived ES256 JWT from the configured .p8 key and reuses it across
/// requests, refreshing it automatically as it approaches expiry (APNs provider tokens are valid
/// for up to 60 minutes). Each instance targets a single environment — development or production —
/// selected via <see cref="ApnSettings.ServerType"/>.
/// </para>
/// <para>
/// This type is thread safe and is meant to be long-lived: register it as a singleton (for example
/// with <c>AddHttpClient&lt;IApnSender, ApnSender&gt;()</c>) rather than creating one per notification.
/// Give it a dedicated <see cref="HttpClient"/>, since the constructor sets the client's
/// <see cref="HttpClient.BaseAddress"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var settings = new ApnSettings
/// {
///     AppBundleIdentifier = "com.example.app",
///     P8PrivateKey = "MIGTAgEA...",   // base64 body of the .p8 key
///     P8PrivateKeyId = "ABC123DEFG",  // 10-char key ID
///     TeamId = "DEF123GHIJ",          // 10-char team ID
///     ServerType = ApnServerType.Production
/// };
///
/// var apn = new ApnSender(settings, httpClient);
/// var payload = new { aps = new { alert = new { title = "Hi", body = "Hello" } } };
///
/// var result = await apn.SendAsync(payload, deviceToken);
/// if (!result.IsSuccessStatusCode)
/// {
///     Console.WriteLine(result.Error); // e.g. ApnsErrorReasons.BadDeviceToken
/// }
/// </code>
/// </example>
public class ApnSender : IApnSender
{
    private static readonly ConcurrentDictionary<string, Tuple<string, DateTime>> tokens = new();
    private static readonly Dictionary<ApnServerType, string> servers = new()
    {
        {ApnServerType.Development, "https://api.development.push.apple.com:443" },
        {ApnServerType.Production, "https://api.push.apple.com:443" }
    };

    private const string apnIdHeader = "apns-id";
    private const int tokenExpiresMinutes = 50;

    private readonly ApnSettings settings;
    private readonly HttpClient http;
    private readonly IJsonSerializer serializer;

    /// <summary>
    /// Creates a new Apple Push Notification sender with default JSON serialization.
    /// </summary>
    /// <param name="settings">Apple Push Notification settings (team ID, key, bundle ID, etc.).</param>
    /// <param name="http">A <see cref="HttpClient"/> dedicated to this <see cref="ApnSender"/>. Do not use a shared <see cref="HttpClient"/> instance, since its instance-level state may be modified. However, its <see cref="HttpClientHandler"/> can be shared.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> or <paramref name="http"/> is null.</exception>
    public ApnSender(ApnSettings settings, HttpClient http) : this(settings, http, new DefaultCorePushJsonSerializer())
    {
    }

    /// <summary>
    /// Creates a new Apple Push Notification sender with a custom JSON serializer.
    /// </summary>
    /// <param name="settings">Apple Push Notification settings (team ID, key, bundle ID, etc.).</param>
    /// <param name="http">A <see cref="HttpClient"/> dedicated to this <see cref="ApnSender"/>. Do not use a shared <see cref="HttpClient"/> instance, since its instance-level state may be modified. However, its <see cref="HttpClientHandler"/> can be shared.</param>
    /// <param name="serializer">The JSON serializer used to serialize notification payloads and deserialize APNs error responses.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/>, <paramref name="http"/>, or <paramref name="serializer"/> is null.</exception>
    public ApnSender(ApnSettings settings, HttpClient http, IJsonSerializer serializer)
    {
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        this.http = http ?? throw new ArgumentNullException(nameof(http));
        this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

        if (http.BaseAddress == null)
        {
            http.BaseAddress = new Uri(servers[settings.ServerType]);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// See the Apple payload reference:
    /// <see href="https://developer.apple.com/documentation/usernotifications/generating-a-remote-notification">Generating a remote notification</see>.
    /// <para>
    /// This method does not throw for HTTP error responses; inspect the returned <see cref="PushResult"/> instead.
    /// If you send many notifications at once, APNs may reply with HTTP 429
    /// (<see cref="ApnsErrorReasons.TooManyRequests"/>) — check the result and retry with back-off as needed.
    /// </para>
    /// </remarks>
    /// <exception cref="HttpRequestException">Thrown only on a network or transport-level failure, not for HTTP error status codes.</exception>
    public async Task<PushResult> SendAsync(
        object notification,
        string deviceToken,
        string apnsId = null,
        int apnsExpiration = 0,
        int apnsPriority = 10,
        ApnPushType apnPushType = ApnPushType.Alert,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceToken);

        var path = $"/3/device/{deviceToken}";
        var json = serializer.Serialize(notification);

        using var message = new HttpRequestMessage(HttpMethod.Post, path);

        message.Version = new Version(2, 0);
        message.Content = new StringContent(json);

        message.Headers.Authorization = new AuthenticationHeaderValue("bearer", GetJwtToken());
        message.Headers.TryAddWithoutValidation(":method", "POST");
        message.Headers.TryAddWithoutValidation(":path", path);
        message.Headers.Add("apns-topic", settings.AppBundleIdentifier);
        message.Headers.Add("apns-expiration", apnsExpiration.ToString());
        message.Headers.Add("apns-priority", apnsPriority.ToString());
        message.Headers.Add("apns-push-type", apnPushType.ToString().ToLowerInvariant()); // required for iOS 13+

        if (!string.IsNullOrWhiteSpace(apnsId))
        {
            message.Headers.Add(apnIdHeader, apnsId);
        }

        using var response = await http.SendAsync(message, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var error = response.IsSuccessStatusCode
            ? null
            : serializer.Deserialize<ApnsError>(content)?.Reason;

        return new PushResult((int)response.StatusCode, response.IsSuccessStatusCode, content, error);
    }

    private string GetJwtToken()
    {
        var cacheKey = $"{settings.AppBundleIdentifier}:{settings.P8PrivateKeyId}";
        var (token, date) = tokens.GetOrAdd(cacheKey, _ => new Tuple<string, DateTime>(CreateJwtToken(), DateTime.UtcNow));
        if (date < DateTime.UtcNow.AddMinutes(-tokenExpiresMinutes))
        {
            tokens.TryRemove(cacheKey, out _);
            return GetJwtToken();
        }

        return token;
    }

    private string CreateJwtToken()
    {
        var header = serializer.Serialize(new { alg = "ES256", kid = settings.P8PrivateKeyId });
        var payload = serializer.Serialize(new { iss = settings.TeamId, iat = CryptoHelper.GetEpochTimestamp() });
        var headerBase64 = Base64UrlEncode(header);
        var payloadBase64 = Base64UrlEncode(payload);
        var unsignedJwtData = $"{headerBase64}.{payloadBase64}";
        var unsignedJwtBytes = Encoding.UTF8.GetBytes(unsignedJwtData);

        var pkcs8Bytes = Convert.FromBase64String(CryptoHelper.CleanP8Key(settings.P8PrivateKey));
        var ecPrivateKey = ExtractEcPrivateKey(pkcs8Bytes);

        using var dsa = ECDsa.Create();
        dsa.ImportECPrivateKey(ecPrivateKey, out _);

        var signature = dsa.SignData(unsignedJwtBytes, HashAlgorithmName.SHA256);
        var signatureBase64 = Base64UrlEncode(signature);
        return $"{unsignedJwtData}.{signatureBase64}";
    }

    /// <summary>
    /// Extracts the inner EC private key (SEC 1 / RFC 5915) from a PKCS#8 envelope.
    /// Using ImportECPrivateKey instead of ImportPkcs8PrivateKey avoids the CNG PKCS#8 import
    /// path which fails on Windows Server/IIS when "Load User Profile" is disabled.
    /// </summary>
    internal static byte[] ExtractEcPrivateKey(byte[] pkcs8PrivateKey)
    {
        var reader = new AsnReader(pkcs8PrivateKey, AsnEncodingRules.DER);
        var pkcs8 = reader.ReadSequence();
        pkcs8.ReadEncodedValue(); // version
        pkcs8.ReadSequence();     // algorithm identifier
        return pkcs8.ReadOctetString(); // SEC 1 EC private key
    }

    private static string Base64UrlEncode(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
