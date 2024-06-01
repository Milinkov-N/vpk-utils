using VpkUtils.Clap;
using VpkUtils.Utility;

namespace VpkUtils.App;

internal class Config
{

    [Flag('w', Description = "sets working directory where files be renamed. --sub-dir flag is ignored")]
    public string? WorkDir { get; set; }

    [Flag('s', Description = "sets subdirectory for listing available projects")]
    public string? SubDir { get; set; }

    [Flag('h', Description = "prints this message")]
    public bool Help { get; set; } = false;

    public int BpFileSizeLimit { get; set; } = 200;
    public int DmFileSizeLimit { get; set; } = 200;
}

[Subcommand(Desc = "renames .jpg and .psd files in vpk project directory")]
internal class RenameSubcommand : ISubcommand
{
    [Flag('v', Description = "verbose output of the program")]
    public bool Verbose { get; set; } = false;

    [Flag('t', Description = "displays program execution time")]
    public bool TimeExec { get; set; } = false;

    [Flag('d', Description = "test program without actually renaming files")]
    public bool DryRun { get; set; } = false;

    public void Execute(Application app)
    {
        var dir = app.Config.WorkDir is not null ? app.GetFullPath() : app.SelectDir();
        var newName = Path.GetFileName(dir);
        var renamers = GetFileRenamers(dir);

        app.Timer.Start();
        foreach (var r in renamers)
        {
            var opt = r.GetOptions();
            opt.LogFunc = Logger.Log;
            opt.NewName = newName;
            var ext = opt.Extension!.ToUpper();
            var path = Path.GetFileName(opt.Path) == newName
                ? ""
                : Path.GetFileName(opt.Path);
            if (Verbose)
                Console.WriteLine($"Renaming {ext} files in /{path}:");
            r.Run(DryRun);
        }

        app.Timer.End();
        if (TimeExec)
            Console.WriteLine($"Finished in: {app.Timer.ElapsedMillis()}ms");
    }

    private static FileRenamer[] GetFileRenamers(string dir)
    {
        return [
            new(new FileRenamerOptions
            {
                Path = dir,
                Extension = ".jpg",
            }),
            new(new FileRenamerOptions
            {
                Path = dir,
                Extension = ".psd",
            }),
            new(new FileRenamerOptions
            {
                Path = Path.Combine(dir, "BP"),
                Extension = ".jpg",
                NewNameSuffix = " вр",
            }),
            new(new FileRenamerOptions
            {
                Path = Path.Combine(dir, "DM"),
                Extension = ".jpg",
                NewNameSuffix = " дм",
            })
        ];
    }
}

[Subcommand(Desc = "checks filesize of .jpg files in BP and DM directories")]
internal class CheckSizeSubcommand : ISubcommand
{
    public void Execute(Application app)
    {
        var dir = app.Config.WorkDir is not null ? app.GetFullPath() : app.SelectDir();
        var invalidBpFiles = CheckFileSizesInDir(Path.Combine(dir, "BP"), app.Config.BpFileSizeLimit);
        var invalidDmFiles = CheckFileSizesInDir(Path.Combine(dir, "DM"), app.Config.DmFileSizeLimit);
        FileSizeCheckPrintResult("BP", invalidBpFiles);
        FileSizeCheckPrintResult("DM", invalidDmFiles);
    }

    private static List<string> CheckFileSizesInDir(string dir, int fileSizeLimit)
    {
        var invalidFiles = new List<string>();
        var files = Directory.EnumerateFiles(dir)
            .Where(d => Path.GetExtension(d) == ".jpg");
        foreach (var file in files)
            if (new FileInfo(file).Length / 1024 >= fileSizeLimit)
                invalidFiles.Add(file);
        return invalidFiles;
    }

    private static void FileSizeCheckPrintResult(string checkedDir, List<string> invalidFiles)
    {
        if (invalidFiles.Count > 0)
        {
            Console.WriteLine($"There {invalidFiles.Count} files in /{checkedDir} with invalid size:");
            foreach (var file in invalidFiles)
                Console.WriteLine($"\t{Path.GetFileName(file)} \x1b[31m{new FileInfo(file).Length / 1024}KB\u001b[0m");
        }
        else
        {
            Console.WriteLine($"\x1b[32m[+]\u001b[0m All Files in /{checkedDir} of valid size.");
        }
    }
}
