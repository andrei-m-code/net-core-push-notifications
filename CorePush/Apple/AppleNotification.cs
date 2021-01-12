using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CorePush.Apple
{
    public class AppleNotification
    {
        public class AlertPayload
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("subtitle")]
            public string SubTitle { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }

        }

        public class ApsPayload
        {
            [JsonProperty("alert")]
            public AlertPayload Alert { get; set; }
        }

        [JsonProperty("aps")]
        public ApsPayload Aps { get; set; }

        // Your custom properties as needed
        // Note that before sending this to apple it's moved from 'data'
        // so that each key is a peer in AppleNotification instead.  I'm sure
        // there's a custom JSON read/writer that we could create, but this
        // is the easiest way I could think of to achieve this for now.
        [JsonProperty("data")]
        public Dictionary<string, string> Data {get; set;}
    }
}
