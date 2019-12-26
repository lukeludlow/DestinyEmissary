using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmissaryCore
{
    public class DestinyItem
    {
        [Key]
        public long ItemInstanceId { get; set; }
        public string Name { get; set; }
        public IList<string> Categories { get; set; }

        // TODO: i want to get rid of the hashes in the model.
        // create a destiny item converter service or something that looks up the hashes in 
        // the manifest so that i can build the destiny item object with the string values
        public uint ItemHash { get; set; }
        public IList<uint> CategoryHashes { get; set; }

        public DestinyItem(long itemInstanceId, string name, IList<string> categories, uint itemHash, IList<uint> categoryHashes)
        {
            this.ItemInstanceId = itemInstanceId;
            this.Name = name;
            this.Categories = categories;
            this.ItemHash = itemHash;
            this.CategoryHashes = categoryHashes;
        }

        public DestinyItem()
        {
            this.Name = "";
            this.Categories = new List<string>();
            this.CategoryHashes = new List<uint>();
        }
    }
}