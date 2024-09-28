using CorePush.Utils;

namespace CorePush.Apple;

#nullable enable

public class ApnSettings
{
    private string? _P8PrivateKey = null;
    private string? _P8PrivateKeyId = null;
    private string? _P8PrivateKeyIdClean = null;

    /// <summary>
    /// p8 certificate string
    /// </summary>
    public string? P8PrivateKey
    {
        get
        {
            return _P8PrivateKey;
        }
        set
        {
            lock (this)
            {
                _P8PrivateKey = value;
                CachedP8PrivateKeyParams = null;
            }
        }
    }

    /// <summary>
    /// 10 digit p8 certificate id. Usually a part of a downloadable certificate filename
    /// </summary>
    public string? P8PrivateKeyId
    {
        get
        {
            return _P8PrivateKeyId;
        }
        set
        {
            lock (this)
            {
                _P8PrivateKeyId = value;
                _P8PrivateKeyIdClean = CryptoHelper.CleanP8Key(value);
            }
        }
    }

    /// <summary>
    /// Apple 10 digit team id
    /// </summary>
    public string? TeamId { get; set; }

    /// <summary>
    /// App slug / bundle name
    /// </summary>
    public string? AppBundleIdentifier { get; set; }

    /// <summary>
    /// Development or Production server
    /// </summary>
    public ApnServerType ServerType { get; set; }

    internal string? P8PrivateKeyIdClean { get { return _P8PrivateKeyIdClean; } }
    internal object? CachedP8PrivateKeyParams;
}