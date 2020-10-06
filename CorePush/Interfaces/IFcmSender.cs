using CorePush.Google;
using System.Threading.Tasks;

namespace CorePush.Interfaces
{
    public interface IFcmSender
    {
        Task<FcmResponse> SendAsync(string deviceId, object payload);
    }
}