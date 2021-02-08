using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;	
using Org.BouncyCastle.Security;

namespace CorePush.Utils
{
    public static class AppleCryptoHelper
    {
        public static ECDsa GetEllipticCurveAlgorithm(string privateKey)	
        {	
            var keyParams = (ECPrivateKeyParameters) PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));	
            var q = keyParams.Parameters.G.Multiply(keyParams.D).Normalize();	

            return ECDsa.Create(new ECParameters	
            {	
                Curve = ECCurve.CreateFromValue(keyParams.PublicKeyParamSet.Id),	
                D = keyParams.D.ToByteArrayUnsigned(),	
                Q =	
                {	
                    X = q.XCoord.GetEncoded(),	
                    Y = q.YCoord.GetEncoded()	
                }	
            });
        }

        public static string CleanP8Key(string p8Key)
        {
            // If we have an empty p8Key, then don't bother doing any tasks.
            if (string.IsNullOrEmpty(p8Key))
            {
                return p8Key;
            }

            List<string> lines = p8Key.Split(new char[] { '\n' }).ToList();
            if (0 != lines.Count && lines[0].StartsWith("-----BEGIN PRIVATE KEY-----"))
            {
                lines.RemoveAt(0);
            }

            if (0 != lines.Count && lines[lines.Count - 1].StartsWith("-----END PRIVATE KEY-----"))
            {
                lines.RemoveAt(lines.Count - 1);
            }

            string result = string.Join("", lines);
            return result;
        }
    }
}
