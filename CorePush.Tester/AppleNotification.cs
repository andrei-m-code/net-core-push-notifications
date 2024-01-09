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

        [JsonPropertyName("badge")]
        public int Badge { get; set; } = 0;
    }

    public AppleNotification(Guid id, string message, string title = "", int badge = 0)
    {
        Id = id;

        Aps = new ApsPayload
        {
            Badge = badge,
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