using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using CorePush.Interfaces;
using CorePush.Utils;

namespace CorePush.Apple
{
    public class DefaultJwtTokenProvider : IJwtTokenProvider
    {
        private static readonly ConcurrentDictionary<string, Tuple<string, DateTime>> tokens = new ConcurrentDictionary<string, Tuple<string, DateTime>>();
        private const int tokenExpiresMinutes = 50;

        public string GetJwtToken(ApnSettings settings)
        {
            var (token, date) = tokens.GetOrAdd(settings.AppBundleIdentifier, _ => new Tuple<string, DateTime>(CreateJwtToken(settings), DateTime.UtcNow));
            if (date < DateTime.UtcNow.AddMinutes(-tokenExpiresMinutes))
            {
                tokens.TryRemove(settings.AppBundleIdentifier, out _);
                return GetJwtToken(settings);
            }

            return token;
        }

        public string CreateJwtToken(ApnSettings settings)
        {
            var header = JsonHelper.Serialize(new { alg = "ES256", kid = settings.P8PrivateKeyId });
            var payload = JsonHelper.Serialize(new { iss = settings.TeamId, iat = ToEpoch(DateTime.UtcNow) });
            var headerBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(header));
            var payloadBasae64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
            var unsignedJwtData = $"{headerBase64}.{payloadBasae64}";
            var unsignedJwtBytes = Encoding.UTF8.GetBytes(unsignedJwtData);
            using var dsa = ECDsa.Create();
            dsa.ImportPkcs8PrivateKey(Convert.FromBase64String(settings.P8PrivateKey), out _);
            var signature = dsa.SignData(unsignedJwtBytes, 0, unsignedJwtBytes.Length, HashAlgorithmName.SHA256);
            return $"{unsignedJwtData}.{Convert.ToBase64String(signature)}";
        }

        private static int ToEpoch(DateTime time)
        {
            var span = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Convert.ToInt32(span.TotalSeconds);
        }

        public void ClearJwtToken(ApnSettings settings)
        {
            tokens.TryRemove(settings.AppBundleIdentifier, out _);
        }
    }
}
