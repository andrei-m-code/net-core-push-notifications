using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CorePush.Interfaces;
using CorePush.Serialization;

namespace CorePush.Google
{
    /// <summary>
    /// Firebase message sender
    /// </summary>
    public class FcmSender : IFcmSender
    {
        // TODO: Migrate to the new API: https://firebase.google.com/docs/cloud-messaging/send-message
        private const string fcmUrl = "https://fcm.googleapis.com/fcm/send";
        
        private readonly FcmSettings settings;
        private readonly HttpClient http;
        private readonly Serialization.IJsonSerializer serializer;

        public FcmSender(FcmSettings settings, HttpClient http) : this(settings, http, new DefaultJsonSerializer())
        {
        }

        public FcmSender(FcmSettings settings, HttpClient http, IJsonSerializer serializer)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.http = http ?? throw new ArgumentNullException(nameof(http));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

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
        /// <param name="payload">Notification payload that will be serialized using Newtonsoft.Json package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="HttpRequestException">Throws exception when not successful</exception>
        public async Task<FcmResponse> SendAsync(object payload, CancellationToken cancellationToken = default)
        {
            var json = serializer.Serialize(payload);

            using var message = new HttpRequestMessage();
            
            message.Method = HttpMethod.Post;
            message.Headers.Add("Authorization", $"key = {settings.ServerKey}");

            if (!string.IsNullOrEmpty(settings.SenderId))
            {
                message.Headers.Add("Sender", $"id = {settings.SenderId}");
            }

            message.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await http.SendAsync(message, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Firebase notification error: " + responseString);
            }

            return serializer.Deserialize<FcmResponse>(responseString);
        }
    }
}
