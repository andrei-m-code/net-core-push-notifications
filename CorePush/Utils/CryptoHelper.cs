using System;
using System.Linq;

namespace CorePush.Utils;

/// <summary>
/// Internal helpers for preparing key material and JWT timestamps.
/// </summary>
internal static class CryptoHelper
{
    /// <summary>
    /// Strips the PEM header/footer lines from a base64 private key, returning the raw base64 body.
    /// Returns the input unchanged when it is null, empty, or already header-free.
    /// </summary>
    public static string CleanP8Key(string p8Key)
    {
        // If we have an empty p8Key, then don't bother doing any tasks.
        if (string.IsNullOrEmpty(p8Key))
        {
            return p8Key;
        }

        var lines = p8Key.Split('\n').ToList();

        if (0 != lines.Count && lines[0].StartsWith("-----BEGIN PRIVATE KEY-----"))
        {
            lines.RemoveAt(0);
        }

        if (0 != lines.Count && lines[^1].StartsWith("-----END PRIVATE KEY-----"))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        var result = string.Join(string.Empty, lines);

        return result;
    }
        
    /// <summary>
    /// Returns the current UTC time as a Unix timestamp (whole seconds since 1970-01-01), used for JWT <c>iat</c>/<c>exp</c> claims.
    /// </summary>
    public static long GetEpochTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}