using System.Threading.Tasks;

namespace CorePush.Interfaces
{
    /// <summary>
    /// Notification sender
    /// </summary>
    public interface INotificationSender
    {
        Task SendAsync(string deviceId, object payload);
    }
}
