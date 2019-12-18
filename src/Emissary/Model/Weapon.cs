namespace Emissary.Model
{
    public class Weapon
    {
        public string Name { get; private set; }  // weapon's name. e.g. "Izanagi's Burden"
        public string WeaponDamageType { get; private set; }  // kinetic, energy, heavy
        public string WeaponClass { get; private set; }  // sniper rifle, submachine gun, etc.

        public Weapon(string name, string weaponDamageType, string weaponClass)
        {
            this.Name = name;
            this.WeaponDamageType = weaponDamageType;
            this.WeaponClass = weaponClass;
        }
    }
}