# CorePush
.NET Core Push Notifications to Android and iOS

# Installation

The easiest way would be to use nuget package https://www.nuget.org/packages/CorePush.

```
Install-Package CorePush
```

# Android GCM Notifications

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
using (var apn = new APNSender(GatewayType.Sandbox, "mycert.p12", "password", true))
{
    await apn.SendAsync(deviceToken, notification);
}
```

Please see Apple notification format examples here: https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/CreatingtheNotificationPayload.html#//apple_ref/doc/uid/TP40008194-CH10-SW1.
Tip: To send properties like {"content-available": true} you can use Newtonsoft.Json attributes over C# properties like [JsonProperty("content-available")].
