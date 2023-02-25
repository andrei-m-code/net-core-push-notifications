namespace CorePush.Firebase
{
    public class FirebaseSettings
    {
        /// <summary>
        /// Google Project ID. E.g. your-project-123456
        /// </summary>
        public string GoogleProjectId { get; set; }

        /// <summary>
        /// FCM Service Account Bearer Token
        /// </summary>
        public string FcmBearerToken { get; set; }
    }
}