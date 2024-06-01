using VpkUtils.Clap;
using VpkUtils.App;
using VpkUtils.Utility;

DotEnv.Load();

//using var _ = new EmailRepository(
//    "milinkov.nik@gmail.com",
//    Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD")!
//);

var cli = new Clap<Config>([..args], new ClapOptions
{
    Name = "vpk-utils",
    Version = "0.3.0",
    Licence = "MIT",
    Description = $"Developed by Nikita Milinkov ({DateTime.Now.Year})"
})
{
    Subcommands = [typeof(RenameSubcommand), typeof(CheckSizeSubcommand)]
};

var (config, subcommand) = cli.Parse();

if (args.Length == 0 || config.Help)
{
    Console.WriteLine(cli.GenerateHelp());
    return;
}

var app = new Application(config, (ISubcommand)subcommand!);
app.Run();

Console.WriteLine("Press any key to continue");
Console.ReadKey();
