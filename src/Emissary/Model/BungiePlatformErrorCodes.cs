namespace EmissaryCore
{
    /// <summary>
	/// https://bungie-net.github.io/multi/schema_Exceptions-PlatformErrorCodes.html#schema_Exceptions-PlatformErrorCodes
	/// </summary>
    public class BungiePlatformErrorCodes
    {
        public static readonly int None = 0;
        public static readonly int Success = 1;
        public static readonly int DestinyAccountNotFound = 1601;
        public static readonly int DestinyItemNotFound = 1623;
        public static readonly int DestinyItemUniqueEquipRestricted = 1641;
    }
}