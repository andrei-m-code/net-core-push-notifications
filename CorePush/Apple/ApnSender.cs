using CorePush.Interfaces;
using CorePush.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
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
        private static readonly Dictionary<ApnServerType, string> servers = new Dictionary<ApnServerType, string>
        {
            {ApnServerType.Development, "https://api.sandbox.push.apple.com:443" },
            {ApnServerType.Production, "https://api.push.apple.com:443" }
        };

        private const string apnidHeader = "apns-id";

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
        /// 
        public async Task<ApnsResponse> SendAsync(
            object notification,
            string deviceToken,
            string apnsId = null,
            int apnsExpiration = 0,
            int apnsPriority = 10,
            bool isBackground = false,
            int maxRetries=0,
            IJwtTokenProvider jwtProvider = null)
        {

            if (jwtProvider == null)
            {
                jwtProvider = new DefaultJwtTokenProvider();
            }

            var path = $"/3/device/{deviceToken}";
            var json = JsonHelper.Serialize(notification);

            int tryCount = 0;
            int statusCode = -1;
            bool succeed = false;
            string content = null;
            ApnsError error = null;

            while (!succeed && tryCount++ <= maxRetries)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(servers[settings.ServerType] + path))
                {
                    Version = new Version(2, 0),
                    Content = new StringContent(json)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", jwtProvider.GetJwtToken(settings));
                request.Headers.TryAddWithoutValidation(":method", "POST");
                request.Headers.TryAddWithoutValidation(":path", path);
                request.Headers.Add("apns-topic", settings.AppBundleIdentifier);
                request.Headers.Add("apns-expiration", apnsExpiration.ToString());
                request.Headers.Add("apns-priority", apnsPriority.ToString());
                request.Headers.Add("apns-push-type", isBackground ? "background" : "alert"); // Required for watchOS 6 and later; recommended for macOS, iOS, tvOS, and iPadOS
                if (!string.IsNullOrWhiteSpace(apnsId))
                {
                    request.Headers.Add(apnidHeader, apnsId);
                }

                using var response = await http.SendAsync(request);
                succeed = response.IsSuccessStatusCode;
                content = await response.Content.ReadAsStringAsync();
                statusCode = (int)response.StatusCode;
                error = JsonHelper.Deserialize<ApnsError>(content);

                if (HttpStatusCode.Forbidden.Equals(response.StatusCode))
                {
                    jwtProvider.ClearJwtToken(settings);
                }
            }

            return new ApnsResponse
            {
                IsSuccess = succeed,
                StatusCode = statusCode,
                Error = error
            };
        }
    }
}