namespace CorePush.Models;

/// <summary>
/// The result of a push notification send operation, returned by both
/// <see cref="CorePush.Apple.ApnSender"/> and <see cref="CorePush.Firebase.FirebaseSender"/>.
/// </summary>
/// <param name="StatusCode">The HTTP status code from the push notification service.</param>
/// <param name="IsSuccessStatusCode">True if the HTTP response indicates success (2xx).</param>
/// <param name="Message">The raw response body on success, or the error message/reason on failure.</param>
/// <param name="Error">The error reason string from APNs or gRPC status from FCM. Null on success.</param>
public record PushResult(
    int StatusCode,
    bool IsSuccessStatusCode,
    string Message,
    string Error);
