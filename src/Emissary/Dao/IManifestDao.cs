namespace EmissaryCore
{
    public interface IManifestDao
    {
        ManifestItemDefinition GetItemDefinition(uint itemHash);
        ManifestItemCategoryDefinition GetItemCategoryDefinition(uint categoryHash);
    }
}