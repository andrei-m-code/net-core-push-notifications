namespace CorePush.Apple;

/// <summary>
/// Represents the response from the Apple Push Notification service.
/// </summary>
public class ApnsResponse
{
    /// <summary>
    /// Indicates whether the push notification was accepted by APNs.
    /// </summary>
    public bool IsSuccess { get; set;  }

    /// <summary>
    /// Error details returned by APNs when the notification was rejected. Null on success.
    /// </summary>
    public ApnsError Error { get; set; }
}

/// <summary>
/// Error information returned by APNs when a push notification is rejected.
/// </summary>
public class ApnsError
{
    /// <summary>
    /// The error reason string returned by APNs.
    /// Compare against constants in <see cref="ApnsErrorReasons"/>.
    /// </summary>
    public string Reason {get; set;}

    /// <summary>
    /// For <see cref="ApnsErrorReasons.Unregistered"/> errors, the last time APNs confirmed
    /// the device token was valid, as a UNIX epoch timestamp in milliseconds.
    /// </summary>
    public long? Timestamp {get; set; }
}

/// <summary>
/// Constants for all APNs error reason strings.
/// See <see href="https://developer.apple.com/documentation/usernotifications/handling-notification-responses-from-apns">Apple documentation</see>.
/// </summary>
public static class ApnsErrorReasons
{
    /// <summary>The collapse identifier exceeds the maximum allowed size.</summary>
    public const string BadCollapseId = "BadCollapseId";

    /// <summary>The specified device token is invalid (e.g. wrong size or contains invalid characters).</summary>
    public const string BadDeviceToken = "BadDeviceToken";

    /// <summary>The apns-expiration value is invalid.</summary>
    public const string BadExpirationDate = "BadExpirationDate";

    /// <summary>The apns-id value is invalid.</summary>
    public const string BadMessageId = "BadMessageId";

    /// <summary>The apns-priority value is invalid.</summary>
    public const string BadPriority = "BadPriority";

    /// <summary>The apns-topic value is invalid.</summary>
    public const string BadTopic = "BadTopic";

    /// <summary>The device token doesn't match the specified topic.</summary>
    public const string DeviceTokenNotForTopic = "DeviceTokenNotForTopic";

    /// <summary>One or more headers are repeated.</summary>
    public const string DuplicateHeaders = "DuplicateHeaders";

    /// <summary>Idle timeout.</summary>
    public const string IdleTimeout = "IdleTimeout";

    /// <summary>The device token was not specified in the request path.</summary>
    public const string MissingDeviceToken = "MissingDeviceToken";

    /// <summary>The apns-topic header is required when the client is connected using a certificate that supports multiple topics.</summary>
    public const string MissingTopic = "MissingTopic";

    /// <summary>The notification payload is empty.</summary>
    public const string PayloadEmpty = "PayloadEmpty";

    /// <summary>Pushing to this topic is not allowed.</summary>
    public const string TopicDisallowed = "TopicDisallowed";

    /// <summary>The certificate is invalid.</summary>
    public const string BadCertificate = "BadCertificate";

    /// <summary>The client certificate doesn't match the target environment (sandbox vs. production).</summary>
    public const string BadCertificateEnvironment = "BadCertificateEnvironment";

    /// <summary>The provider token is stale and a new token should be generated.</summary>
    public const string ExpiredProviderToken = "ExpiredProviderToken";

    /// <summary>The specified action is not allowed.</summary>
    public const string Forbidden = "Forbidden";

    /// <summary>The provider token is not valid, or the token signature could not be verified.</summary>
    public const string InvalidProviderToken = "InvalidProviderToken";

    /// <summary>No provider certificate was used to connect to APNs, and the authorization header is missing or no provider token is specified.</summary>
    public const string MissingProviderToken = "MissingProviderToken";

    /// <summary>The request contained an invalid path.</summary>
    public const string BadPath = "BadPath";

    /// <summary>The specified HTTP method is not allowed. Use POST.</summary>
    public const string MethodNotAllowed = "MethodNotAllowed";

    /// <summary>The device token is inactive for the specified topic. The <see cref="ApnsError.Timestamp"/> field contains the last valid timestamp.</summary>
    public const string Unregistered = "Unregistered";

    /// <summary>The notification payload is too large (max 4096 bytes).</summary>
    public const string PayloadTooLarge = "PayloadTooLarge";

    /// <summary>The provider has made too many token update requests in too short a time.</summary>
    public const string TooManyProviderTokenUpdates = "TooManyProviderTokenUpdates";

    /// <summary>Too many requests were sent to APNs. Retry with exponential back-off.</summary>
    public const string TooManyRequests = "TooManyRequests";

    /// <summary>An internal server error occurred on the APNs side.</summary>
    public const string InternalServerError = "InternalServerError";

    /// <summary>The APNs service is unavailable. Retry later.</summary>
    public const string ServiceUnavailable = "ServiceUnavailable";

    /// <summary>The APNs server is shutting down.</summary>
    public const string Shutdown = "Shutdown";
}
