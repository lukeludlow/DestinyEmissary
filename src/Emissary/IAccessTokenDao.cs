namespace EmissaryCore
{
    public interface IAccessTokenDao
    {
        void AddOrUpdateAccessToken(BungieAccessToken accessToken);
        BungieAccessToken GetAccessTokenByDiscordId(ulong discordId);
    }
}