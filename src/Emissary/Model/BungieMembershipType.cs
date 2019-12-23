using System;
using System.ComponentModel;

namespace Emissary
{
    public static class BungieMembershipType
    {
        // "All" is only valid for searching capabilities: you need to pass the actual matching BungieMembershipType for
        // any query where you pass a known membershipId.
        public static readonly int All = -1;
        public static readonly int None = 0;
        public static readonly int Xbox = 1;
        public static readonly int Psn = 2;
        public static readonly int Steam = 3;
        public static readonly int BungieNext = 254;
    }
}