using System;
using System.Linq;

namespace CorePush.Utils;

internal static class CryptoHelper
{
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
        
    public static int GetEpochTimestamp()
    {
        var span = DateTime.UtcNow - new DateTime(1970, 1, 1);
        return Convert.ToInt32(span.TotalSeconds);
    }
}