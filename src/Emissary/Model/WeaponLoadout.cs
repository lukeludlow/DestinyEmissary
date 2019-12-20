using System;

namespace Emissary
{
    public class WeaponLoadout
    {
        public Weapon KineticWeapon { get; set; }
        public Weapon EnergyWeapon { get; set; }
        public Weapon HeavyWeapon { get; set; }

        public WeaponLoadout()
        {
        }

        public WeaponLoadout(Weapon kineticWeapon, Weapon energyWeapon, Weapon heavyWeapon)
        {
            this.KineticWeapon = kineticWeapon;
            this.EnergyWeapon = energyWeapon;
            this.HeavyWeapon = heavyWeapon;
        }
    }
}