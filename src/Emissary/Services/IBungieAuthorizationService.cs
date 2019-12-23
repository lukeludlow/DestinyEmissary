namespace Emissary
{
    public interface IBungieAuthorizationService
    {
        void ReceiveAuthorizationCode(ulong discordId);
        void GetMembershipDataForCurrentUser();
        // maybe. BungieAuthorizationService and DestinyEquipmentService
    }
}