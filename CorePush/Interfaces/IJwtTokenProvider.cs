using System;
using CorePush.Apple;

namespace CorePush.Interfaces
{
    public interface IJwtTokenProvider
    {
        void ClearJwtToken(ApnSettings settings);
        string CreateJwtToken(ApnSettings settings);
        string GetJwtToken(ApnSettings settings);
    }
}
