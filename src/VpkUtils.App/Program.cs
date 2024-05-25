using VpkUtils.Clap;
using VpkUtils.App;

var cli = new Clap<Config>(args, new ClapOptions
{
    Name = "vpk-utils",
    Version = "1.0.0",
    Licence = "MIT",
    Description = $"Developed by Nikita Milinkov ({DateTime.Now.Year})"
});

var config = cli.Parse();

if (args.Length == 0 || config.Help)
{
    Console.WriteLine(cli.GenerateHelp());
    return;
}

var app = new Application(config);
app.Run();

Console.WriteLine("Press any key to continue");
Console.ReadKey(); 
