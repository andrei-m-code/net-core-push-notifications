using CorePush.Google;
using System.Threading;
using System.Threading.Tasks;

namespace CorePush.Interfaces
{
    public interface IFcmSender
    {
        Task<FcmResponse> SendAsync(object payload, CancellationToken cancellationToken = default);
    }
}