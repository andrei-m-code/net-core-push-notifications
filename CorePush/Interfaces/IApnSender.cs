using System;
using System.Threading.Tasks;
using CorePush.Apple;

namespace CorePush.Interfaces
{
    public interface IApnSender
    {
        Task<ApnsResponse> SendAsync(
            AppleNotification notification,
            string deviceToken,
            string apnsId = null,
            long apnsExpiration = 0,
            int apnsPriority = 10,
            bool isBackground = false,
            int maxRetries = 0,
            IJwtTokenProvider jwtProvider = null);
    }
}