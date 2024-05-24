using VpkUtils.Cli;
using VpkUtils.App;

var cli = new Cli<Config>(new CliOptions
{
    Name = "vpk-utils",
    Version = "1.0.0",
    Licence = "MIT",
    Description = $"Developed by Nikita Milinkov ({DateTime.Now.Year})"
});

if (args.Length == 0)
{
    Console.WriteLine(cli.GenerateHelp());
    return;
}

var config = cli.ParseArgs(args);
var app = new Application(config!);

app.Run();

Console.WriteLine("Press any key to continue");
Console.ReadKey(); 
