using System.Text.Json.Serialization;

namespace CorePush.Firebase;

/// <summary>
/// Configuration settings for Firebase Cloud Messaging (FCM) HTTP v1 API.
/// This record maps directly to the Google Service Account JSON key file
/// (e.g. myproject-12345-abc123123.json) downloaded from the Firebase Console.
/// </summary>
/// <param name="ProjectId">The Firebase/GCP project ID (e.g. "myproject-12345").</param>
/// <param name="PrivateKey">The PEM-encoded RSA private key from the service account key file.</param>
/// <param name="ClientEmail">The service account email address (e.g. "firebase-adminsdk-xxxxx@myproject.iam.gserviceaccount.com").</param>
/// <param name="TokenUri">The OAuth 2.0 token endpoint URL, typically "https://oauth2.googleapis.com/token".</param>
public record FirebaseSettings(
    [property: JsonPropertyName("project_id")] string ProjectId,
    [property: JsonPropertyName("private_key")] string PrivateKey,
    [property: JsonPropertyName("client_email")] string ClientEmail,
    [property: JsonPropertyName("token_uri")] string TokenUri
);
