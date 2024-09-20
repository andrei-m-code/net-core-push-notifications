using System.Threading;
using System.Threading.Tasks;

using CorePush.Apple;
using CorePush.Models;

namespace CorePush.Interfaces;

public interface IApnSender
{
    Task<CodePushResponse> SendAsync(
        object notification,
        string deviceToken,
        string apnsId = null,
        int apnsExpiration = 0,
        int apnsPriority = 10,
        ApnPushType apnPushType = ApnPushType.Alert,
        CancellationToken cancellationToken = default);
}