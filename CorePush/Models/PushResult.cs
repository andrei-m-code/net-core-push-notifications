namespace CorePush.Models;

public record PushResult(
    int StatusCode,
    bool IsSuccessStatusCode,
    string Message,
    string Error);