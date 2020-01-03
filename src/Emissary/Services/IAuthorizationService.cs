using System;

namespace EmissaryCore
{
    public interface IAuthorizationService
    {
        string GetAccessToken(ulong discordId);
        bool AuthorizeUser(ulong discordId, string authCode);
    }
}