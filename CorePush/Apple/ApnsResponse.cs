using CorePush.Utils;
using Newtonsoft.Json;

namespace CorePush.Apple
{
    public class ApnsResponse
    {
        public bool IsSuccess { get; set;  }

        public ApnsError Error { get; set; }

        public override string ToString() => JsonHelper.Serialize(this);
    }

    public class ApnsError
    {
        public ReasonEnum Reason {get; set;}
        public long? Timestamp {get; set; }
    }

    /// <summary>
    /// https://developer.apple.com/library/archive/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/CommunicatingwithAPNs.html#//apple_ref/doc/uid/TP40008194-CH11-SW15
    /// </summary>
    public enum ReasonEnum
    {
        BadCollapseId,
        BadDeviceToken,
        BadExpirationDate,
        BadMessageId,
        BadPriority,
        BadTopic,
        DeviceTokenNotForTopic,
        DuplicateHeaders,
        IdleTimeout,
        MissingDeviceToken,
        MissingTopic,
        PayloadEmpty,
        TopicDisallowed,
        BadCertificate,
        BadCertificateEnvironment,
        ExpiredProviderToken,
        Forbidden,
        InvalidProviderToken,
        MissingProviderToken,
        BadPath,
        MethodNotAllowed,
        Unregistered,
        PayloadTooLarge,
        TooManyProviderTokenUpdates,
        TooManyRequests,
        InternalServerError,
        ServiceUnavailable,
        Shutdown,  
    }
}
