using System.Reflection;

using VpkUtils.Utility;

namespace VpkUtils.Clap;

public class Clap<T>
    where T : class, new()
{
    private string[] Args { get; }
    private ClapOptions Options { get; }
    private static SchemaMetadata<T> Meta { get; }

    public Clap(string[] args)
    {
        Args = args;
        Options = new ClapOptions();
    }

    public Clap(string[] args, ClapOptions opt)
    {
        Args = args;
        Options = opt;
    }

    public T Parse()
    {
        var instance = new T();

        if (Args.Length == 0) { return instance; }

        ParseSubcommand(instance);

        foreach (var arg in Args)
        {
            if (arg.StartsWith("--"))
                ParseLongFlag(instance, arg);
            else if (arg.StartsWith('-'))
                ParseShortFlag(instance, arg);
        }

        return instance;
    }

    private void ParseSubcommand(T inst)
    {
        var subcommandProp = Meta.Props
            .First(prop => prop.GetCustomAttribute<SubcommandAttribute>() is not null);

        if (subcommandProp == null) { return; }

        if (!subcommandProp.PropertyType.IsEnum)
            throw new Exception("subcommand is not of enum type");

        foreach (var arg in Args.Where(arg => !arg.StartsWith("--") && !arg.StartsWith('-')))
        {
            foreach (var enumVal in Enum.GetValues(subcommandProp.PropertyType))
            {
                if (enumVal.ToString()!.ToKebabCase().Equals(arg))
                {
                    subcommandProp.SetValue(
                        inst,
                        Enum.Parse(subcommandProp.PropertyType, enumVal.ToString()!),
                        null
                    );
                    break;
                }
            }
        }
    }

    public string GenerateHelp()
    {
        var header = $"{Options.Name} v{Options.Version}  {Options.Licence} Licence";
        var usage = "USAGE:\n";
        var subcommands = "SUBCOMMANDS:\n";
        var subcommandProp = Meta.Props.First(prop =>
        {
            return prop.GetCustomAttribute<SubcommandAttribute>() is not null;
        });

        if (!subcommandProp.PropertyType.IsEnum)
            throw new MemberAccessException("subcommand is not of enum type");

        foreach (var enumVal in Enum.GetValues(subcommandProp.PropertyType))
        {
            var name = enumVal.ToString()!.ToKebabCase();
            if (name != "unset") subcommands += $"\t{name}\n";
        }

        foreach (var prop in Meta.FlagProps)
        {
            var attr = prop.GetCustomAttribute<FlagAttribute>()!;
            var longName = prop.Name.ToKebabCase();
            var shortName = attr.ShortName;
            var desc = attr.Description;
            usage += $"\t--{longName}, -{shortName}\t {desc}\n";
        }

        return $"{header}\n{Options.Description}\n\n{subcommands}\n{usage}";
    }

    private static void ParseLongFlag(T inst, string arg)
    {
        var (name, value) = ArgKeyValue(arg);
        var argProp = Meta.FlagProps.First(prop => prop.Name.Equals(name));

        if (argProp != null && argProp.PropertyType.Equals(typeof(bool)))
        {
            argProp.SetValue(inst, true);
        }
        else if (argProp != null && argProp.PropertyType.Equals(typeof(string)))
        {
            argProp.SetValue(inst, value ?? string.Empty);
        }
        else
        {
            throw new ArgumentException($"unknown '{name}' flag");
        }
    }

    private static void ParseShortFlag(T inst, string arg)
    {
        var name = arg[1..];

        for (int i = 0; i < name.Length; i++)
        {
            char ch = name[i];
            var argProp = Meta.FlagProps.First(prop =>
            {
                var attr = prop.GetCustomAttribute<FlagAttribute>();
                return ValidateShortFlagAttr(attr, prop, ch);
            });

            if (argProp != null && argProp.PropertyType.Equals(typeof(bool)))
            {
                argProp.SetValue(inst, true);
            }
            else if (argProp != null && argProp.PropertyType.Equals(typeof(string)))
            {
                if (i > 0)
                {
                    throw new ArgumentException("invalid flag position");
                }
                else if (arg.Length > 1)
                {
                    argProp.SetValue(inst, arg[2..]);
                }
                else
                {
                    argProp.SetValue(inst, "");
                }

                break;
            }
        }
    }

    private static (string, string?) ArgKeyValue(string arg)
    {
        var eqIndex = arg.IndexOf('=');
        var upTo = eqIndex == -1 ? arg.Length : eqIndex;
        var name = arg[2..upTo].ToPascalCase();
        var value = eqIndex == -1 ? null : arg[(eqIndex + 1)..arg.Length];
        return (name, value);
    }

    private static bool ValidateShortFlagAttr(FlagAttribute? attr, PropertyInfo prop, char ch)
    {
        if (attr is null) return false;

        if (attr.ShortName is not null)
        {
            if (attr.ShortName.Length != 1)
                throw new ArgumentException("short flag name should be 1 character long");

            var a = attr.ShortName.First().Equals(ch);
            var b = attr.ShortName.First().ToLowerCase().Equals(prop.Name.First().ToLowerCase());
            return a && b;
        }

        return prop.Name.First().ToLowerCase().Equals(ch);
    }
}

public class ClapOptions
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "0.1.0";
    public string Licence { get; set; } = "MIT";
    public string Description { get; set; } = string.Empty;
}

