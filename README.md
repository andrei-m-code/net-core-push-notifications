[![Build Status](https://travis-ci.org/andrei-m-code/net-core-push-notifications.svg?branch=master)](https://travis-ci.org/andrei-m-code/net-core-push-notifications) [![NuGet](https://img.shields.io/nuget/v/CorePush.svg)](https://www.nuget.org/packages/CorePush/)


# .NET Core Push Notifications for Android and iOS
CorePush is a simple .NET Core library for sending Push Notifications for Android Firebase (FCM) and iOS (APN) with JWT HTTP/2 API. It's very lightweight and only has basic functionality. Please contribute or open github issue if you need additional features. Thank you for using it!

## Installation

### NuGet Package

The easiest way would be to use [nuget](https://www.nuget.org/packages/CorePush) package.

dotnet cli:
```
dotnet add package CorePush
```

Package Manager Console:
```
Install-Package CorePush
```

### Setup for ASP.NET Core and/or other Dependency Injection

Both `ApnSender` and `FcmSender` have dependencies that need to be registered in order to enable DI. 

1. Register HttpClient in Startup.cs:

```
services.AddHttpClient();
```

2. Register settings as a singleton:

If you've added ApnSettings and FcmSettings into a configuration section, you can bind section directly to settings object from `IConfiguration` available in Startup.cs:

```
var section = configuration.GetSection("ApnSettings");
var settings = new AppSettings();
section.Bind(settings);
```

Add settings to services:
```
services.AddSingleton(apnSettings);
services.AddSingleton(fcmSettings);
```

# Firebase Cloud Messages for Android and iOS

For Firebase messages (aka FCM) we will need project Server Key and Sender ID. To find Server Key and Sender ID go to Firebase Console (https://console.firebase.google.com), select your project, then go to project settings -> cloud messaging. You should be able to find everything you need there. Here is a simple example of how you send Firebase notification:

```csharp
var fcm = new FcmSender(settings, httpClient);
await fcm.SendAsync(deviceToken, notification);
```
If you want to use Firebase to send iOS notifications, please checkout this article: https://firebase.google.com/docs/cloud-messaging/ios/certs.
The library serializes notification object to JSON using Newtonsoft.Json library and sends it to Google cloud. Here is more details on the expected payloads for FCM https://firebase.google.com/docs/cloud-messaging/concept-options#notifications. Please note, we are setting the "to" property to use device token, so you don't have to do it yourself.

# Apple Push Notifications

To send notifications to Apple devices you have to create a publisher profile and pass settings object with necessary parameters to ApnSender constructor. Apn Sender will create and sign JWT token and attach it to every request to Apple servers:
1. P8 private key - p8 certificate generated in itunes. Just 1 line string without spaces, ----- or line breaks.
2. Private key id - 10 digit p8 certificate id. Usually a part of a downloadable certificate filename e.g. AuthKey_IDOFYOURCR.p8</param>
3. Team id - Apple 10 digit team id from itunes
4. App bundle identifier - App slug / bundle name e.g.com.mycompany.myapp
5. Server type - Development or Production APN server

```csharp
var apn = new ApnSender(settings, httpClient);
await apn.SendAsync(notification, deviceToken);
```
**IMPORTANT**: Initialize 1 ApnSender per bundle. When you send many messages at once make sure to retry the sending in case of an error. If error happens it's recommended to retry the call after 1 second delay (await Task.Delay(1000)). Apple typically doesn't like to receive too many messages and will ocasionally respond with HTTP 429. From my experiance it happens once per 1000 requests.

Please see Apple notification format examples here: https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/CreatingtheNotificationPayload.html#//apple_ref/doc/uid/TP40008194-CH10-SW1.
Tip: To send properties like {"content-available": true} you can use Newtonsoft.Json attributes over C# properties like `[JsonProperty("content-available")]`.

# Examples of notification payload
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
Use `[JsonProperty("alert-type")]` attribute to serialize C# properties into JSON properties with dashes.

# Known Issues

* Doesn't seem like it's running on Mono (#55)[https://github.com/andrei-m-code/net-core-push-notifications/issues/55]

# MIT License

Copyright (c) 2020 Andrei M

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

