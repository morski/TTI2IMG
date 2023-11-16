using CommandLine;

namespace TTI2IMG
{
    public class CommandLineOptions
    {
        [Option(shortName: 'i', longName: "input", Required = true, HelpText = "Path to TTI file(s). Directory or file")]
        public string Path { get; set; }

        [Option(shortName: 'o', longName: "output", Required = true, HelpText = "Directory where to save the image(s).")]
        public string OutputPath { get; set; }

        [Option(shortName: 'f', longName: "format", Required = false, HelpText = "Format of the saved image. Accepted formats are: PNG, JPG, GIF, BMP, PBM, TGA, TIFF, WEBP.", Default = FileFormats.PNG)]
        public FileFormats Format { get; set; }

        [Option(shortName: 'w', longName: "width", Required = false, HelpText = "Width of image", Default = 768)]
        public int Width { get; set; }

        [Option(shortName: 'h', longName: "height", Required = false, HelpText = "Height of image", Default = 576 )]
        public int Height { get; set; }

    }

    public enum FileFormats
    {
        PNG,
        JPG,
        GIF,
        BMP,
        PBM,
        TGA,
        TIFF,
        WEBP
    }
}
