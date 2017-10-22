using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CorePush.Google
{
    public class GCMSender : IDisposable
    {
        private const string gcmUrl = "https://android.googleapis.com/gcm/send";

        private readonly string apiKey;
        private readonly string senderId;

        private readonly Lazy<HttpClient> lazyHttp = new Lazy<HttpClient>();

        public GCMSender(string apiKey, string senderId)
        {
            this.apiKey = apiKey;
            this.senderId = senderId;
        }

        public async Task SendAsync(string deviceId, object payload)
        {
            var payloadString = JsonConvert.SerializeObject(new
            {
                to = deviceId,
                data = payload
            });

            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, gcmUrl))
            {
                httpRequest.Headers.Add("Authorization", $"key = {apiKey}");
                httpRequest.Headers.Add("Sender", senderId);
                httpRequest.Content = new StringContent(payloadString, Encoding.UTF8, "application/json");

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
