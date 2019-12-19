using System;

namespace Emissary.Model
{
    public class Loadout
    {
        public string Name { get; set; }

        public Weapon KineticWeapon { get; set; }
        public Weapon EnergyWeapon { get; set; }
        public Weapon HeavyWeapon { get; set; }

        public Armor HelmetArmor { get; set; }
        public Armor GauntletArmor { get; set; }
        public Armor ChestArmor { get; set; }
        public Armor LegArmor { get; set; }
        public Armor ClassItemArmor { get; set; }

        public CharacterClass CharacterClass { get; set; }

        public string ToJson()
        {
            throw new NotImplementedException();
        }
    }
}