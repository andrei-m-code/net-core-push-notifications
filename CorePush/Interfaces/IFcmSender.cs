using System.Threading.Tasks;
using CorePush.Google;

namespace CorePush.Interfaces
{
    public interface IFcmSender
    {
        Task<FcmResponse> SendAsync(string deviceId, object payload);
        Task<FcmResponse> SendAsync(object payload);
    }
}