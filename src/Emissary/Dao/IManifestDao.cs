namespace EmissaryCore
{
    public interface IManifestDao
    {
        DestinyItem LookupItem(uint itemHash);
        // DestinyItemCategory LookupItemCategory(uint itemCategoryHash);
    }
}