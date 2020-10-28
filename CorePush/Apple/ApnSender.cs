using CorePush.Interfaces;
using CorePush.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CorePush.Apple
{
    /// <summary>
    /// HTTP2 Apple Push Notification sender
    /// </summary>
    public class ApnSender : IApnSender
    {
        private static readonly ConcurrentDictionary<string, Tuple<string, DateTime>> tokens = new ConcurrentDictionary<string, Tuple<string, DateTime>>();
        private static readonly Dictionary<ApnServerType, string> servers = new Dictionary<ApnServerType, string>
        {
            {ApnServerType.Development, "https://api.development.push.apple.com:443" },
            {ApnServerType.Production, "https://api.push.apple.com:443" }
        };

        private const string apnidHeader = "apns-id";
        private const int tokenExpiresMinutes = 50;

        private readonly ApnSettings settings;
        private readonly HttpClient http;

        /// <summary>
        /// Apple push notification sender constructor
        /// </summary>
        /// <param name="settings">Apple Push Notification settings</param>
        public ApnSender(ApnSettings settings, HttpClient http)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.http = http ?? throw new ArgumentNullException(nameof(http));
        }

        /// <summary>
        /// Serialize and send notification to APN. Please see how your message should be formatted here:
        /// https://developer.apple.com/library/archive/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/CreatingtheNotificationPayload.html#//apple_ref/doc/uid/TP40008194-CH10-SW1
        /// Payload will be serialized using Newtonsoft.Json package.
        /// !IMPORTANT: If you send many messages at once, make sure to retry those calls. Apple typically doesn't like 
        /// to receive too many requests and may ocasionally respond with HTTP 429. Just try/catch this call and retry as needed.
        /// </summary>
        /// <exception cref="HttpRequestException">Throws exception when not successful</exception>
        public async Task<ApnsResponse> SendAsync(
            object notification,
            string deviceToken,
            string apnsId = null,
            int apnsExpiration = 0,
            int apnsPriority = 10,
            bool isBackground = false)
        {
            var path = $"/3/device/{deviceToken}";
            var json = JsonHelper.Serialize(notification);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(servers[settings.ServerType] + path))
            {
                Version = new Version(2, 0),
                Content = new StringContent(json)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", GetJwtToken());
            request.Headers.TryAddWithoutValidation(":method", "POST");
            request.Headers.TryAddWithoutValidation(":path", path);
            request.Headers.Add("apns-topic", settings.AppBundleIdentifier);
            request.Headers.Add("apns-expiration", apnsExpiration.ToString());
            request.Headers.Add("apns-priority", apnsPriority.ToString());
            request.Headers.Add("apns-push-type", isBackground ? "background" : "alert"); // for iOS 13 required
            if (!string.IsNullOrWhiteSpace(apnsId))
            {
                request.Headers.Add(apnidHeader, apnsId);
            }

            using var response = await http.SendAsync(request);
            var succeed = response.IsSuccessStatusCode;
            var content = await response.Content.ReadAsStringAsync();
            var error = JsonHelper.Deserialize<ApnsError>(content);

            return new ApnsResponse
            {
                IsSuccess = succeed,
                Error = error
            };
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
            var header = JsonHelper.Serialize(new { alg = "ES256", kid = CleanP8Key(settings.P8PrivateKeyId) });
            var payload = JsonHelper.Serialize(new { iss = settings.TeamId, iat = ToEpoch(DateTime.UtcNow) });
            var headerBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(header));
            var payloadBasae64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
            var unsignedJwtData = $"{headerBase64}.{payloadBasae64}";
            var unsignedJwtBytes = Encoding.UTF8.GetBytes(unsignedJwtData);
            using var dsa = ECDsa.Create();
            dsa.ImportPkcs8PrivateKey(Convert.FromBase64String(CleanP8Key(settings.P8PrivateKey)), out _);
            var signature = dsa.SignData(unsignedJwtBytes, 0, unsignedJwtBytes.Length, HashAlgorithmName.SHA256);
            return $"{unsignedJwtData}.{Convert.ToBase64String(signature)}";
        }

        private static int ToEpoch(DateTime time)
        {
            var span = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Convert.ToInt32(span.TotalSeconds);
        }

        private static string CleanP8Key(string p8Key)
        {
            // If we have an empty p8Key, then don't bother doing any tasks.
            if (string.IsNullOrEmpty(p8Key))
            {
                return p8Key;
            }

            List<string> lines = p8Key.Split(new char[] { '\n' }).ToList();
            if (0 != lines.Count && lines[0].StartsWith("-----BEGIN PRIVATE KEY-----"))
            {
                lines.RemoveAt(0);
            }

            if (0 != lines.Count && lines[lines.Count - 1].StartsWith("-----END PRIVATE KEY-----"))
            {
                lines.RemoveAt(lines.Count - 1);
            }

            string result = string.Join("", lines);
            return result;
        }
    }
}
