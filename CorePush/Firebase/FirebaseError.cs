namespace CorePush.Firebase;

/// <summary>
/// Error information returned by the Firebase Cloud Messaging HTTP v1 API.
/// </summary>
public class FirebaseError
{
    /// <summary>
    /// Additional details about the error, such as the FCM-specific error code.
    /// </summary>
    public class Detail
    {
        /// <summary>
        /// The protobuf "@type" URL describing this detail entry.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The FCM-specific error code (e.g. "UNREGISTERED", "INVALID_ARGUMENT").
        /// See <see href="https://firebase.google.com/docs/cloud-messaging/send-message#rest">FCM error codes</see>.
        /// </summary>
        public string ErrorCode { get; set; }
    }

    /// <summary>
    /// The HTTP status code returned by FCM.
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// A human-readable error message describing what went wrong.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// The gRPC status string (e.g. "INVALID_ARGUMENT", "NOT_FOUND", "PERMISSION_DENIED").
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// An array of additional error detail objects with FCM-specific error codes.
    /// </summary>
    public Detail[] Details { get; set; }
}
