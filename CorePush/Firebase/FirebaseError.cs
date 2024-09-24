namespace CorePush.Firebase;

public class FirebaseError
{
    public class Detail
    {
        public string Type { get; set; }
        public string ErrorCode { get; set; }
    }

    public int Code { get; set; }
    public string Message { get; set; }
    public string Status { get; set; }
    public Detail[] Details { get; set; }
}
