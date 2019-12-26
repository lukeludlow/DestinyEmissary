using System.ComponentModel.DataAnnotations;

namespace EmissaryCore
{
    public class DestinyItem
    {
        [Key]
        public long ItemInstanceId { get; set; }
        public string Name { get; set; }
        public string[] Categories { get; set; }

        // TODO: i want to get rid of the hashes in the model.
        // create a destiny item converter service or something that looks up the hashes in 
        // the manifest so that i can build the destiny item object with the string values
        public uint ItemHash { get; set; }
        public uint[] CategoryHashes { get; set; }
    }
}