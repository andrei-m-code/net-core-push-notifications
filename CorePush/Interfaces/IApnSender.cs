using System.Threading;
using System.Threading.Tasks;

using CorePush.Apple;
using CorePush.Models;

namespace CorePush.Interfaces;

/// <summary>
/// Interface for sending Apple Push Notifications via the APNs HTTP/2 API.
/// </summary>
/// <remarks>
/// The default implementation, <see cref="ApnSender"/>, is thread safe and is designed to be
/// registered as a singleton for dependency injection.
/// </remarks>
public interface IApnSender
{
    /// <summary>
    /// Sends a push notification to an Apple device.
    /// </summary>
    /// <param name="notification">The notification payload object. Will be serialized to JSON.
    /// See <see href="https://developer.apple.com/documentation/usernotifications/generating-a-remote-notification">Apple payload documentation</see>.</param>
    /// <param name="deviceToken">The target device token (hex string) obtained from the device at registration.</param>
    /// <param name="apnsId">Optional unique notification identifier. APNs returns this in its response. If omitted, APNs generates a new UUID.</param>
    /// <param name="apnsExpiration">A UNIX epoch timestamp (in seconds) after which APNs stops attempting to deliver the notification. 0 tells APNs to attempt delivery only once and to not store the notification.</param>
    /// <param name="apnsPriority">The delivery priority: 10 for immediate delivery, 5 for power-considerate delivery, or 1 for the lowest priority (iOS 15 and later).</param>
    /// <param name="apnPushType">The push notification type. Required for iOS 13+ and watchOS 6+.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PushResult"/> indicating success or failure with status code and error details. The call does not throw for APNs HTTP error responses.</returns>
    Task<PushResult> SendAsync(
        object notification,
        string deviceToken,
        string apnsId = null,
        int apnsExpiration = 0,
        int apnsPriority = 10,
        ApnPushType apnPushType = ApnPushType.Alert,
        CancellationToken cancellationToken = default);
}
