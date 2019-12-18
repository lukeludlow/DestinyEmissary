namespace Emissary.Model
{
    public class CharacterClass
    {
        public string Race { get; private set; }  // human, awoken, exo
        public string Gender { get; private set; }  // male, female 
        public string SubClass { get; private set; }  // arc, solar, void
        public string SubClassTree { get; private set; }  // e.g. well of radiance

        public CharacterClass(string race, string gender, string subClass, string subClassTree)
        {
            this.Race = race;
            this.Gender = gender;
            this.SubClass = subClass;
            this.SubClassTree = subClassTree;
        }
    }
}