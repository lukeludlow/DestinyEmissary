using Newtonsoft.Json; 

namespace Emissary
{
    public class LoadoutDbEntity
    {
        public ulong DiscordID { get; set; }
        public string Name { get; set; }
        public string Json { get; set; }

        public LoadoutDbEntity()
        {
        }

        public LoadoutDbEntity(Loadout loadout)
        {
            this.DiscordID = loadout.DiscordID;
            this.Name = loadout.Name;
            this.Json = JsonConvert.SerializeObject(loadout);
        }

        public Loadout ToLoadout()
        {
            return JsonConvert.DeserializeObject<Loadout>(this.Json);
        }
    }
}