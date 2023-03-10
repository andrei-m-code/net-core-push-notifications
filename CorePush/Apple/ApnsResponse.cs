namespace CorePush.Apple;

public class ApnsResponse
{
    public bool IsSuccess { get; set;  }

    public ApnsError Error { get; set; }
}

public class ApnsError
{
    /// <summary>
    /// Use <see cref="ApnsErrorReasons"/> to compare against
    /// </summary>
    public string Reason {get; set;}
    public long? Timestamp {get; set; }
}

/// <summary>
/// https://developer.apple.com/library/archive/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/CommunicatingwithAPNs.html#//apple_ref/doc/uid/TP40008194-CH11-SW15
/// </summary>
public static class ApnsErrorReasons
{
    public const string BadCollapseId = "BadCollapseId";
    public const string BadDeviceToken = "BadDeviceToken";
    public const string BadExpirationDate = "BadExpirationDate";
    public const string BadMessageId = "BadMessageId";
    public const string BadPriority = "BadPriority";
    public const string BadTopic = "BadTopic";
    public const string DeviceTokenNotForTopic = "DeviceTokenNotForTopic";
    public const string DuplicateHeaders = "DuplicateHeaders";
    public const string IdleTimeout = "IdleTimeout";
    public const string MissingDeviceToken = "MissingDeviceToken";
    public const string MissingTopic = "MissingTopic";
    public const string PayloadEmpty = "PayloadEmpty";
    public const string TopicDisallowed = "TopicDisallowed";
    public const string BadCertificate = "BadCertificate";
    public const string BadCertificateEnvironment = "BadCertificateEnvironment";
    public const string ExpiredProviderToken = "ExpiredProviderToken";
    public const string Forbidden = "Forbidden";
    public const string InvalidProviderToken = "InvalidProviderToken";
    public const string MissingProviderToken = "MissingProviderToken";
    public const string BadPath = "BadPath";
    public const string MethodNotAllowed = "MethodNotAllowed";
    public const string Unregistered = "Unregistered";
    public const string PayloadTooLarge = "PayloadTooLarge";
    public const string TooManyProviderTokenUpdates = "TooManyProviderTokenUpdates";
    public const string TooManyRequests = "TooManyRequests";
    public const string InternalServerError = "InternalServerError";
    public const string ServiceUnavailable = "ServiceUnavailable";
    public const string Shutdown = "Shutdown";  
}