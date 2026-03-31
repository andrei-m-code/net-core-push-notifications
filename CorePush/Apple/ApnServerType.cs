namespace CorePush.Apple;

/// <summary>
/// Specifies the APNs server environment to send push notifications to.
/// </summary>
public enum ApnServerType
{
    /// <summary>
    /// Apple sandbox environment for development and testing.
    /// Uses api.development.push.apple.com.
    /// </summary>
    Development,

    /// <summary>
    /// Apple production environment for live apps.
    /// Uses api.push.apple.com.
    /// </summary>
    Production
}
