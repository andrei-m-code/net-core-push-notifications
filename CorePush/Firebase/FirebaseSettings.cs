using System.Text.Json.Serialization;

namespace CorePush.Firebase;

public record FirebaseSettings(
    [property: JsonPropertyName("project_id")] string ProjectId,
    [property: JsonPropertyName("private_key")] string PrivateKey,
    [property: JsonPropertyName("client_email")] string ClientEmail,
    [property: JsonPropertyName("token_uri")] string TokenUri
);