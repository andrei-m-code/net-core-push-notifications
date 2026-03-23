[![NuGet Version](https://badge.fury.io/nu/CorePush.svg)](https://badge.fury.io/nu/CorePush)


# .NET Core Push Notifications for Web, Android and iOS
Send notifications to:
- **iOS** - Apple Push Notifications (via Latest Apple Push Notifications HTTP2 JWT API)
- **Android** - via Firebase Cloud Messaging (via Latest Firebase HTTP v1 API)
- **Web** - via Firebase Cloud Messaging (via Latest Firebase HTTP v1 API)

CorePush is a simple lightweight library with **no external dependencies**. It uses built-in .NET cryptography for JWT token generation and signing. Send notifications to Android and Web using Firebase Cloud Messaging and iOS APN with JWT HTTP/2 API.

Both `ApnSender` and `FirebaseSender` are thread safe.

# Installation - NuGet

| Package Version | .NET Version |
|---|---|
| v5.0.0+ | .NET 10 |
| v4.4.0 | .NET 9 |
| v4.3.0 | .NET 8 |

For earlier versions please use v3.1.1 of the library as it targets netstandard2.0, though please note, it uses the legacy FCM send API.

The easiest way to get started with CorePush is to use [NuGet](https://www.nuget.org/packages/CorePush) package.

dotnet cli:
```
dotnet add package CorePush
```

Package Manager Console:
```
Install-Package CorePush
```

Check out the Tester project [Program.cs](https://github.com/andrei-m-code/net-core-push-notifications/blob/master/CorePush.Tester/Program.cs) for a quick getting started example.

# Firebase Cloud Messages for Android, iOS and Web

To start sending Firebase messages you need a Service Account JSON key file from your Google project:
1. Go to [Firebase Console](https://console.firebase.google.com) > Project Settings > Service Accounts.
2. Click "Generate new private key" to download the JSON key file.
3. Use the JSON file contents to configure `FirebaseSender` either by deserializing it into `FirebaseSettings` or by passing the JSON string directly into the constructor.

Sending messages:

```csharp
var firebaseSettingsJson = await File.ReadAllTextAsync("./link/to/my-project-123345-e12345.json");
var fcm = new FirebaseSender(firebaseSettingsJson, httpClient);
var result = await fcm.SendAsync(payload);

if (!result.IsSuccessStatusCode)
{
    Console.WriteLine($"Firebase error: {result.Error} - {result.Message}");
}
```

Useful links:
- Message formats: https://firebase.google.com/docs/cloud-messaging/customize-messages/set-message-type
- Migrating from legacy API: https://firebase.google.com/docs/cloud-messaging/migrate-v1

## Firebase iOS notifications
If you want to use Firebase to send iOS notifications, please check out this article: https://firebase.google.com/docs/cloud-messaging/ios/certs.
The library serializes the notification object to JSON and sends it to Google Cloud. See the expected payload formats for FCM here: https://firebase.google.com/docs/cloud-messaging/concept-options#notifications.

## Firebase Notification Payload Example

```json
{
  "message": {
    "token": "DEVICE_TOKEN",
    "notification": {
      "title": "Match update",
      "body": "Arsenal goal in added time, score is now 3-0"
    },
    "android": {
      "ttl": "86400s",
      "notification": {
        "click_action": "OPEN_ACTIVITY_1"
      }
    },
    "apns": {
      "headers": {
        "apns-priority": "5"
      },
      "payload": {
        "aps": {
          "category": "NEW_MESSAGE_CATEGORY"
        }
      }
    },
    "webpush": {
      "headers": {
        "TTL": "86400"
      }
    }
  }
}
```

# Apple Push Notifications

To send notifications to Apple devices you need to create a push notification key in the Apple Developer portal and pass the settings to the `ApnSender` constructor. `ApnSender` will create and sign a JWT token and attach it to every request to Apple servers:
1. **P8 private key** - generated in the Apple Developer portal. Just the base64 content without headers, spaces, or line breaks.
2. **Private key id** - 10 digit p8 certificate id. Usually part of the downloadable certificate filename, e.g. AuthKey_IDOFYOURCR.p8
3. **Team id** - Apple 10 digit team id
4. **App bundle identifier** - e.g. com.mycompany.myapp
5. **Server type** - Development or Production APN server

```csharp
var settings = new ApnSettings
{
    AppBundleIdentifier = "com.mycompany.myapp",
    P8PrivateKey = "YOUR_P8_KEY_CONTENT",
    P8PrivateKeyId = "IDOFYOURCR",
    TeamId = "YOURTEAMID",
    ServerType = ApnServerType.Production
};

var apn = new ApnSender(settings, httpClient);
var result = await apn.SendAsync(notification, deviceToken);

if (!result.IsSuccessStatusCode)
{
    Console.WriteLine($"APN error: {result.Error} - {result.Message}");
}
```

Please see Apple notification payload examples here: https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/RemoteNotificationsPG/CreatingtheNotificationPayload.html#//apple_ref/doc/uid/TP40008194-CH10-SW1.
Tip: To send properties like `{"content-available": true}` you can use `System.Text.Json` attributes over C# properties like `[JsonPropertyName("content-available")]`.

## SendAsync Options

`SendAsync` supports optional parameters for fine-grained control:

```csharp
var result = await apn.SendAsync(
    notification,
    deviceToken,
    apnsId: "unique-notification-id",  // optional unique ID for the notification
    apnsExpiration: 0,                  // 0 = immediate delivery or discard
    apnsPriority: 10,                   // 10 = immediate, 5 = power-saving
    apnPushType: ApnPushType.Alert,     // Alert, Background, or Voip
    cancellationToken: ct);
```

For silent background notifications, use `ApnPushType.Background` with priority `5` and include `"content-available": 1` in your payload.

## Example of notification payload
You can find expected notification formats for different types of notifications in the Apple documentation. Here is a simple example of a visible notification (the one that appears in the phone's notification center):

```csharp
public class AppleNotification
{
    public class ApsPayload
    {
        public class Alert
        {
            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("body")]
            public string Body { get; set; }
        }

        [JsonPropertyName("alert")]
        public Alert AlertBody { get; set; }

        [JsonPropertyName("badge")]
        public int Badge { get; set; }
    }

    [JsonPropertyName("aps")]
    public ApsPayload Aps { get; set; }
}
```

Use `[JsonPropertyName("alert-type")]` attribute to serialize C# properties into JSON properties with dashes.

## Error Handling

Both `ApnSender` and `FirebaseSender` return a `PushResult`:

```csharp
public record PushResult(
    int StatusCode,
    bool IsSuccessStatusCode,
    string Message,
    string Error);
```

For APN errors, you can compare against `ApnsErrorReasons` constants:

```csharp
var result = await apn.SendAsync(notification, deviceToken);

if (result.Error == ApnsErrorReasons.BadDeviceToken)
{
    // Remove invalid token from your database
}
else if (result.Error == ApnsErrorReasons.TooManyRequests)
{
    // Retry after a delay
}
```

# HttpClient and Dependency Injection

Each `ApnSender` requires a **dedicated** `HttpClient` instance because it sets `BaseAddress` on the client. Do not share an `HttpClient` between multiple senders or other services. However, the underlying `HttpClientHandler` can be shared.

`IApnSender` and `IFirebaseSender` interfaces are provided for dependency injection:

```csharp
// Using IHttpClientFactory (recommended)
services.AddHttpClient<IApnSender, ApnSender>();
services.AddHttpClient<IFirebaseSender, FirebaseSender>();
```

If you send many APN messages at once, Apple may occasionally respond with HTTP 429. Wrap your calls in try/catch and retry as needed.

# Custom JSON Serializer

Both senders use `System.Text.Json` with camelCase naming by default. You can provide a custom serializer by implementing `IJsonSerializer`:

```csharp
public interface IJsonSerializer
{
    string Serialize(object obj);
    TObject Deserialize<TObject>(string json);
}
```

Pass your implementation to the sender constructor:

```csharp
var apn = new ApnSender(settings, httpClient, myCustomSerializer);
var fcm = new FirebaseSender(firebaseSettingsJson, httpClient, myCustomSerializer);
```

# Migrating from v4 to v5

v5.0.0 removes the BouncyCastle dependency. JWT token generation and signing now use built-in .NET cryptography (`System.Security.Cryptography`). No code changes are required on your end — the public API is unchanged.

Key changes:
- **Removed** BouncyCastle.Cryptography NuGet dependency
- **Target framework** upgraded from .NET 9 to .NET 10
- **Fixed** Base64URL encoding for JWT tokens
- **Fixed** potential null reference in APN error deserialization
- **Fixed** APN token cache key collision when using multiple key IDs
- **Added** device token validation in `ApnSender`
- **Added** `CancellationToken` propagation in `FirebaseSender`

# MIT License

Copyright (c) 2020 Andrei M

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
