using System;
using System.ComponentModel;

namespace Emissary
{
    /// <summary>
    /// Represents the possible components that can be returned from Destiny "Get" calls such as GetProfile,
    /// GetCharacter, GetVendor etc... 
    /// https://bungie-net.github.io/multi/schema_Destiny-DestinyComponentType.html#schema_Destiny-DestinyComponentType
    public static class DestinyComponentType
    {
        public static readonly int None = 0;
        public static readonly int Profiles = 100;
        public static readonly int ProfileInventories = 102;
        public static readonly int Characters = 200;
        public static readonly int CharacterInventories = 201;
        public static readonly int CharacterEquipment = 205;
        public static readonly int ItemInstances = 300;
        public static readonly int ItemPerks = 302;
        public static readonly int ItemStats = 304;
        public static readonly int ItemCommonData = 307;
    }
}