namespace Emissary
{
    public interface IUserInfoAccessor
    {
        long LookupBungieId(ulong discordId);
        // TODO LookupLoadouts
    }
}