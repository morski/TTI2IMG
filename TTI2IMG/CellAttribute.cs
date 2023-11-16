using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTI2IMG
{
    public class CellAttribute
    {
        public int charcode = 0x20;
        public int heldchar = 0x20;
        public bool heldmode = false;
        public int forecolor = 0x07;
        public int backcolor = 0x20;
        public bool mosaics = false;
        public bool dh = false;
        public bool dw = false;
        public bool rh = false;
        public bool br = false;
        public int charSet;
        public int NOS;
        public bool conceal = false;
        public bool separated = false;
        public bool underline = false;
        public bool proportional = false;
        public bool bold = false;
        public bool italic = false;

        public CellAttribute(CharacterSets characterSets)
        {
            charSet = characterSets.DefaultG0Set;
            NOS = characterSets.DefaultNOS;
        }
    }
}