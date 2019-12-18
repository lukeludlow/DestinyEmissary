using Emissary.Model;

namespace Emissary
{
    public interface IManifestAccessor
    {
        DestinyItem LookupItem(uint itemHash);
        DestinyItemCategory LookupItemCategory(uint itemCategoryHash);
    }
}