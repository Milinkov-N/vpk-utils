// See https://aka.ms/new-console-template for more information

using VpkUtils.Cli;

Console.WriteLine("Hello, World!");

var cli = new Cli<VpkUtilsArgsSchema>(new CliOptions
{
    Name = "vpk-utils",
    Version = "1.0.0",
    Licence = "MIT",
    Description = "bla bla bla"
});

class VpkUtilsArgsSchema
{
    [Subcommand]
    public VpkUtilsSubcommand Subcommand { get; set; }

    [Flag(ShortName = "w", Description = "sets root directory where all projects are located")]
    public string? WorkDir { get; set; }

    [Flag(ShortName = "s", Description = "sets subdirectory for listing available projects")]
    public string? SubDir { get; set; }

    [Flag(ShortName = "v", Description = "verbose output of the program")]
    public bool Verbose { get; set; } = false;
    [Flag(ShortName = "t", Description = "displays program execution time")]
    public bool TimeExec { get; set; } = false;
}

enum VpkUtilsSubcommand
{
    Rename,
    CheckSize,
}
