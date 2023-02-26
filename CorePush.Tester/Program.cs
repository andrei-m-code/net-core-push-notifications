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
        
    private const string fcmServiceAccountFilename = "/Users/andrei/Projects/FCMTokens/mobileinstein-174121-e899c9a53553-new.json";
    private const string fcmReceiverToken = "dAaOttQ1SlmtlP_b5yli5K:APA91bGDVKu8vk1a9_BG7KRcMI4YVPYqT7yZ4VXXZ7eiWu4vr1FYOf2-1LBTLae3PjscNPvSWtJZ03iE6mTrWNYFEoJy3QgwDUgMR7Mo0GJdXGj4eGlUEcIugKHub5Js122_8ox6n8RP";

    # endregion

    private static readonly HttpClient http = new();

    static async Task Main()
    {
        // await SendApnNotificationAsync();
        await SendFirebaseNotificationAsync();

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

    private static async Task SendFirebaseNotificationAsync()
    {
        var contents = await File.ReadAllTextAsync(fcmServiceAccountFilename);
        var serializer = new DefaultCorePushJsonSerializer();
        var settings = serializer.Deserialize<FirebaseSettings>(contents);
            
        var fcm = new FirebaseSender(settings, http);
        var payload = new
        {
            message = new
            {
                token = fcmReceiverToken,
                notification = new
                {
                    title = "Test",
                    body = "Test Body"
                }
            }
        };
        
        var response = await fcm.SendAsync(payload);
    }
}