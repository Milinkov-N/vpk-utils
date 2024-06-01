namespace VpkUtils.Clap;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FlagAttribute(char shortName) : Attribute
{
    public char? ShortName { get; set; } = shortName;
    public string? LongName { get; set; }
    public string? Description { get; set; }

}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SubcommandAttribute : Attribute
{
    public string? Alias { get; set; }
    public string? Desc { get; set; }
}