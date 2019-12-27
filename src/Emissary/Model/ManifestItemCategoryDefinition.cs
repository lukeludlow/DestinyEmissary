using Newtonsoft.Json;

namespace EmissaryCore
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class ManifestItemCategoryDefinition
    {
        [JsonProperty("displayProperties.name")]
        public string CategoryName { get; set; }

        public ManifestItemCategoryDefinition(string categoryName)
        {
            this.CategoryName = categoryName;
        }

        public ManifestItemCategoryDefinition()
        {
        }
    }
}