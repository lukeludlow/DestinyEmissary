namespace Emissary
{
    public interface IBungieApiService
    {
        // TODO turn it into one method to get profile with all components, including characters list (to find most
        // recently played character) and all characters inventories
        DestinyProfileCharactersResponse GetCharacters(int membershipType, long destinyMembershipId);
        DestinyProfileCharacterEquipmentResponse GetEquipment(int membershipType, long destinyMembershipId, long characterId);
        // SearchDestinyPlayerResponse SearchDestinyPlayer(int membershipType, string displayName);
    }
}