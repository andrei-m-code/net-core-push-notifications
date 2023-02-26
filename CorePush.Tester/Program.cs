using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using CorePush.Apple;
using CorePush.Firebase;
using CorePush.Serialization;

namespace CorePush.Tester;

class Program
{
    #region Apn Sender Settings

    private const string apnBundleId = "TODO";
    private const string apnP8PrivateKey = "TODO";
    private const string apnP8PrivateKeyId = "TODO";
    private const string apnTeamId = "TODO";
    private const string apnDeviceToken = "TODO";
    private const ApnServerType apnServerType = ApnServerType.Production;

    #endregion

    #region FCM Sender Settings
        
    private const string fcmServiceAccountFilename = "TODO";
    private const string fcmReceiverToken = "TODO";
        
    # endregion

    private static readonly HttpClient http = new();

    static async Task Main()
    {
        //await SendApnNotificationAsync();
        await SendFcmNotificationAsync();

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
        var contents = await File.ReadAllTextAsync(fcmServiceAccountFilename);
        var serializer = new DefaultCorePushJsonSerializer();
        var settings = serializer.Deserialize<FirebaseSettings>(contents);
            
        var fcm = new FirebaseSender(settings, http);
        var payload = new FirebasePayload
        {
            Message = new FirebaseMessage
            {
                Token = fcmReceiverToken,
                Notification = new FirebaseNotification
                {
                    Title = "Test",
                    Body = "Test Body"
                }
            }
        };
        
        var response = await fcm.SendAsync(payload);
    }
}