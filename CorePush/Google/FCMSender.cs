using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using CorePush.Interfaces;

namespace CorePush.Google
{
    public class FCMSender : INotificationSender, IDisposable
    {
        private readonly string fcmUrl = "https://fcm.googleapis.com/fcm/send";
        private readonly string serverKey;
        private readonly string senderId;
        private readonly Lazy<HttpClient> lazyHttp = new Lazy<HttpClient>();

        public FCMSender(string serverKey, string senderId)
        {
            this.serverKey = serverKey;
            this.senderId = senderId;
        }

        public async Task SendAsync(string deviceId, object payload)
        {
            var jsonObject = JObject.FromObject(payload);
            jsonObject.Remove("to");
            jsonObject.Add("to", JToken.FromObject(deviceId));
            var json = jsonObject.ToString();

            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, fcmUrl))
            {
                httpRequest.Headers.Add("Authorization", $"key = {serverKey}");
                httpRequest.Headers.Add("Sender", $"id = {senderId}");
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var response = await lazyHttp.Value.SendAsync(httpRequest))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"HTTP {response.StatusCode}, {response.ReasonPhrase}");
                    }
                }
            };
        }

        public void Dispose()
        {
            if (lazyHttp.IsValueCreated)
            {
                lazyHttp.Value.Dispose();
            }
        }
    }
}
