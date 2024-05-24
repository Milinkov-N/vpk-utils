using VpkUtils.Cli;

namespace VpkUtils.App;

internal class Config
{
    [Subcommand]
    public Subcommand Subcommand { get; set; }

    [Flag(ShortName = "w", Description = "sets root directory where all projects are located")]
    public string? WorkDir { get; set; }

    [Flag(ShortName = "s", Description = "sets subdirectory for listing available projects")]
    public string? SubDir { get; set; }

    [Flag(ShortName = "v", Description = "verbose output of the program")]
    public bool Verbose { get; set; } = false;
    [Flag(ShortName = "t", Description = "displays program execution time")]
    public bool TimeExec { get; set; } = false;

    [Flag(ShortName = "d", Description = "test program without actually renaming files")]
    public bool DryRun { get; set; } = false;
}

internal enum Subcommand
{
    Rename,
    CheckSize,
}
