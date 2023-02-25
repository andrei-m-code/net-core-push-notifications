using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CorePush.Interfaces;
using CorePush.Serialization;

namespace CorePush.Firebase
{
    /// <summary>
    /// Firebase message sender
    /// </summary>
    public class FirebaseSender : IFirebaseSender
    {
        private readonly FirebaseSettings settings;
        private readonly HttpClient http;
        private readonly IJsonSerializer serializer;

        public FirebaseSender(FirebaseSettings settings, HttpClient http) : this(settings, http, new DefaultJsonSerializer())
        {
            
        }

        public FirebaseSender(FirebaseSettings settings, HttpClient http, IJsonSerializer serializer)
        {
            this.http = http ?? throw new ArgumentNullException(nameof(http));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(settings.GoogleProjectId) ||
                string.IsNullOrWhiteSpace(settings.FcmBearerToken))
            {
                throw new ArgumentException("Some settings are not defined", nameof(settings));
            }
            
            if (http.BaseAddress == null)
            {
                var url = $"https://fcm.googleapis.com/v1/projects/{settings.GoogleProjectId}/messages:send";
                http.BaseAddress = new Uri(url);
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
        public async Task<FirebaseResponse> SendAsync(object payload, CancellationToken cancellationToken = default)
        {
            var json = serializer.Serialize(payload);

            using var message = new HttpRequestMessage();
            
            message.Method = HttpMethod.Post;
            message.Headers.Add("Authorization", $"Bearer {settings.FcmBearerToken}");
            message.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await http.SendAsync(message, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Firebase notification error: " + responseString);
            }

            return serializer.Deserialize<FirebaseResponse>(responseString);
        }
    }
}
