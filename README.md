[![Build Status](https://travis-ci.org/andrei-m-code/CorePush.svg?branch=master)](https://travis-ci.org/andrei-m-code/CorePush) [![NuGet](https://img.shields.io/nuget/v/CorePush.svg)](https://www.nuget.org/packages/CorePush/)


# CorePush - .NET Core Android, iOS and Firebase push notifications
.NET Core Push Notifications for Android (GCM), Firebase and iOS.

# Installation

The easiest way would be to use nuget package https://www.nuget.org/packages/CorePush.

```
Install-Package CorePush
```
# Firebase Notifications (Android and iOS)

For Firebase messages we will need project Server Key and Sender ID. If you are moving away from GCM, sender ID will stay the same and GCM subscribers will still be able to receive notifications sent through Firebase. To find Server Key and Sender ID go to Firebase Console (https://console.firebase.google.com), select your project, then go to project settings -> cloud messaging. You should be able to find everything you need there. Here is a simple example of how you send Firebase notification:

```
using (var fcm = new FCMSender(serverKey, senderId))
{
    await fcm.SendAsync(deviceToken, notification);
}
```
If you want to use Firebase to send iOS notifications, please checkout this article: https://firebase.google.com/docs/cloud-messaging/ios/certs.

# Android GCM Notifications (will be shut down in spring 2019 - use Firebase instead)

In Google Cloud Developer Console in Library https://console.cloud.google.com/apis/library find and enable "Google Cloud Messaging". You will need Api Key and Sender ID. Your Android app must be configured to receive GCM push notifications. It should provide it's device token to the server. When you have it all, you can start sending remote push notifications to this device:

```
using (var gcm = new GCMSender(apiKey, senderId))
{
    await gcm.SendAsync(deviceToken, notification);
}
```

The library serializes notification object to JSON using Newtonsoft.Json library and sends it to Google cloud. Please see the docs on what you can send https://developers.google.com/cloud-messaging/http-server-ref#send-downstream. Please note, we are setting the "to" property, so you don't have to do it yourself.

# Apple Push Notifications

To send notifications to Apple devices you have to create push notifications certificate in apple developer portal. Create myapp.p12 certificate with password for the app that you want to send notificatins to. You can use sandbox or production certificates. The device has to register to receive push notifications and get push notifications device token. This token has to be forwarded to your backend so that you can send notifications to this device. Once you have everything ready:

```
var apn = new APNSender(GatewayType.Sandbox, "mycert.p12", "password", true)
await apn.SendAsync(deviceToken, notification);
```

Please see Apple notification format examples here: https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/CreatingtheNotificationPayload.html#//apple_ref/doc/uid/TP40008194-CH10-SW1.
Tip: To send properties like {"content-available": true} you can use Newtonsoft.Json attributes over C# properties like [JsonProperty("content-available")].

# Examples of notification payloads
You can find expected notification formats for different types of notifications in the documentation. To make it easier to get started, here is a simple example of visible notification (the one that you'll see in phone's notification center) for iOS and Android:

```
    public class GoogleNotification
    {
        public class DataPayload
        {
            // Add your custom properties as needed

            [JsonProperty("message")]
            public string Message { get; set; }
        }

        [JsonProperty("priority")]
        public string Priority { get; set; } = "high";

        [JsonProperty("data")]
        public DataPayload Data { get; set; }
    }

    public class AppleNotification
    {
        public class ApsPayload
        {
            [JsonProperty("alert")]
            public string AlertBody { get; set; }
        }

        // Your custom properties as needed

        [JsonProperty("aps")]
        public ApsPayload Aps { get; set; }
    }
```
# Please contribute
This is a very simple library that only supports basic functionality. So contributions are very welcome!

