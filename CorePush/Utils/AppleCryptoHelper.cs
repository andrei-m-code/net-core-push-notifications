
using System;
using System.Security.Cryptography;

#if NETSTANDARD2_0
using Org.BouncyCastle.Crypto.Parameters;	
using Org.BouncyCastle.Security;
#endif

namespace CorePush.Utils
{
    public static class AppleCryptoHelper
    {
#if NETSTANDARD2_1
        public static ECDsa GetEllipticCurveAlgorithm(string privateKey)
        {
            var dsa = ECDsa.Create();

            try
            {
                dsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
                return dsa;
            }
            catch
            {
                dsa.Dispose();
                throw;
            }
        }
#endif

#if NETSTANDARD2_0
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
#endif
    }
}
