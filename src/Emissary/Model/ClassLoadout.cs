using System;

namespace Emissary
{
    public class ClassLoadout
    {
        public string Race { get; set; }  // human, awoken, exo
        public string Gender { get; set; }  // male, female 
        public string SubClass { get; set; }  // arc, solar, void
        public string SubClassTree { get; set; }  // e.g. well of radiance

        public ClassLoadout()
        {
        }

        public ClassLoadout(string race, string gender, string subClass, string subClassTree)
        {
            this.Race = race;
            this.Gender = gender;
            this.SubClass = subClass;
            this.SubClassTree = subClassTree;
        }
    }
}