using VpkUtils.Utility;
using Timer = VpkUtils.Utility.Timer;

namespace VpkUtils.App;

internal class Application(Config cfg)
{
    private Timer Timer { get; } = new Timer();

    public void Run()
    {
        switch (cfg.Subcommand)
        {
            case Subcommand.Rename: ExecRename(); break;
            default: throw new NotImplementedException();
        };

        Timer.End();
        if (cfg.TimeExec)
            Console.WriteLine($"Finished in: {Timer.ElapsedMillis()}ms");
    }

    private void ExecRename()
    {
        var dir = cfg.WorkDir is not null ? GetFullPath() : SelectDir();
        var newName = Path.GetFileName(dir);
        var renamers = GetFileRenamers(dir);

        Timer.Start();
        foreach (var r in renamers)
        {
            var opt = r.GetOptions();
            opt.LogFunc = Log;
            opt.NewName = newName;
            var ext = opt.Extension!.ToUpper();
            var path = Path.GetFileName(opt.Path) == newName
                ? ""
                : Path.GetFileName(opt.Path);
            if (cfg.Verbose)
                Console.WriteLine($"Renaming {ext} files in /{path}:");
            r.Run(cfg.DryRun);
        }
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

    private void Log(string entry, string newName)
    {
        if (cfg.Verbose)
            Console.WriteLine($"\t{Path.GetFileName(entry)}\t-->\t{newName}");
    }

    private string SelectDir()
    {
        var idx = int.MaxValue;
        Console.WriteLine("Select directory:");
        var dirs = ListAvailableDirectories();
        while (idx >= dirs.Count || idx < 0)
        {
            Console.Write("Select directory index: ");
            idx = int.Parse(Console.ReadLine()!);
        }
        return dirs[idx];
    }

    private List<string> ListAvailableDirectories()
    {
        var dirs = Directory.EnumerateDirectories(GetFullPath());
        var n = dirs.Count();
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine($"\t[{i}] {Path.GetFileName(dirs.ElementAt(i))}");
        }
        return dirs.ToList();
    }

    private string GetFullPath()
    {
        var vpkDir = Environment.GetEnvironmentVariable("VPK_DIR") ?? Directory.GetCurrentDirectory();
        var subDir = cfg.SubDir ?? "";
        return Path.Combine(vpkDir, subDir);
    }
}
