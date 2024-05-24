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
    public void Cli_ParseArgsWithInvalidSubcommand_ThrowsException()
    {
        var cli = new Cli<CorrectMockSchema>();
        var args = new[] { "invalid" };

        var action = () => cli.ParseArgs(args);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Cli_ParseLongFlagsOfBooleanType_ReturnsValidInstance()
    {
        var cli = new Cli<CorrectMockSchema>(new CliOptions
        {
            ExcludeSubcommand = true,
        });
        var args = new[] { "--verbose", "--dry-run", "--explain" };

        var schema = cli.ParseArgs(args);

        Assert.NotNull(schema);
        Assert.True(schema.Verbose);
        Assert.True(schema.DryRun);
        Assert.True(schema.Explain);
    }

    [Fact]
    public void Cli_ParseLongFlagsWithValues_ReturnsValidInstance()
    {
        var cli = new Cli<CorrectMockSchema>(new CliOptions
        {
            ExcludeSubcommand = true,
        });
        var args = new[] { "--work-dir=/foo/bar", "--sub-dir=q/s/d" };

        var schema = cli.ParseArgs(args);

        Assert.NotNull(schema);
        Assert.Equal("/foo/bar", schema.WorkDir);
        Assert.Equal("q/s/d", schema.SubDir);
    }

    [Fact]
    public void Cli_ParseShortFlagsOfBooleanType_ReturnsValidInstance()
    {
        var cli = new Cli<CorrectMockSchema>(new CliOptions
        {
            ExcludeSubcommand = true,
        });
        var args = new[] { "-v", "-d", "-e" };

        var schema = cli.ParseArgs(args);

        Assert.NotNull(schema);
        Assert.True(schema.Verbose);
        Assert.True(schema.DryRun);
        Assert.True(schema.Explain);
    }

    [Fact]
    public void Cli_ParseShortFlagsWithValues_ReturnsValidInstance()
    {
        var cli = new Cli<CorrectMockSchema>(new CliOptions
        {
            ExcludeSubcommand = true,
        });
        var args = new[] { "-w/foo/bar", "-sq/s/d" };

        var schema = cli.ParseArgs(args);

        Assert.NotNull(schema);
        Assert.Equal("/foo/bar", schema.WorkDir);
        Assert.Equal("q/s/d", schema.SubDir);
    }

    [Fact]
    public void Cli_ParseShortFlagsOfBooleanTypeInBatch_ReturnsValidInstance()
    {
        var cli = new Cli<CorrectMockSchema>(new CliOptions
        {
            ExcludeSubcommand = true,
        });
        var args = new[] { "-vde" };

        var schema = cli.ParseArgs(args);

        Assert.NotNull(schema);
        Assert.True(schema.Verbose);
        Assert.True(schema.DryRun);
        Assert.True(schema.Explain);
    }

    [Fact]
    public void Cli_ParseShortFlagsAndLongFlagsInBatch_ThrowsException()
    {
        var cli = new Cli<CorrectMockSchema>(new CliOptions
        {
            ExcludeSubcommand = true,
        });
        var args = new[] { "-vw/foo/bar" };

        var action = () => cli.ParseArgs(args);

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
    First,
    Second,
    Third
}
