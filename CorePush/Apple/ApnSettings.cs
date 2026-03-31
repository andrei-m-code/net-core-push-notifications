namespace CorePush.Apple;

/// <summary>
/// Configuration settings for Apple Push Notification service (APNs).
/// Uses token-based authentication with a .p8 key file from the Apple Developer portal.
/// </summary>
public class ApnSettings
{
    /// <summary>
    /// The contents of the .p8 private key file downloaded from the Apple Developer portal.
    /// Can include or omit the PEM header/footer lines.
    /// </summary>
    public string P8PrivateKey { get; set; }

    /// <summary>
    /// The 10-character Key ID for the .p8 key, found in the Apple Developer portal
    /// or as part of the downloaded .p8 filename.
    /// </summary>
    public string P8PrivateKeyId { get; set; }

    /// <summary>
    /// Your 10-character Apple Developer Team ID, visible in the Apple Developer portal
    /// under Membership details.
    /// </summary>
    public string TeamId { get; set; }

    /// <summary>
    /// The bundle identifier of your app (e.g. "com.example.myapp"),
    /// used as the apns-topic header value.
    /// </summary>
    public string AppBundleIdentifier { get; set; }

    /// <summary>
    /// The APNs server environment: <see cref="ApnServerType.Development"/> for sandbox
    /// or <see cref="ApnServerType.Production"/> for live apps.
    /// </summary>
    public ApnServerType ServerType { get; set; }
}
