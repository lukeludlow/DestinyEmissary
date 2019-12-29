using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmissaryCore
{
    public class Loadout
    {
        public ulong DiscordId { get; set; }
        public long DestinyCharacterId { get; set; }
        public string LoadoutName { get; set; }
        public IList<DestinyItem> Items { get; set; }

        public Loadout(ulong discordId, long destinyCharacterId, string loadoutName, IList<DestinyItem> items)
        {
            this.DiscordId = discordId;
            this.DestinyCharacterId = destinyCharacterId;
            this.LoadoutName = loadoutName;
            this.Items = items;
        }

        public Loadout()
        {
            this.DiscordId = default;
            this.DestinyCharacterId = default;
            this.LoadoutName = "";
            this.Items = new List<DestinyItem>();
        }

        public override string ToString()
        {
            string kineticWeapon = this.Items.Where(item => item.Categories.Contains("Kinetic Weapon")).Select(item => item.Name).FirstOrDefault();
            string energyWeapon = this.Items.Where(item => item.Categories.Contains("Energy Weapon")).Select(item => item.Name).FirstOrDefault();
            string powerWeapon = this.Items.Where(item => item.Categories.Contains("Power Weapon")).Select(item => item.Name).FirstOrDefault();
            string helmet = this.Items.Where(item => item.Categories.Contains("Helmets")).Select(item => item.Name).FirstOrDefault();
            string arms = this.Items.Where(item => item.Categories.Contains("Arms")).Select(item => item.Name).FirstOrDefault();
            string chest = this.Items.Where(item => item.Categories.Contains("Chest")).Select(item => item.Name).FirstOrDefault();
            string legs = this.Items.Where(item => item.Categories.Contains("Legs")).Select(item => item.Name).FirstOrDefault();
            string classItem = this.Items.Where(item => item.Categories.Contains("Class Items")).Select(item => item.Name).FirstOrDefault();
            StringBuilder sb = new StringBuilder();
            sb.Append($"\n**{this.LoadoutName}**\n");
            sb.Append($"weapons:\n");
            sb.Append($"- {kineticWeapon}\n");
            sb.Append($"- {energyWeapon}\n");
            sb.Append($"- {powerWeapon}\n");
            sb.Append($"armor:\n");
            sb.Append($"- {helmet}\n");
            sb.Append($"- {arms}\n");
            sb.Append($"- {chest}\n");
            sb.Append($"- {legs}\n");
            sb.Append($"- {classItem}\n");
            return sb.ToString();
        }
    }
}