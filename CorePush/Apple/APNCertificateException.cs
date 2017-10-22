using System;
using System.Net.Security;

namespace CorePush.Apple
{
    public class APNCertificateException : Exception
    {
        private readonly SslPolicyErrors policyErrors;

        public APNCertificateException(SslPolicyErrors policyErrors)
        {
            if (policyErrors == SslPolicyErrors.None)
            {
                throw new ArgumentException("Policy errors can't be none", nameof(policyErrors));
            }

            this.policyErrors = policyErrors;
        }

        public override string ToString()
        {
            return policyErrors.ToString();
        }
    }
}
