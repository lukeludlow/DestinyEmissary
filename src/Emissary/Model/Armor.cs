using System;
using System.ComponentModel.DataAnnotations;

namespace Emissary
{
    public class Armor
    {
        public string Name { get; set; }
        public string Type { get; set; }  // helmet, gauntlets, chest, leg, cloak/mark/bond
        public string ClassType { get; set; }  // hunter, titan, warlock

        public Armor()
        {
        }

        public Armor(string name, string type, string classType)
        {
            this.Name = name;
            this.Type = type;
            this.ClassType = classType;
        }
    }
}