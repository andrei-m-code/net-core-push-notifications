namespace CorePush.Firebase;

/// <summary>
/// Represents the response from the Firebase Cloud Messaging HTTP v1 API.
/// </summary>
public class FirebaseResponse
{
    /// <summary>
    /// The identifier of the sent message in the format "projects/*/messages/{message_id}".
    /// Populated on success; null on failure.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Error details returned by FCM when the request fails. Null on success.
    /// </summary>
    public FirebaseError Error { get; set; }
}
