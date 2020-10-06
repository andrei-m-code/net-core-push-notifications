using CorePush.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CorePush.Google
{
    public class FcmResponse
    {
        [JsonProperty("multicast_id")]
        public string MulticastId { get; set; }

        [JsonProperty("canonical_ids")]
        public int CanonicalIds { get; set; }

        /// <summary>
        /// Success count
        /// </summary>
        public int Success { get; set; }

        /// <summary>
        /// Failure count
        /// </summary>
        public int Failure { get; set; }

        /// <summary>
        /// Results
        /// </summary>
        public List<FcmResult> Results { get; set; }

        /// <summary>
        /// Returns value indicating notification sent success or failure
        /// </summary>
        /// <returns></returns>
        public bool IsSuccess()
        {
            return Success > 0 && Failure == 0;
        }

        public override string ToString() => JsonHelper.Serialize(this);
    }
}
