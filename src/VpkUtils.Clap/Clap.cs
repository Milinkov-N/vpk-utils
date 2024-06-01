using System.Reflection;

using VpkUtils.Utility;

namespace VpkUtils.Clap;

public class Clap<T>
    where T : class, new()
{
    private List<string> Args { get; }

    public List<Type> Subcommands { get; init; } = [];
    private ClapOptions Options { get; }

    public Clap(List<string> args)
    {
        Args = args;
        Options = new ClapOptions();
    }

    public Clap(List<string> args, ClapOptions opt)
    {
        Args = args;
        Options = opt;
    }

    public void AddSubcommand<S>()
        where S : class, new()
    {
        if (Subcommands.Contains(typeof(S)))
            throw new ArgumentException($"subcommand `{typeof(S).Name}` is already listed");
        Subcommands.Add(typeof(S));
    }

    public (T, object?) Parse()
    {
        var instance = new T();
        var parser = new FlagParser(instance, typeof(T));
        object? subCmd = null;

        if (Args.Count == 0) { return (instance, subCmd); }

        var (idx, type) = FindSubcommand();

        if (idx == -1)
        {
            parser.ParseArgs(Args);
        }
        else
        {
            parser.ParseArgs(Args[..idx]);
            subCmd = ParseSubcommand(idx, type);
        }

        return (instance, subCmd);
    }

    private object? ParseSubcommand(int idx, Type? type)
    {
        if (type != null)
        {
            var inst = Activator.CreateInstance(type);
            var parser = new FlagParser(inst!, type);

            parser.ParseArgs(Args[(idx + 1)..]);

            return inst;
        }

        return null;
    }

    private (int, Type?) FindSubcommand()
    {
        foreach (var arg in Args
            .Where(arg => !arg.StartsWith("--") && !arg.StartsWith('-')))
        {
            var ty = GetSubcommand(arg);
            if (ty != null) return (Args.IndexOf(arg), ty);
        }

        return (-1, null);
    }

    private Type? GetSubcommand(string name)
    {
        return Subcommands.First(cmd => name.Equals(
                cmd.Name.TrimEnd("Subcommand").TrimEnd("Options"),
                StringComparison.CurrentCultureIgnoreCase));
    }

    public string GenerateHelp()
    {
        var meta = new SchemaMetadata(typeof(T));
        var header = $"{Options.Name} v{Options.Version}  {Options.Licence} Licence";
        var options = "OPTIONS:\n";
        var subcommands = "SUBCOMMANDS:\n";

        foreach (var subCmd in Subcommands)
        {
            var name = subCmd.Name.TrimEnd("Subcommand").TrimEnd("Options").ToLower();
            var desc = subCmd.GetCustomAttribute<SubcommandAttribute>()!.Desc;
            subcommands += $"\t{name}\t{desc}\n";
        }

        foreach (var prop in meta.FlagProps)
        {
            var attr = prop.GetCustomAttribute<FlagAttribute>()!;
            var longName = prop.Name.ToKebabCase();
            var shortName = attr.ShortName;
            var desc = attr.Description;
            options += $"\t--{longName}, -{shortName}\t {desc}\n";
        }

        return $"{header}\n{Options.Description}\n\n{subcommands}\n{options}";
    }
}

public class ClapOptions
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "0.1.0";
    public string Licence { get; set; } = "MIT";
    public string Description { get; set; } = string.Empty;
}
