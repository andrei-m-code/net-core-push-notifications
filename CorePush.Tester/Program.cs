using System;
using System.Net.Http;
using System.Threading.Tasks;
using CorePush.Apple;
using CorePush.Google;

namespace CorePush.Tester
{
    class Program
    {
        #region Apn Sender Settings

        private const string apnBundleId = "TODO";
        private const string apnP8PrivateKey = "TODO";
        private const string apnP8PrivateKeyId = "TODO";
        private const string apnTeamId = "TODO";
        private const string apnDeviceToken = "TODO";
        private const ApnServerType apnServerType = ApnServerType.Development;

        #endregion

        #region FCM Sender Settings
        private const string fcmReceiverToken = "TODO";
        private const string fcmSenderId = "TODO";
        private const string fcmServerKey = "TODO";

        # endregion

        private static readonly HttpClient http = new HttpClient();

        static async Task Main()
        {
            //await SendApnNotificationAsync();
            //await SendFcmNotificationAsync();

            Console.WriteLine("Done!");
        }

        private static async Task SendApnNotificationAsync()
        {
            var settings = new ApnSettings
            {
                AppBundleIdentifier = apnBundleId,
                P8PrivateKey = apnP8PrivateKey,
                P8PrivateKeyId = apnP8PrivateKeyId,
                TeamId = apnTeamId,
                ServerType = apnServerType,
            };

            while (true)
            {
                var apn = new ApnSender(settings, http);
                var payload = new AppleNotification(
                    Guid.NewGuid(), 
                    "Hello World (Message)",
                    "Hello World (Title)");
                var response = await apn.SendAsync(payload, apnDeviceToken);
            }
        }

        private static async Task SendFcmNotificationAsync()
        {
            var settings = new FcmSettings
            {
                SenderId = fcmSenderId,
                ServerKey = fcmServerKey
            };

            var fcm = new FcmSender(settings, http);
            var payload = new 
            {
                notification = new { body = "Hello World!" }
            };

            var response = await fcm.SendAsync(fcmReceiverToken, payload);
        }
    }
}
