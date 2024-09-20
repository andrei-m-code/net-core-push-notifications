using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

using CorePush.Interfaces;
using CorePush.Models;
using CorePush.Utils;
using CorePush.Serialization;

namespace CorePush.Apple;

/// <summary>
/// HTTP2 Apple Push Notification sender
/// </summary>
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

    public ApnSender(ApnSettings settings, HttpClient http) : this(settings, http, new DefaultCorePushJsonSerializer())
    {
    }
        
    /// <summary>
    /// Apple push notification sender constructor
    /// </summary>
    /// <param name="settings">Apple Push Notification settings</param>
    /// <param name="http">HTTP client instance</param>
    /// <param name="serializer">JSON serializer</param>
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

    /// <summary>
    /// Serialize and send notification to APN. Please see how your message should be formatted here:
    /// https://developer.apple.com/library/archive/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/CreatingtheNotificationPayload.html#//apple_ref/doc/uid/TP40008194-CH10-SW1
    /// Payload will be serialized using Newtonsoft.Json package.
    /// !IMPORTANT: If you send many messages at once, make sure to retry those calls. Apple typically doesn't like 
    /// to receive too many requests and may occasionally respond with HTTP 429. Just try/catch this call and retry as needed.
    /// </summary>
    /// <exception cref="HttpRequestException">Throws exception when not successful</exception>
    public async Task<PushResult> SendAsync(
        object notification,
        string deviceToken,
        string apnsId = null,
        int apnsExpiration = 0,
        int apnsPriority = 10,
        ApnPushType apnPushType = ApnPushType.Alert,
        CancellationToken cancellationToken = default)
    {
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
            : serializer.Deserialize<ApnsError>(content).Reason;

        return new PushResult((int)response.StatusCode, response.IsSuccessStatusCode, content, error);
    }

    private string GetJwtToken()
    {
        var (token, date) = tokens.GetOrAdd(settings.AppBundleIdentifier, _ => new Tuple<string, DateTime>(CreateJwtToken(), DateTime.UtcNow));
        if (date < DateTime.UtcNow.AddMinutes(-tokenExpiresMinutes))
        {
            tokens.TryRemove(settings.AppBundleIdentifier, out _);
            return GetJwtToken();
        }

        return token;
    }

    private string CreateJwtToken()
    {
        var header = serializer.Serialize(new { alg = "ES256", kid = CryptoHelper.CleanP8Key(settings.P8PrivateKeyId) });
        var payload = serializer.Serialize(new { iss = settings.TeamId, iat = CryptoHelper.GetEpochTimestamp() });
        var headerBase64 = Base64UrlEncode(header);
        var payloadBase64 = Base64UrlEncode(payload);
        var unsignedJwtData = $"{headerBase64}.{payloadBase64}";
        var unsignedJwtBytes = Encoding.UTF8.GetBytes(unsignedJwtData);
            
        var privateKeyBytes = Convert.FromBase64String(CryptoHelper.CleanP8Key(settings.P8PrivateKey));
        var keyParams = (ECPrivateKeyParameters) PrivateKeyFactory.CreateKey(privateKeyBytes);	
        var q = keyParams.Parameters.G.Multiply(keyParams.D).Normalize();	
            
        using var dsa = ECDsa.Create(new ECParameters	
        {	
            Curve = ECCurve.CreateFromValue(keyParams.PublicKeyParamSet.Id),	
            D = keyParams.D.ToByteArrayUnsigned(),	
            Q =	
            {	
                X = q.XCoord.GetEncoded(),	
                Y = q.YCoord.GetEncoded()	
            }	
        });
            
        var signature = dsa.SignData(unsignedJwtBytes, 0, unsignedJwtBytes.Length, HashAlgorithmName.SHA256);
        var signatureBase64 = Base64UrlEncode(signature);
        return $"{unsignedJwtData}.{signatureBase64}";
    }
    
    private static string Base64UrlEncode(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes) => Convert.ToBase64String(bytes);
}