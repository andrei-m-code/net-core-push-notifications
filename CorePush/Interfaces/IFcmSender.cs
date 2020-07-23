using System;
using System.Threading.Tasks;
using CorePush.Google;

namespace CorePush.Interfaces
{
    public interface IFcmSender : IDisposable
    {
        Task<FcmResponse> SendAsync(string deviceId, object payload);
    }
}