using System.Reflection;

namespace VpkUtils.Clap;

internal readonly struct SchemaMetadata(Type ty)
{

    public readonly Type Type { get; init; } = ty;

    public readonly PropertyInfo[] Props
    {
        get
        {
            return Type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }
    }

    public readonly PropertyInfo[] FlagProps
    {
        get
        {
            return Props
                .Where(prop => prop.GetCustomAttribute<FlagAttribute>() is not null)
                .ToArray();
        }
    }
}
