namespace Emissary.Model
{
    public class Armor
    {
        public string Name { get; private set; }
        public string Type { get; private set; }  // helmet, gauntlets, chest, leg, cloak/mark/bond
        public string ClassType { get; private set; }  // hunter, titan, warlock

        public Armor(string name, string type, string classType)
        {
            this.Name = name;
            this.Type = type;
            this.ClassType = classType;

        }
    }
}