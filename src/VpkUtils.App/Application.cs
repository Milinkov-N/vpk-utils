namespace VpkUtils.App;

internal class Application(Config cfg)
{
    private readonly Config _config = cfg;
    private readonly string _fullPath = Path.Combine(
        cfg.WorkDir
            ?? (Environment.GetEnvironmentVariable("VPK_DIR")
                ?? Directory.GetCurrentDirectory()),
        cfg.SubDir ?? ""
    );

    public void Run()
    {
        var startTime = DateTime.Now;

        switch (_config.Subcommand)
        {
            case Subcommand.Rename: ExecRename(); break;
            default: throw new NotImplementedException();
        };

        var elapsed = DateTime.Now - startTime;
        if (_config.TimeExec)
            Console.WriteLine($"Finished in: {elapsed.Milliseconds}ms");
    }

    private void ExecRename()
    {
        var dir = _config.WorkDir is not null ? _fullPath : SelectDir();
        var newName = Path.GetFileName(dir);
        var renamers = GetFileRenamers(dir);

        foreach (var r in renamers)
        {
            var opt = r.GetOptions();
            opt.LogFunc = Log;
            opt.NewName = newName;
            var ext = opt.Extension!.ToUpper();
            var path = Path.GetFileName(opt.Path) == newName
                ? ""
                : Path.GetFileName(opt.Path);
            if (_config.Verbose)
                Console.WriteLine($"Renaming {ext} files in /{path}:");
            r.Run(_config.DryRun);
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
        if (_config.Verbose)
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
        var dirs = Directory.EnumerateDirectories(_fullPath);
        var n = dirs.Count();
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine($"\t[{i}] {Path.GetFileName(dirs.ElementAt(i))}");
        }
        return dirs.ToList();
    }
}
