namespace Emissary
{
    public interface IManifestAccessor
    {
        string LookupItem(uint itemHash);
        string LookupItemCategory(uint itemCategoryHash);
    }
}