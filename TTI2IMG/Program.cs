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
        FileAttributes attr = File.GetAttributes(opts.Path);

        if (attr.HasFlag(FileAttributes.Directory))
        {
            DirectoryInfo d = new(opts.Path);

            foreach (var file in d.GetFiles("*.tti"))
            {
                await CreateImageFromTti(file.FullName, opts);
            }
        }
        else
        {
            await CreateImageFromTti(opts.Path, opts);
        }
    }

    private static async Task CreateImageFromTti(string fileFullName, CommandLineOptions opts)
    {
        var ttiFile = File.ReadAllBytes(fileFullName);
        var tti = new TTI();
        await tti.Parse(ttiFile);
        new Renderer(tti, opts).Render();
    }
}
