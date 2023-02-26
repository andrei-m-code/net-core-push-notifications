using System;
using System.Text.Json.Serialization;

namespace CorePush.Tester;

public class AppleNotification
{
    public class ApsPayload
    {
        public class Alert
        {
            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("body")]
            public string Body { get; set; }
        }

        [JsonPropertyName("alert")]
        public Alert AlertBody { get; set; }

        [JsonPropertyName("apns-push-type")]
        public string PushType { get; set; } = "alert";
    }

    public AppleNotification(Guid id, string message, string title = "")
    {
        Id = id;

        Aps = new ApsPayload
        {
            AlertBody = new ApsPayload.Alert
            {
                Title = title,
                Body = message
            }
        };
    }

    [JsonPropertyName("aps")]
    public ApsPayload Aps { get; set; }

    [JsonPropertyName("id")]
    public Guid Id { get; set; }
}