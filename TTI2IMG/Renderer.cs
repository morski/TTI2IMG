using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TTI2IMG;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TTI2IMG
{
    public class Renderer
    {
        readonly TTI tti;
        readonly CommandLineOptions options;
        readonly CellAttribute[][] screenAttributesArray = new CellAttribute[26][];
        readonly List<(int, int, (int, int, Rgba32))> pixelData = new();


        public Renderer(TTI tti, CommandLineOptions opts)
        {
            this.tti = tti;
            options = opts;
        }

        private void DecodePage(SubPage subPage)
        {
            /* initialise attributes array */
            for (var r = 0; r < 26; r++)
            {
                this.screenAttributesArray[r] = new CellAttribute[72];
                for (var c = 0; c < 72; c++)
                {
                    this.screenAttributesArray[r][c] = new CellAttribute(subPage.characterSets);
                }
            }

            /* read packets from level1PageArray and set screen attributes */
            var bottomrow = false;
            for (var r = 0; r < 26; r++)
            {
                if (r == 25 && !subPage.displayRow25)
                {
                    break; // don't display packet 25.
                }

                /* start of row defaults go here */
                var nextforecolor = 0x07;
                var backcolor = 0x20; // special black
                var nextconceal = false;
                var nextmosaics = false;
                var nextseparated = false;
                var nextdoubleheight = false;
                var nextdoublewidth = false;
                var nexthold = false;
                var nextheldchar = 0x20;
                var nextheldmode = false;
                var rowhasdoubleheight = false;
                for (var c = 0; c < 40; c++)
                {
                    

                    if (!bottomrow)
                    {
                        var forecolor = nextforecolor;
                        var mosaics = nextmosaics;
                        var separated = nextseparated;
                        var conceal = nextconceal;
                        var doubleheight = nextdoubleheight;
                        var doublewidth = nextdoublewidth;
                        var hold = nexthold;
                        var heldchar = nextheldchar;
                        var heldmode = nextheldmode;
                        var charcode = subPage.level1PageArray[r, c];

                        if (charcode < 0x20)
                        {
                            int nextbackcolor;
                            switch (charcode)
                            {
                                case 0x01:
                                case 0x02:
                                case 0x03:
                                case 0x04:
                                case 0x05:
                                case 0x06:
                                case 0x07:
                                    /* alphanumeric color codes */
                                    nextforecolor = charcode;
                                    nextmosaics = false; /* switch off mosaics */
                                    nextheldchar = 0x20; /* clear held mosaic */
                                    nextheldmode = false;
                                    nextconceal = false; /* switch off conceal */
                                    break;

                                case 0x0C:
                                    /* normal size */
                                    if (doubleheight || doublewidth)
                                    { /* size change */
                                        nextheldchar = 0x20;
                                        nextheldmode = false;
                                        heldchar = nextheldchar;
                                        heldmode = nextheldmode;
                                    }
                                    nextdoubleheight = false;
                                    nextdoublewidth = false;
                                    break;

                                case 0x0D:
                                    /* double height */
                                    if (!doubleheight)
                                    { /* size change */
                                        nextheldchar = 0x20;
                                        nextheldmode = false;
                                    }
                                    nextdoubleheight = true;
                                    nextdoublewidth = false;
                                    rowhasdoubleheight = true; // set flag for row
                                    break;

                                case 0x11:
                                case 0x12:
                                case 0x13:
                                case 0x14:
                                case 0x15:
                                case 0x16:
                                case 0x17:
                                    /* mosaic color codes */
                                    nextforecolor = charcode & 0x07;
                                    nextmosaics = true; /* switch on mosaics */
                                    nextconceal = false; /* switch off conceal */
                                    break;

                                case 0x18:
                                    /* Conceal */
                                    nextconceal = true;
                                    conceal = nextconceal; // set-at
                                    break;

                                case 0x19:
                                    /* contiguous mosaics */
                                    nextseparated = false;
                                    separated = nextseparated; // set-at
                                    break;

                                case 0x1A:
                                    /* separated mosaics */
                                    nextseparated = true;
                                    separated = nextseparated; // set-at
                                    break;

                                case 0x1C:
                                    /* black background */
                                    backcolor = 0x20; /* set-at */
                                    break;

                                case 0x1D:
                                    /* new background */
                                    nextbackcolor = forecolor;
                                    backcolor = nextbackcolor; /* set-at */
                                    break;

                                case 0x1E:
                                    /* hold mosaics */
                                    nexthold = true;
                                    hold = true; /* set-at */
                                    break;

                                case 0x1F:
                                    /* release mosaics */
                                    nexthold = false;
                                    break;
                            }

                            if (!hold)
                            {
                                heldchar = 0x20;
                            }

                        }
                        else if (mosaics)
                        { /* mosaics active */
                            if ((charcode & 0x20) > 0)
                            { /* bit 6 set */
                                nextheldchar = charcode;
                                heldchar = nextheldchar;
                                nextheldmode = separated; /* save separated-ness */
                                heldmode = nextheldmode;
                            }
                            else
                            {
                                separated = false; // don't underline blast through 
                            }
                        }
                        else
                        {
                            separated = false; // don't underline alphanumerics 
                        }

                        screenAttributesArray[r][c].charcode = charcode;
                        screenAttributesArray[r][c].heldchar = heldchar;
                        screenAttributesArray[r][c].heldmode = heldmode;
                        screenAttributesArray[r][c].forecolor = forecolor;
                        screenAttributesArray[r][c].backcolor = backcolor;
                        screenAttributesArray[r][c].mosaics = mosaics;
                        screenAttributesArray[r][c].dh = doubleheight;
                        screenAttributesArray[r][c].dw = doublewidth;
                        screenAttributesArray[r][c].rh = false; /* this is set later when rendering */
                        screenAttributesArray[r][c].charSet = subPage.characterSets.DefaultG0Set;
                        screenAttributesArray[r][c].NOS = subPage.characterSets.DefaultNOS;
                        screenAttributesArray[r][c].conceal = conceal;
                        screenAttributesArray[r][c].separated = separated;
                    }

                    screenAttributesArray[r][c].br = bottomrow;
                }

                bottomrow = rowhasdoubleheight;
            }
        }

        private void RedrawScreen()
        {
            for (var row = 0; row < 26; row++)
            {
                RenderRow(row);
            }
        }

        private void RenderRow(int row)
        {
            for (var column = 0; column < 40; column++)
            {
                RenderCharacter(row, column);
            }
        }

        private void RenderCharacter(int row, int column)
        {
            Dictionary<int, int> charCodeMap = new()
            {
                { 0x23, 0 }, { 0x24, 1 }, { 0x40, 2 }, { 0x5B, 3 }, { 0x5C, 4 },
                { 0x5D, 5 }, { 0x5E, 6 }, { 0x5F, 7 }, { 0x60, 8 }, { 0x7B, 9 },
                { 0x7C, 10 }, { 0x7D, 11 }, { 0x7E, 12 }
            };

            var cellAttributesRowColumn = screenAttributesArray[row][column];
            var r = row;
            var c = column;
            var italic = false;
            var bold = false;
            var br = cellAttributesRowColumn.br;


            if (c > 0 && br && screenAttributesArray[r][c].rh && !screenAttributesArray[r][c - 1].br)
            {
                screenAttributesArray[r][c].br = false;
                br = false;
                if (screenAttributesArray[r][c - 1].dh)
                {
                    screenAttributesArray[r][c].dh = true;
                    screenAttributesArray[r + 1][c].br = true;
                }
            }

            if (br) /* bottom row of double height */
                r = row - 1;

            var dh = screenAttributesArray[r][column].dh;
            var dw = screenAttributesArray[r][column].dw;
            var rh = screenAttributesArray[r][column].rh;

            if (dw)
                if (rh) /* right column of double width */
                    c = column - 1;
                else if (column < 39)
                {
                    screenAttributesArray[r][column + 1].rh = true;
                    screenAttributesArray[r][column + 1].dw = true;
                }

            if (dw && !rh && column == 39)
            {
                dw = false;
            }

            var cellAttributesRC = screenAttributesArray[r][c];
            var separated = cellAttributesRC.separated;
            var underline = cellAttributesRC.underline;

            var character = cellAttributesRC.charcode;
            var mosaicMode = cellAttributesRC.mosaics;


            if (character < 0x20)
            {
                character = cellAttributesRC.heldchar;
                if (character > 0x20)
                {
                    mosaicMode = true;
                    separated = cellAttributesRC.heldmode;
                }
                else
                {
                    mosaicMode = false;
                }
            }

            if (cellAttributesRC.conceal && character > 0x20)
            {
                character = 0x20; // display spaces for concealed characters when reveal mode is not active
            }

            if (br && !dh)
            {
                character = 0x20;
                underline = false;
            }

            List<Rgba32> charImage = new();

            if (character < 0x20)
            {
                if (rh)
                {
                    charImage = this.tti.charsetImageData[0][0]; // render space
                }
                else
                {
                    dw = false; /* don't be double width */
                }
                underline = false;
            }
            else
            {
                var charSet = cellAttributesRC.charSet;
                var NOS = cellAttributesRC.NOS;
                if (charSet < 13 && mosaicMode && (character < 0x40 || character >= 0x60))
                {
                    if (separated) // if mosaics and separated
                        charSet = 12; // use separated mosaics character set
                    else
                        charSet = 11; // contiguous
                }

                charImage = this.tti.charsetImageData[charSet][character - 0x20];

                if ((charSet == 0) && NOS > 0)
                {
                    if (charCodeMap.TryGetValue(character, out int index))
                    {
                        charImage = this.tti.NOSImageData[NOS - 1][index];
                    }
                }

                //SaveCharacter(charImage, character.ToString());

                if (charSet > 10)
                {
                    underline = false; // don't underline G1, G3 or DRCS characters
                }
                else
                {
                    italic = cellAttributesRC.italic;
                    bold = cellAttributesRC.bold;
                }
            }


            //var charImageData = charImage.SelectMany(BitConverter.GetBytes).ToArray(); // copy of character data

            var charImageData = new List<int>();

            if (underline)
            { // extra bit for underline from non spacing attributes
                for (var i = 108; i < 120; i++)
                {
                    charImageData[i] = 255; // add underline
                }
            }

            if (bold)
            {
                var charImageDataTemp = charImageData; // copy of character data

                for (var py = 0; py < 10; py++)
                {
                    for (var i = 0; i < 11; i++)
                    {
                        charImageData[py * 12 + i] = Convert.ToByte((charImageData[py * 12 + i] | charImageDataTemp[py * 12 + i + 1]).ToString("X"), 16);
                    }
                }
            }

            if (italic)
            {
                for (var py = 0; py < 3; py++)
                {
                    for (var i = 11; i >= 1; i--)
                    {
                        charImageData[py * 12 + i] = charImageData[py * 12 + i - 1];
                    }
                    charImageData[py * 12] = 0;
                }
                for (var py = 6; py < 10; py++)
                {
                    for (var i = 0; i < 11; i++)
                    {
                        charImageData[py * 12 + i] = charImageData[py * 12 + i + 1];
                    }
                    charImageData[py * 12 + 11] = 0;
                }
            }

            
            var shiftright = 0;
            var shiftdown = 0;
            var width = 1;
            var height = 1;
            if (dw)
            {
                width = 2;
                if (rh)
                {
                    shiftright = 6;
                }
            }
            if (dh)
            {
                height = 2;
                if (br)
                {
                    shiftdown = 5;
                }

            }

            var forecolor = Color.GetColor(cellAttributesRC.forecolor);
            var backcolor = Color.GetColor(cellAttributesRC.backcolor);

            int pr, pg, pb, pa;
            for (var py = 0; py < 10; py++)
            {
                for (var px = 0; px < 12; px++)
                {
                    if (charImage[((py / height) + shiftdown) * 12 + px / width + shiftright] == Rgba32.ParseHex("#fff"))
                    {
                        pr = forecolor[0];
                        pg = forecolor[1];
                        pb = forecolor[2];
                        pa = forecolor[3];
                    }
                    else
                    {
                        pr = backcolor[0];
                        pg = backcolor[1];
                        pb = backcolor[2];
                        pa = backcolor[3];
                    }

                    pixelData.Add((row, column, (py, px, new Rgba32(pr, pg, pb, pa))));
                }
            }
        }

        private void BlitScreen(SubPage subPage)
        {
            using Image<Rgba32> image = new(480, 260);

            foreach (var pixel in pixelData)
            {
                image[pixel.Item2 * 12 + pixel.Item3.Item2, pixel.Item1 * 10 + pixel.Item3.Item1] = pixel.Item3.Item3;
            }

            image.Mutate(x => x.Resize(options.Width, options.Height));

            var fileName = $"{options.OutputPath}/{subPage.pageNumber}-{subPage.subPageNumber}";

            switch(options.Format)
            {
                case FileFormats.PNG:
                    image.SaveAsPng($"{fileName}.png");
                    break;
                case FileFormats.JPG:
                    image.SaveAsJpeg($"{fileName}.jpeg");
                    break;
                case FileFormats.GIF:
                    image.SaveAsGif($"{fileName}.gif");
                    break;
                case FileFormats.BMP:
                    image.SaveAsBmp($"{fileName}.bmp");
                    break;
                case FileFormats.PBM:
                    image.SaveAsPbm($"{fileName}.pbm");
                    break;
                case FileFormats.TGA:
                    image.SaveAsTga($"{fileName}.tga");
                    break;
                case FileFormats.TIFF:
                    image.SaveAsTiff($"{fileName}.tiff");
                    break;
                case FileFormats.WEBP:
                    image.SaveAsWebp($"{fileName}.webp");
                    break;
            }
        }

        public void Render()
        {
            for (var index = 0; index < tti.subPages.Count; index++)
            {
                DecodePage(tti.subPages[index]);
                this.RedrawScreen();
                BlitScreen(tti.subPages[index]);
            }
        }
    }
}
