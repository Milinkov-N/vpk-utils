using System.Reflection;

namespace VpkUtils.Clap;

internal struct SchemaMetadata<T>
{

    public readonly Type Type { get { return typeof(T); } }

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
