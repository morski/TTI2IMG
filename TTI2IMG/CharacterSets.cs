using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TTI2IMG
{
    public class CharacterSets
    {
        public int DefaultG0Set = 0;
        public int DefaultNOS = 0;
        readonly Dictionary<int, CharacterSetMapping> CharacterSetMappings = new()
        {
            { 0x00, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 2 }}, // English
            { 0x01, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 5 }}, // German
            { 0x02, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 12 }}, // Swedish/Finnish/Hungarian
            { 0x03, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 6 }}, // Italian
            { 0x04, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 4 }}, // French
            { 0x05, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 9 }}, // Portuguese/Spanish
            { 0x06, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 1 }}, // Czech/Slovak
            { 0x08, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 8 }}, // Polish
            { 0x16, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 13 }}, // Turkish
            { 0x1D, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 11 }}, // Serbian/Croatian/Slovenian
            { 0x1F, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 10 }}, // Romanian
            { 0x20, new CharacterSetMapping { DefaultG0Set = 1, DefaultNOS = 0 }}, // Serbian/Croatian
            { 0x22, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 3 }}, // Estonian
            { 0x23, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 7 }}, // Latvian/Lithuanian
            { 0x24, new CharacterSetMapping { DefaultG0Set = 2, DefaultNOS = 0 }}, // Russian/Bulgarian
            { 0x25, new CharacterSetMapping { DefaultG0Set = 3, DefaultNOS = 0 }}, // Ukrainian
            { 0x37, new CharacterSetMapping { DefaultG0Set = 4, DefaultNOS = 0 }}, // Greek
            { 0x40, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 2 }}, // English
            { 0x44, new CharacterSetMapping { DefaultG0Set = 0, DefaultNOS = 4 }}, // French
            { 0x47, new CharacterSetMapping { DefaultG0Set = 5, DefaultNOS = 0 }}, // Arabic G0
            { 0x55, new CharacterSetMapping { DefaultG0Set = 6, DefaultNOS = 0 }}, // Hebrew G0
            { 0x57, new CharacterSetMapping { DefaultG0Set = 5, DefaultNOS = 0 }}  // Arabic G0 (duplicate)
        };

        public void SetCharacterSets(int defaultG0andG2)
        {
            var defaultValues = CharacterSetMappings[defaultG0andG2] ?? CharacterSetMappings[0x00]; // Use English as default

            DefaultG0Set = defaultValues.DefaultG0Set;
            DefaultNOS = defaultValues.DefaultNOS;
        }
    }

    internal  class CharacterSetMapping
    {
        public int DefaultG0Set { get; set; }
        public int DefaultNOS { get; set; }
    }
}
