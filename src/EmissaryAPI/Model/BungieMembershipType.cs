
using System;
using System.ComponentModel;

namespace EmissaryApi.Model
{
    public static class BungieMembershipType
    {

        // "All" is only valid for searching capabilities: you need to pass the actual matching BungieMembershipType for
        // any query where you pass a known membershipId.
        public static readonly int All = -1;

        // "TigerSteam"
        public static readonly int Steam = 3;

        // None = 0,
        // Xbox = 1,
        // Psn = 2,
        // Blizzard = 4,
        // Stadia = 5,
        // Demon = 10,
        // BungieNext = 254,
    }
}