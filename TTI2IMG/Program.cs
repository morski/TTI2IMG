using System.Text;
using CommandLine;
using TTI2IMG;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        return await Parser.Default.ParseArguments<CommandLineOptions>(args)
            .MapResult(async (opts) =>
            {
                try
                {
                    // We have the parsed arguments, so let's just pass them down
                    await InitializeTti2ImgAsync(opts);
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error!" + ex.ToString());
                    return -3; // Unhandled error
                }
            },
            errs => Task.FromResult(-1)); // Invalid arguments
    }

    private static async Task InitializeTti2ImgAsync(CommandLineOptions opts)
    {
        // stdin
        if (opts.Path.Equals("-"))
        {
            await CreateImageFromStdIn(opts);
        }
        else
        {
            FileAttributes attr = File.GetAttributes(opts.Path);

            // Directory of files
            if (attr.HasFlag(FileAttributes.Directory))
            {
                await CreateImagesFromDirectory(opts);
            }
            // Single file
            else
            {
                await CreateImageFromFile(opts.Path, opts);
            }
        }
    }

    private static async Task CreateImageFromStdIn(CommandLineOptions opts)
    {
        using (var sr = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding))
        {
            var input = await sr.ReadToEndAsync();
            var bytes = Encoding.ASCII.GetBytes(input);
            await CreateImageFromBytes(bytes, opts);
        }
    }

    private static async Task CreateImagesFromDirectory(CommandLineOptions opts)
    {
        DirectoryInfo d = new(opts.Path);

        foreach (var file in d.GetFiles("*.tti"))
        {
            await CreateImageFromFile(file.FullName, opts);
        }
    }

    private static async Task CreateImageFromFile(string fileFullName, CommandLineOptions opts)
    {
        var ttiFile = File.ReadAllBytes(fileFullName);
        await CreateImageFromBytes(ttiFile, opts);
    }

    private static async Task CreateImageFromBytes(byte[] bytes, CommandLineOptions opts)
    {
        var tti = new TTI();
        await tti.Parse(bytes);
        new Renderer(tti, opts).Render();
    }
}
