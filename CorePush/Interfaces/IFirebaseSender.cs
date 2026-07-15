using System.Threading;
using System.Threading.Tasks;

using CorePush.Models;

namespace CorePush.Interfaces;

/// <summary>
/// Interface for sending push notifications via Firebase Cloud Messaging (FCM) HTTP v1 API.
/// </summary>
/// <remarks>
/// The default implementation, <see cref="CorePush.Firebase.FirebaseSender"/>, is thread safe and is
/// designed to be registered as a singleton for dependency injection.
/// </remarks>
public interface IFirebaseSender
{
    /// <summary>
    /// Sends a push notification through Firebase Cloud Messaging.
    /// </summary>
    /// <param name="payload">The FCM message payload object. Will be serialized to JSON.
    /// See <see href="https://firebase.google.com/docs/cloud-messaging/send-message">FCM message format</see>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PushResult"/> indicating success or failure with status code and error details. The call does not throw for FCM HTTP error responses.</returns>
    Task<PushResult> SendAsync(object payload, CancellationToken cancellationToken = default);
}
