
namespace Emissary.Model
{
    public class Loadout
    {
        public Weapon KineticWeapon { get; set; }

        public Loadout()
        {
            this.KineticWeapon = new Weapon("unknown", "unknown");
        }
    }
}