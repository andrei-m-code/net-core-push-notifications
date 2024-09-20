namespace CorePush.Models;

public record CodePushResponse(
    int StatusCode,
    bool IsSuccessStatusCode,
    string Message,
    string Error);