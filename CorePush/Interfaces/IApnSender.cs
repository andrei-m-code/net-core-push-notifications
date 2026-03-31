using System.Threading;
using System.Threading.Tasks;

using CorePush.Apple;
using CorePush.Models;

namespace CorePush.Interfaces;

/// <summary>
/// Interface for sending Apple Push Notifications via the APNs HTTP/2 API.
/// </summary>
public interface IApnSender
{
    /// <summary>
    /// Sends a push notification to an Apple device.
    /// </summary>
    /// <param name="notification">The notification payload object. Will be serialized to JSON.
    /// See <see href="https://developer.apple.com/documentation/usernotifications/generating-a-remote-notification">Apple payload documentation</see>.</param>
    /// <param name="deviceToken">The target device token (hex string) obtained from the device at registration.</param>
    /// <param name="apnsId">Optional unique notification identifier. APNs returns this in its response. If omitted, APNs generates a new UUID.</param>
    /// <param name="apnsExpiration">The UNIX epoch timestamp (seconds) when the notification expires. 0 means immediate delivery only.</param>
    /// <param name="apnsPriority">The notification priority: 10 for immediate delivery, 5 for power-saving delivery.</param>
    /// <param name="apnPushType">The push notification type. Required for iOS 13+ and watchOS 6+.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PushResult"/> indicating success or failure with status code and error details.</returns>
    Task<PushResult> SendAsync(
        object notification,
        string deviceToken,
        string apnsId = null,
        int apnsExpiration = 0,
        int apnsPriority = 10,
        ApnPushType apnPushType = ApnPushType.Alert,
        CancellationToken cancellationToken = default);
}
