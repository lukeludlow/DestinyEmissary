using System;

namespace EmissaryCore
{
    public interface IAuthorizationService
    {
        string GetAccessToken(ulong discordId);
        OAuthResponse AuthorizeUser(ulong discordId, string authCode);
    }
}