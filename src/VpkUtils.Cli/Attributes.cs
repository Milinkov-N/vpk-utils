namespace VpkUtils.Cli;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FlagAttribute : Attribute
{
    public string? ShortName { get; set; }
    public string? LongName { get; set; }
    public string? Description { get; set; }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SubcommandAttribute : Attribute;
