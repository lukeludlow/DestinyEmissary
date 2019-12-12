
namespace EmissaryApi.Model
{
    /// <summary>
    /// Represents the possible components that can be returned from Destiny "Get" calls such as GetProfile,
    /// GetCharacter, GetVendor etc... 
    /// https://bungie-net.github.io/multi/schema_Destiny-DestinyComponentType.html#schema_Destiny-DestinyComponentType
    public enum DestinyComponentType
    {
        None = 0,
        Profiles = 100,
        ProfileInventories = 102,
        Characters = 200,
        CharacterInventories = 201,
        CharacterEquipment = 205,
        ItemInstances = 300,
        ItemPerks = 302,
        ItemStats = 304,
        ItemCommonData = 307
    }
}