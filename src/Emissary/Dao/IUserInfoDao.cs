namespace Emissary
{
    public interface IUserInfoDao
    {
        long GetBungieId(ulong discordId);
        string GetAllLoadouts(ulong discordId);
        string GetLoadout(ulong discordId, string loadoutName);
        // TODO use DestinyLoadout for these return times
    }
}