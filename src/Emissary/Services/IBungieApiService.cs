namespace EmissaryCore
{
    public interface IBungieApiService
    {
        OAuthResponse GetOAuthAccessToken(OAuthRequest request);
        OAuthResponse RefreshAccessToken(string refreshToken);
        UserMembershipsResponse GetMembershipsForUser(UserMembershipsRequest request);
        ProfileCharactersResponse GetProfileCharacters(ProfileCharactersRequest request);
        CharacterEquipmentResponse GetCharacterEquipment(CharacterEquipmentRequest request);
        EquipItemsResponse EquipItems(EquipItemsRequest request);
    }
}