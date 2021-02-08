using System.Threading;

namespace CorePush.Apple
{
    public class ApnSettings
    {
        private int _DataDirtyNonce = 0;
        private string _P8PrivateKey;
        private string _P8PrivateKeyId;

        /// <summary>
        /// p8 certificate string
        /// </summary>
        public string P8PrivateKey
        {
            get { return _P8PrivateKey; }
            set
            {
                _P8PrivateKey = value;
                Interlocked.Increment(ref _DataDirtyNonce);
            }
        }

        /// <summary>
        /// 10 digit p8 certificate id. Usually a part of a downloadable certificate filename
        /// </summary>
        public string P8PrivateKeyId
        {
            get { return _P8PrivateKeyId; }
            set
            {
                _P8PrivateKeyId = value;
                Interlocked.Increment(ref _DataDirtyNonce);
            }
        }

        /// <summary>
        /// Apple 10 digit team id
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// App slug / bundle name
        /// </summary>
        public string AppBundleIdentifier { get; set; }

        /// <summary>
        /// Development or Production server
        /// </summary>
        public ApnServerType ServerType { get; set; }

        internal int GetDirtyNonce()
        {
            return _DataDirtyNonce;
        }
    }
}