using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTI2IMG
{
    public class SubPage
    {
        public string PN = "";
        public string SC = "";
        public string PS = "";
        public string RE = "";
        public List<List<byte>> OL = Enumerable.Repeat(new List<byte>(), 26).ToList();

        public int[,] level1PageArray;
        public CharacterSets characterSets = new();
        public bool displayRow24 = true;
        public bool displayRow25 = false;
        int G0andG2 = 0x00;

        public int pageNumber = 0;
        public int subPageNumber = 0;

        public void LoadTTIdata()
        {

            if (!string.IsNullOrWhiteSpace(PN))
            {
                pageNumber = Convert.ToInt32(PN[..3], 16);
            }

            if (!string.IsNullOrWhiteSpace(SC))
            {
                subPageNumber = Convert.ToInt32(this.SC, 16);
            }

            var fileRegion = 0;

            if (!string.IsNullOrWhiteSpace(RE))
            {
                var reParseSuccessfull = Int32.TryParse(this.RE, out fileRegion);
                if (!reParseSuccessfull || fileRegion < 0 || fileRegion > 15)
                {
                    fileRegion = 0;
                }
            }

            if (!string.IsNullOrWhiteSpace(PS))
            {
                var filePageStatus = Convert.ToInt32(this.PS, 16);
                var fileNOS = ((filePageStatus & 0x200) >> 9) | ((filePageStatus & 0x100) >> 7) | ((filePageStatus & 0x80) >> 5);
                G0andG2 = (fileRegion << 3) | fileNOS;
            }

            var longestLine = OL.OrderByDescending(s => s.Count).First().Count;

            level1PageArray = GetNew2DArray(26, longestLine > 39 ? longestLine : 40, 0x20);

            for (var row = 0; row < 26; row++)
            {

                if (OL[row].Count == 0)
                {
                    continue;
                }

                if (row == 25)
                {
                    displayRow25 = true;
                }

                var column = 0;

                for (var offset = 0; offset < OL[row].Count; offset++)
                {
                    var charcode = (int)OL[row][offset];

                    if (charcode == 0x0D)
                    {
                        charcode = 0x20;
                    }
                        

                    if ((charcode & 0x80) > 0)
                    {
                        charcode &= 0x7F;
                    }
                    else if (charcode == 0x1B)
                    {
                        offset++;
                        charcode = ((int)OL[row][offset]) - 0x40;
                    }
                    if (row == 0 && column < 8)
                    {
                        charcode = 0x20;
                    }

                     level1PageArray[row, column] = charcode;

                    column++;
                }

            }

            characterSets.SetCharacterSets(G0andG2);
        }

        public static T[,] GetNew2DArray<T>(int x, int y, T initialValue)
        {
            T[,] nums = new T[x, y];
            for (int i = 0; i < x * y; i++) nums[i % x, i / x] = initialValue;
            return nums;
        }
    }
}

