[![Build Status](https://travis-ci.org/andrei-m-code/net-core-push-notifications.svg?branch=master)](https://travis-ci.org/andrei-m-code/net-core-push-notifications) [![NuGet](https://img.shields.io/nuget/v/CorePush.svg)](https://www.nuget.org/packages/CorePush/)


# CorePush - .NET Core Android Firebase (FCM) and Apple iOS JWT HTTP/2 Push notifications (APN)
Simple .NET Core library for sending Push Notifications for Android Firebase (FCM) and iOS (APN) with JWT HTTP/2 API.

## Installation

The easiest way would be to use nuget package https://www.nuget.org/packages/CorePush.

```
Install-Package CorePush
```
## Firebase Notifications (Android and iOS)

For Firebase messages we will need project Server Key and Sender ID. If you are moving away from GCM, sender ID will stay the same and GCM subscribers will still be able to receive notifications sent through Firebase. To find Server Key and Sender ID go to Firebase Console (https://console.firebase.google.com), select your project, then go to project settings -> cloud messaging. You should be able to find everything you need there. Here is a simple example of how you send Firebase notification:

```csharp
using (var fcm = new FcmSender(serverKey, senderId))
{
    await fcm.SendAsync(deviceToken, notification);
}
```
If you want to use Firebase to send iOS notifications, please checkout this article: https://firebase.google.com/docs/cloud-messaging/ios/certs.
The library serializes notification object to JSON using Newtonsoft.Json library and sends it to Google cloud. Please see the docs on what you can send https://developers.google.com/cloud-messaging/http-server-ref#send-downstream and https://firebase.google.com/docs/cloud-messaging/concept-options#notifications. Please note, we are setting the "to" property to use device token, so you don't have to do it yourself.

## Apple Push Notifications

To send notifications to Apple devices you have to create a publisher profile and pass necessary parameters to ApnSender constructor. Apn Sender will create and sign JWT token and attach it to every request to Apple servers:
1. p8privateKey - p8 certificate generated in itunes. Just 1 line string without spaces, ----- or line breaks.
2. privateKeyId - 10 digit p8 certificate id. Usually a part of a downloadable certificate filename e.g. AuthKey_IDOFYOURCR.p8</param>
3. teamId - Apple 10 digit team id from itunes
4. appBundleIdentifier - App slug / bundle name e.g.com.myawesomecompany.helloworld
5. server - Development or Production APN server

```csharp
using (var apn = new ApnSender(p8privateKey, p8privateKeyId, teamId, appBundleIdentifier, server)) 
{
    await apn.SendAsync(deviceToken, notification);
}
```
**IMPORTANT 1**: Initialize 1 ApnSender per bundle, send messages and don't forget to dispose your object. When you send many messages at once make sure to retry the sending in case of an error. If error happens it's recommended to retry the call after 1 second delay (await Task.Delay(1000)). Apple typically doesn't like to receive too many messages and will ocasionally respond with HTTP 429. From my experiance it happens once per 1000 requests.

**IMPORTANT 2**: APN sender uses WinHttpHandler to send HTTP/2 requests which makes it usable only on Windows OS unfortunately. Once there is a cross-platform version or HttpClient will support HTTP/2, it will be migrated.

Please see Apple notification format examples here: https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/CreatingtheNotificationPayload.html#//apple_ref/doc/uid/TP40008194-CH10-SW1.
Tip: To send properties like {"content-available": true} you can use Newtonsoft.Json attributes over C# properties like [JsonProperty("content-available")].

## Examples of notification payloads
You can find expected notification formats for different types of notifications in the documentation. To make it easier to get started, here is a simple example of visible notification (the one that you'll see in phone's notification center) for iOS and Android:

```csharp
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
## Please contribute
This is a very simple library that only supports basic functionality. So contributions are very very welcome!

