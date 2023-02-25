using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace CorePush.Tester;

public class Utils
{
    public static async Task SendNotificationViaFirebaseSDKAsync(string pathToJsonServiceAccountFile, string token)
    {
        var credential = GoogleCredential.FromFile(pathToJsonServiceAccountFile);
        FirebaseApp.Create(new AppOptions {Credential = credential});

        var message = new Message
        {
            Notification = new Notification
            {
                Title = "Hello!",
                Body = "World!"
            },
            Token = token,
        };
        var messaging = FirebaseMessaging.DefaultInstance;
        var result = await messaging.SendAsync(message);
    }

    public static async Task GenerateFirebaseJWTAsync(string pathToJsonServiceAccountFile)
    {
        var credential = GoogleCredential.FromFile(pathToJsonServiceAccountFile);
        FirebaseApp.Create(new AppOptions {Credential = credential});

        var jwt = await credential
            .CreateScoped("https://www.googleapis.com/auth/firebase.messaging")
            .UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}