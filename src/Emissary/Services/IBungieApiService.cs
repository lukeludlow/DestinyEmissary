namespace EmissaryCore
{
    public interface IBungieApiService
    {
        OAuthResponse GetOAuthAccessToken(OAuthRequest request);
        UserMembershipsResponse GetMembershipsForUser(UserMembershipsRequest request);
        ProfileCharactersResponse GetProfileCharacters(ProfileCharactersRequest request);
        CharacterEquipmentResponse GetCharacterEquipment(CharacterEquipmentRequest request);
        EquipItemsResponse EquipItems(EquipItemsRequest request);
    }
}