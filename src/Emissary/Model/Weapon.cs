namespace Emissary.Model
{
    public class Weapon
    {
        public string Name { get; private set; }
        public string Type { get; private set; }

        public Weapon(string name, string type)
        {
            this.Name = name;
            this.Type = type;
        }
    }
}