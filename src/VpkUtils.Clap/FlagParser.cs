using System.Reflection;
using VpkUtils.Utility;

namespace VpkUtils.Clap;

internal class FlagParser(object inst, Type instType)
{
    private SchemaMetadata Meta { get; } = new SchemaMetadata(instType);

    public void ParseArgs(List<string> args)
    {
        foreach (var arg in args)
        {
            try
            {
                if (arg.StartsWith("--"))
                    ParseLongFlag(arg);
                else if (arg.StartsWith('-'))
                    ParseShortFlag(arg);
            }
            catch (InvalidOperationException ex)
            {
                throw new UnexpectedArgumentException(arg, ex);
            }
        }
    }

    private void ParseLongFlag(string arg)
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

    private void ParseShortFlag(string arg)
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
        if (attr is null || attr.ShortName is null) return false;
        return attr.ShortName.Equals(ch);
    }
}
