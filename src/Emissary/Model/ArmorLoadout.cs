using System;

namespace Emissary
{
    public class ArmorLoadout
    {
        public Armor Helmet { get; set; }
        public Armor Gauntlets { get; set; }
        public Armor Chest { get; set; }
        public Armor Legs { get; set; }
        public Armor ClassItem { get; set; }

        public ArmorLoadout()
        {
        }

        public ArmorLoadout(Armor helmet, Armor gauntlets, Armor chest, Armor legs, Armor classItem)
        {
            this.Helmet = helmet;
            this.Gauntlets = gauntlets;
            this.Chest = chest;
            this.Legs = legs;
            this.ClassItem = classItem;
        }
    }
}