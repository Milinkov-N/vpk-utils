using System.IO;
using System.Security;
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
            case Subcommand.CheckSize: ExecCheckSize(); break;
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

    private void ExecCheckSize()
    {
        var dir = cfg.WorkDir is not null ? GetFullPath() : SelectDir();
        var invalidBpFiles = CheckFileSizesInDir(Path.Combine(dir, "BP"), cfg.BpFileSizeLimit);
        var invalidDmFiles = CheckFileSizesInDir(Path.Combine(dir, "DM"), cfg.DmFileSizeLimit);
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
