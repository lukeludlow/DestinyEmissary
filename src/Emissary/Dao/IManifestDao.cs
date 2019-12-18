using Emissary.Model;

namespace Emissary
{
    public interface IManifestDao
    {
        DestinyItem LookupItem(uint itemHash);
        DestinyItemCategory LookupItemCategory(uint itemCategoryHash);
    }
}