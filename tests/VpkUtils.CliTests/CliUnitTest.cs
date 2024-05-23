using VpkUtils.Cli;

namespace VpkUtils.Tests;

public class CliUnitTest
{
    [Fact]
    public void Cli_ParseArgsWithFirstSubcommand_ReturnsValidInstance()
    {
        var cli = new Cli<CorrectMockSchema>();
        var args = new[] { "first" };

        var schema = cli.ParseArgs(args);

        Assert.NotNull(schema);
        Assert.StrictEqual(Subcommand.First, schema.Subcommand);
    }

    [Fact]
    public void Cli_ParseArgsWithThirdSubcommand_ReturnsValidInstance()
    {
        var cli = new Cli<CorrectMockSchema>();
        var args = new[] { "third" };

        var schema = cli.ParseArgs(args);

        Assert.NotNull(schema);
        Assert.StrictEqual(Subcommand.Third, schema.Subcommand);
    }

    [Fact]
    public void Cli_ParseArgsWithInvalidSubcommand_ReturnsNull()
    {
        var cli = new Cli<CorrectMockSchema>();
        var args = new[] { "invalid" };

        var action = () => cli.ParseArgs(args);

        Assert.Throws<ArgumentException>(action);
    }
}

internal class CorrectMockSchema
{
    [Subcommand]
    public Subcommand Subcommand { get; set; }
}

internal enum Subcommand
{
    First,
    Second,
    Third
}
