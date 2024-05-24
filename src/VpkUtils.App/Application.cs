namespace VpkUtils.App;

internal class Application(Config cfg)
{
    private readonly Config _config = cfg;
    private readonly string _fullPath = Path.Combine(
        Environment.GetEnvironmentVariable("VPK_DIR")
            ?? (cfg.WorkDir ?? Directory.GetCurrentDirectory()),
        cfg.SubDir ?? ""
    );

    public void Run()
    {
        DateTime startTime;
        switch (_config.Subcommand)
        {
            case Subcommand.Rename:
                {
                    var dir = SelectDir();
                    var newName = Path.GetFileName(dir);
                    var logFunc = (string entry, string n) =>
                    {
                        if (_config.Verbose)
                            Console.WriteLine($"\t{Path.GetFileName(entry)}\t-->\t{n}");
                    };
                    FileRenamer[] renamers = [
                        new(new FileRenamerOptions
                        {
                            Path = dir,
                            NewName = newName,
                            FilterByExt = ".jpg",
                            LogFunc = logFunc,
                        }),
                        new(new FileRenamerOptions
                        {
                            Path = dir,
                            NewName = newName,
                            FilterByExt = ".psd",
                            LogFunc = logFunc,
                        }),
                        new(new FileRenamerOptions
                        {
                            Path = Path.Combine(dir, "BP"),
                            NewName = newName,
                            FilterByExt = ".jpg",
                            NewNameSuffix = " вр",
                            LogFunc = logFunc,
                        }),
                        new(new FileRenamerOptions
                        {
                            Path = Path.Combine(dir, "DM"),
                            NewName = newName,
                            FilterByExt = ".jpg",
                            NewNameSuffix = " дм",
                            LogFunc = logFunc,
                        })
                    ];

                    startTime = DateTime.Now;
                    foreach (var r in renamers)
                    {
                        var ext = r.GetOptions().FilterByExt!.ToUpper();
                        var path = Path.GetFileName(r.GetOptions().Path) == newName
                            ? ""
                            : Path.GetFileName(r.GetOptions().Path);
                        if (_config.Verbose)
                            Console.WriteLine($"Renaming {ext} files in /{path}:");
                        r.Run(_config.DryRun);
                    }
                }
                break;
            default: throw new NotImplementedException();
        };

        var elapsed = DateTime.Now - startTime;
        if (_config.TimeExec)
            Console.WriteLine($"Finished in: {elapsed.Milliseconds}ms");
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
