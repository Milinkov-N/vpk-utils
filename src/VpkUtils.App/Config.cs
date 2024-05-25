using VpkUtils.Clap;

namespace VpkUtils.App;

internal class Config
{
    [Subcommand]
    public Subcommand Subcommand { get; set; }

    [Flag(ShortName = "w", Description = "sets working directory where files be renamed. --sub-dir flag is ignored")]
    public string? WorkDir { get; set; }

    [Flag(ShortName = "s", Description = "sets subdirectory for listing available projects")]
    public string? SubDir { get; set; }

    [Flag(ShortName = "v", Description = "verbose output of the program")]
    public bool Verbose { get; set; } = false;
    [Flag(ShortName = "t", Description = "displays program execution time")]
    public bool TimeExec { get; set; } = false;

    [Flag(ShortName = "d", Description = "test program without actually renaming files")]
    public bool DryRun { get; set; } = false;

    [Flag(ShortName = "h", Description = "prints this message")]
    public bool Help { get; set; } = false;

    public int BpFileSizeLimit { get; set; } = 200;
    public int DmFileSizeLimit { get; set; } = 200;
}

internal enum Subcommand
{
    Unset,
    Rename,
    CheckSize,
}
