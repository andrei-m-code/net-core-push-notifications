using System.Threading;
using System.Threading.Tasks;

using CorePush.Firebase;
using CorePush.Models;

namespace CorePush.Interfaces;

public interface IFirebaseSender
{
    Task<PushResult> SendAsync(object payload, CancellationToken cancellationToken = default);
}