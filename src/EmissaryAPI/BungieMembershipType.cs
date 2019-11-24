
namespace EmissaryApi
{
    public enum BungieMembershipType
    {
        None = 0,
        Xbox = 1,
        Psn = 2,
        Steam = 3,
        Blizzard = 4,
        Stadia = 5,
        Demon = 10,
        BungieNext = 254,
        // "All" is only valid for searching capabilities: you need to pass the actual matching BungieMembershipType for
        // any query where you pass a known membershipId.
        All = -1,
    }
}