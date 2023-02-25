namespace CorePush.Tester;

public class FirebasePayload
{
    public FirebaseMessage Message { get; set; }
}

public class FirebaseMessage
{
    public string Token { get; set; }
    public FirebaseNotification Notification { get; set; }
    public object Data { get; set; }
}

public class FirebaseNotification
{
    public string Title { get; set; }
    public string Body { get; set; }
}