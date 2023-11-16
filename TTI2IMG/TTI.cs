using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace TTI2IMG
{
    public class TTI
    {
        public List<SubPage> subPages = new();
        public List<List<List<Rgba32>>> charsetImageData = new();
        public List<List<List<Rgba32>>> NOSImageData = new();
    
        public async Task Parse(byte[] fileData) {
            var fileString = Encoding.GetEncoding("iso-8859-1").GetString(fileData);
            var subPagesAsString = Regex.Split(fileString, @"(?=PN,)").ToList();

            var byteList = fileData.ToList();
            byteList.RemoveAll(b => b == 13 || b == 10);
            var byteCounter = 0;

            for(var i = 0; i < subPagesAsString.Count; i++) {

                if (!subPagesAsString[i].StartsWith("PN,"))
                {
                    byteCounter += subPagesAsString[i].Replace("\r", "").Replace("\n", "").Length;
                    continue;
                }

                var subPage = new SubPage();
                var lines = subPagesAsString[i].Replace("\r", "").Split('\n').Where(l => !string.IsNullOrEmpty(l)).ToArray();
                for (var line = 0; line < lines.Length; line++){
                    var tag = lines[line][..3];
                    switch (tag){
                        case "PN,":
                            subPage.PN = lines[line][3..];
                            break;
                        case "SC,":
                            subPage.SC = lines[line][3..];
                            break;
                        case "PS,":
                            subPage.PS = lines[line][3..];
                            break;
                        case "RE,":
                            subPage.RE = lines[line][3..];
                            break;
                        case "OL,":
                            var row = int.Parse(lines[line].Split(",")[1]);
                            if (row < 26){
                                var nextComma = byteList.Skip(byteCounter + 3).ToList().FindIndex(f => f == 44) + 1;
                                var rowBytes = byteList.Skip(byteCounter + 3 + nextComma).Take(lines[line].Skip(3 + nextComma).Count()).ToList();
                                subPage.OL.Insert(row, rowBytes);
                            }
                            break;
                    }
                    byteCounter += lines[line].Length;
                }

                subPage.LoadTTIdata();
                subPages.Add(subPage);
            }

            await InitCharacterSetsImageDataAsync();
        }

        private async Task InitCharacterSetsImageDataAsync() {
            var chardataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TTI2IMG.Resources.chardata.png") ?? throw new Exception("Failed to load char data");
            using var image = await Image.LoadAsync<Rgba32>(chardataStream);
            static int startOffset(int index) => index * 960;

            // Populate charsetImageData array
            for (var i = 0; i < 14; i++)
            {
                charsetImageData.Add(ImageToPixelArray(image, startOffset(i), 0x60));
            }

            // Populate NOSImageData array
            for (var i = 0; i < 13; i++)
            {
                NOSImageData.Add(ImageToPixelArray(image, 13760 + 130 * i, 13));
            }
        }

        private static List<List<Rgba32>> ImageToPixelArray(Image<Rgba32> image, int offset, int characters) {
            List<List<Rgba32>> imageDataArray = new();

            for (var character = 0; character < characters; character++)
            {
                List<Rgba32> data = new();

                for (var y = 0; y < 10; y++)
                {
                    var yPixel = character * 10 + y + offset;

                    for (var x = 0; x < 12; x++)
                    {
                        var pixel = image[x, yPixel];
                        data.Insert(y * 12 + x, pixel);
                    }
                }

                imageDataArray.Insert(character, data);
            }

            return imageDataArray;
        }
    }
}
