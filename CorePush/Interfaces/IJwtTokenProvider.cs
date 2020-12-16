using System;
using CorePush.Apple;

namespace CorePush.Interfaces
{
    public interface IJwtTokenProvider
    {
        string CreateJwtToken(ApnSettings settings);
        string GetJwtToken(ApnSettings settings);
    }
}
