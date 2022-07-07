using System;
using CorePush.Interfaces;
using CorePush.Utils;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CorePush.Google
{
    /// <summary>
    /// Firebase message sender
    /// </summary>
    public class FcmSender : IFcmSender
    {
        private const string fcmUrl = "https://fcm.googleapis.com/fcm/send";
        
        private readonly FcmSettings settings;
        private readonly HttpClient http;

        public FcmSender(FcmSettings settings, HttpClient http)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.http = http ?? throw new ArgumentNullException(nameof(http));

            if (http.BaseAddress == null)
            {
                http.BaseAddress = new Uri(fcmUrl);    
            }
        }

        /// <summary>
        /// Send firebase notification.
        /// Please check out payload formats:
        /// https://firebase.google.com/docs/cloud-messaging/concept-options#notifications
        /// The SendAsync method will add/replace "to" value with deviceId
        /// </summary>
        /// <param name="deviceId">Device token (will add `to` to the payload)</param>
        /// <param name="payload">Notification payload that will be serialized using Newtonsoft.Json package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="HttpRequestException">Throws exception when not successful</exception>
        public Task<FcmResponse> SendAsync(string deviceId, object payload, CancellationToken cancellationToken = default)
        {
            var jsonObject = JObject.FromObject(payload);
            jsonObject.Remove("to");
            jsonObject.Add("to", JToken.FromObject(deviceId));

            return SendAsync(jsonObject, cancellationToken);
        }

        /// <summary>
        /// Send firebase notification.
        /// Please check out payload formats:
        /// https://firebase.google.com/docs/cloud-messaging/concept-options#notifications
        /// The SendAsync method will add/replace "to" value with deviceId
        /// </summary>
        /// <param name="payload">Notification payload that will be serialized using Newtonsoft.Json package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="HttpRequestException">Throws exception when not successful</exception>
        public async Task<FcmResponse> SendAsync(object payload, CancellationToken cancellationToken = default)
        {
            var serialized = JsonHelper.Serialize(payload);

            using (var message = new HttpRequestMessage())
            {
                message.Method = HttpMethod.Post;
                message.Headers.Add("Authorization", $"key = {settings.ServerKey}");

                if (!string.IsNullOrEmpty(settings.SenderId))
                {
                    message.Headers.Add("Sender", $"id = {settings.SenderId}");
                }

                message.Content = new StringContent(serialized, Encoding.UTF8, "application/json");

                using (var response = await http.SendAsync(message, cancellationToken))
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException("Firebase notification error: " + responseString);
                    }

                    return JsonHelper.Deserialize<FcmResponse>(responseString);
                }
            }
        }
    }
}
