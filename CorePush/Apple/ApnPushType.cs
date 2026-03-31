namespace CorePush.Apple;

/// <summary>
/// The type of APNs push notification. Required for iOS 13+.
/// See <see href="https://developer.apple.com/documentation/usernotifications/sending-notification-requests-to-apns">Apple documentation</see>.
/// </summary>
public enum ApnPushType
{
    /// <summary>
    /// Background notification that wakes the app to perform a task.
    /// Must set apns-priority to 5.
    /// </summary>
    Background,

    /// <summary>
    /// Visible notification that displays an alert, plays a sound, or badges the app icon.
    /// </summary>
    Alert,

    /// <summary>
    /// VoIP push notification. Requires the VoIP push entitlement in your app.
    /// </summary>
    Voip
}
