using Timer = VpkUtils.Utility.Timer;

namespace VpkUtils.App;

internal class Application(Config cfg, ISubcommand subcommand)
{
    internal Config Config { get { return cfg; } }
    internal Timer Timer { get; } = new Timer();

    public void Run() => subcommand.Execute(this);

    internal string SelectDir()
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

    internal string GetFullPath()
    {
        var vpkDir = Environment.GetEnvironmentVariable("VPK_DIR") ?? Directory.GetCurrentDirectory();
        var subDir = cfg.SubDir ?? "";
        return Path.Combine(vpkDir, subDir);
    }
}
