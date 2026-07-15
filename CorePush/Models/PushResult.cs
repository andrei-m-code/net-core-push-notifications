namespace CorePush.Models;

/// <summary>
/// The result of a push notification send operation, returned by both
/// <see cref="CorePush.Apple.ApnSender"/> and <see cref="CorePush.Firebase.FirebaseSender"/>.
/// </summary>
/// <param name="StatusCode">The HTTP status code returned by the push notification service.</param>
/// <param name="IsSuccessStatusCode">True when the HTTP response status indicates success (2xx).</param>
/// <param name="Message">Additional detail about the result. On success this is the raw APNs response body (<see cref="CorePush.Apple.ApnSender"/>) or the FCM message identifier (<see cref="CorePush.Firebase.FirebaseSender"/>); on failure it is the error message returned by the service.</param>
/// <param name="Error">The error reason string from APNs (see <see cref="CorePush.Apple.ApnsErrorReasons"/>) or the gRPC status from FCM. Null on success.</param>
public record PushResult(
    int StatusCode,
    bool IsSuccessStatusCode,
    string Message,
    string Error);
