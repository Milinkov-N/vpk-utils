using VpkUtils.Clap;

namespace VpkUtils.Tests;

public class CliUnitTest
{
    [Fact]
    public void Cli_ParseSubcommandWithFirstValue_ReturnsValidInstance()
    {
        var args = new[] { "first" };
        var cli = new Clap<CorrectMockSchema>(args);

        var schema = cli.Parse();

        Assert.StrictEqual(Subcommand.First, schema.Subcommand);
    }

    [Fact]
    public void Cli_ParseSubcommandWithThirdValue_ReturnsValidInstance()
    {
        var args = new[] { "third" };
        var cli = new Clap<CorrectMockSchema>(args);

        var schema = cli.Parse();

        Assert.StrictEqual(Subcommand.Third, schema.Subcommand);
    }

    [Fact]
    public void Cli_ParseInvalidSubcommand_ThrowsException()
    {
        var args = new[] { "invalid" };
        var cli = new Clap<CorrectMockSchema>(args);

        var schema = cli.Parse();

        Assert.StrictEqual(Subcommand.Unset, schema.Subcommand);
    }

    [Fact]
    public void Cli_ParseLongFlagsOfBooleanType_ReturnsValidInstance()
    {
        var args = new[] { "--verbose", "--dry-run", "--explain" };
        var cli = new Clap<CorrectMockSchema>(args);

        var schema = cli.Parse();

        Assert.NotNull(schema);
        Assert.True(schema.Verbose);
        Assert.True(schema.DryRun);
        Assert.True(schema.Explain);
    }

    [Fact]
    public void Cli_ParseLongFlagsWithValues_ReturnsValidInstance()
    {
        var args = new[] { "--work-dir=/foo/bar", "--sub-dir=q/s/d" };
        var cli = new Clap<CorrectMockSchema>(args);

        var schema = cli.Parse();

        Assert.NotNull(schema);
        Assert.Equal("/foo/bar", schema.WorkDir);
        Assert.Equal("q/s/d", schema.SubDir);
    }

    [Fact]
    public void Cli_ParseShortFlagsOfBooleanType_ReturnsValidInstance()
    {
        var args = new[] { "-v", "-d", "-e" };
        var cli = new Clap<CorrectMockSchema>(args);

        var schema = cli.Parse();

        Assert.NotNull(schema);
        Assert.True(schema.Verbose);
        Assert.True(schema.DryRun);
        Assert.True(schema.Explain);
    }

    [Fact]
    public void Cli_ParseShortFlagsWithValues_ReturnsValidInstance()
    {
        var args = new[] { "-w/foo/bar", "-sq/s/d" };
        var cli = new Clap<CorrectMockSchema>(args);

        var schema = cli.Parse();

        Assert.NotNull(schema);
        Assert.Equal("/foo/bar", schema.WorkDir);
        Assert.Equal("q/s/d", schema.SubDir);
    }

    [Fact]
    public void Cli_ParseShortFlagsOfBooleanTypeInBatch_ReturnsValidInstance()
    {
        var args = new[] { "-vde" };
        var cli = new Clap<CorrectMockSchema>(args);

        var schema = cli.Parse();

        Assert.NotNull(schema);
        Assert.True(schema.Verbose);
        Assert.True(schema.DryRun);
        Assert.True(schema.Explain);
    }

    [Fact]
    public void Cli_ParseShortFlagsAndLongFlagsInBatch_ThrowsException()
    {
        var args = new[] { "-vw/foo/bar" };
        var cli = new Clap<CorrectMockSchema>(args);

        var action = () => cli.Parse();

        Assert.Throws<ArgumentException>(action);
    }
}

internal class CorrectMockSchema
{
    [Subcommand]
    public Subcommand Subcommand { get; set; }


    [Flag(ShortName = "v")]
    public bool Verbose { get; set; }

    [Flag(ShortName = "d")]
    public bool DryRun { get; set; }

    [Flag(ShortName = "e")]
    public bool Explain { get; set; }

    [Flag(ShortName = "w")]
    public string WorkDir { get; set; } = string.Empty;

    [Flag(ShortName = "s")]
    public string SubDir { get; set; } = string.Empty;
}

internal enum Subcommand
{
    Unset,
    First,
    Second,
    Third
}
