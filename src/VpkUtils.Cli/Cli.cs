using System.Reflection;

using VpkUtils.Utility;

namespace VpkUtils.Cli;

public class Cli<T>
    where T : class, new()
{
    private readonly CliOptions _options;
    private readonly Type _schemaType;

    public Cli()
    {
        _options = new CliOptions();
        _schemaType = typeof(T);
    }

    public Cli(CliOptions opt)
    {
        _options = opt;
        _schemaType = typeof(T);
    }

    public T? ParseArgs(string[] args)
    {
        if (args.Length == 0) { return null; }

        var instance = new T();
        var schemaProps = _schemaType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var subcommandProp = schemaProps.First(prop =>
            {
                return prop.GetCustomAttribute<SubcommandAttribute>() is not null;
            });

        ParseSubcommand(instance, subcommandProp, args[0]);

        foreach (var arg in args)
        {
            if (arg.StartsWith("--"))
                ParseLongFlag(instance, schemaProps, arg);
            else if (arg.StartsWith('-'))
                ParseShortFlag(instance, schemaProps, arg);
        }

        return instance;
    }

    public string GenerateHelp()
    {
        var helpMessage = $"{_options.Name} v{_options.Version}  {_options.Licence} Licence";
        return helpMessage;
    }

    private static void ParseSubcommand(T inst, PropertyInfo? prop, string raw)
    {
        if (prop != null)
        {
            if (!prop.PropertyType.IsEnum)
                throw new MemberAccessException("subcommand is not of enum type");

            var valueSet = false;

            foreach (var enumVal in Enum.GetValues(prop.PropertyType))
            {
                if (enumVal.ToString()!.ToLower().Equals(raw))
                {
                    prop.SetValue(inst, enumVal);
                    valueSet = true;
                }
            }

            if (!valueSet) throw new ArgumentException("subcommand at index 0 is invalid");
        }
    }

    private static void ParseLongFlag(T inst, PropertyInfo[] schemaProps, string arg)
    {
        var (name, value) = ArgKeyValue(arg);
        var argProp = schemaProps.First(prop => prop.Name.Equals(name));

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
            throw new ArgumentException("unsupported cli agrument type");
        }
    }

    private static void ParseShortFlag(T inst, PropertyInfo[] schemaProps, string arg)
    {
        var name = arg[1..];
        foreach (var ch in name)
        {
            var argProp = schemaProps.First(prop =>
            {
                var attr = prop.GetCustomAttribute<FlagAttribute>();
                return ValidateShortFlagAttr(attr, prop, ch);
            });

            if (argProp != null && argProp.PropertyType.Equals(typeof(bool)))
            {
                argProp.SetValue(inst, true);
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

public class CliOptions
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "0.1.0";
    public string Licence { get; set; } = "MIT";
    public string Description { get; set; } = string.Empty;
}
