using System;

namespace Emissary
{
    public class Weapon
    {
        public string Name { get; set; }  // weapon's name. e.g. "Izanagi's Burden"
        public string WeaponDamageType { get; set; }  // kinetic, energy, heavy
        public string WeaponClass { get; set; }  // sniper rifle, submachine gun, etc.

        public Weapon()
        {
        }

        public Weapon(string name, string weaponDamageType, string weaponClass)
        {
            this.Name = name;
            this.WeaponDamageType = weaponDamageType;
            this.WeaponClass = weaponClass;
        }
    }
}