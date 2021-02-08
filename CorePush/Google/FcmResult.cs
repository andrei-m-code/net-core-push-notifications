using Newtonsoft.Json;

namespace CorePush.Google
{
    public class FcmResult
    {
        [JsonProperty("message_id")]
        public string MessageId { get; set; }

        [JsonProperty("registration_id")]
        public string RegistrationId { get; set; }

        public string Error { get; set; }
    }
}
