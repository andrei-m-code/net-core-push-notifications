using System;
using System.Net.Http;
using System.Threading.Tasks;
using CorePush.Google;

namespace CorePush.Tester
{
    class Program
    {
        private const string fcmReceiverToken = "TODO";
        private const string fcmSenderId = "TODO";
        private const string fcmServerKey = "TODO";

        static async Task Main()
        {
            await SendFcmNotificationAsync();

            Console.WriteLine("Done!");
        }

        private static async Task SendFcmNotificationAsync()
        {
            var settings = new FcmSettings
            {
                SenderId = fcmSenderId,
                ServerKey = fcmServerKey
            };

            var payload = new 
            {
                notification = new { body = "Hello World!" }
            };

            var fcm = new FcmSender(settings, new HttpClient());
            var response = await fcm.SendAsync(fcmReceiverToken, payload);
        }
    }
}
