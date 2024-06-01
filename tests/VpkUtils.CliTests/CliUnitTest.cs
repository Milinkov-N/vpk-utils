using VpkUtils.Clap;

namespace VpkUtils.Tests;

public class CliUnitTest
{
    private static Clap<RootOptions> NewClap(List<string> args)
    {
        return new Clap<RootOptions>(args)
        {
            Subcommands = [
                typeof(FirstOptions),
                typeof(SecondOptions),
            ]
        };
    }

    [Fact]
    public void Cli_ParseRootOptionsWithoutFlags_ReturnsValidInstance()
    {
        var cli = NewClap([]);

        var (schema, _) = cli.Parse();

        Assert.NotNull(schema);
        Assert.False(schema.Help);
    }

    [Fact]
    public void Cli_ParseRootOptionsWithOneFlag_ReturnsValidInstance()
    {
        var cli = NewClap(["--help"]);

        var (schema, _) = cli.Parse();

        Assert.NotNull(schema);
        Assert.True(schema.Help);
    }

    [Fact]
    public void Cli_ParseRootOptions_ReturnsValidInstance()
    {
        var cli = NewClap(["--help", "-Ldebug"]);

        var (schema, _) = cli.Parse();

        Assert.NotNull(schema);
        Assert.True(schema.Help);
        Assert.Equal("debug", schema.LogLevel);
    }

    [Fact]
    public void Cli_ParseRootOptionsInvalidFlag_ReturnsValidInstance()
    {
        var cli = NewClap(["--invalid"]);

        Assert.Throws<UnexpectedArgumentException>(() => cli.Parse());
    }

    [Fact]
    public void Cli_ParseFirstSubcommandWithNoFlags_ReturnsValidInstance()
    {
        var cli = NewClap(["first"]);

        var (_, subcommand) = cli.Parse();

        Assert.NotNull(subcommand);
        Assert.IsType<FirstOptions>(subcommand);
    }

    [Fact]
    public void Cli_ParseFirstSubcommandWithLongFlags_ReturnsValidInstance()
    {
        var cli = NewClap(["first", "--work-dir=foo", "--sub-dir=bar"]);

        var (_, subcommand) = cli.Parse();

        Assert.NotNull(subcommand);
        Assert.IsType<FirstOptions>(subcommand);

        FirstOptions firstOptions = (FirstOptions)subcommand;
        Assert.Equal("foo", firstOptions.WorkDir);
        Assert.Equal("bar", firstOptions.SubDir);
    }

    [Fact]
    public void Cli_ParseFirstSubcommandWithShortFlags_ReturnsValidInstance()
    {
        var cli = NewClap(["first", "-wfoo", "-sbar"]);

        var (_, subcommand) = cli.Parse();

        Assert.NotNull(subcommand);
        Assert.IsType<FirstOptions>(subcommand);

        FirstOptions firstOptions = (FirstOptions)subcommand;
        Assert.Equal("foo", firstOptions.WorkDir);
        Assert.Equal("bar", firstOptions.SubDir);
    }


    [Fact]
    public void Cli_ParseFirstSubcommandWithInvalidFlag_ReturnsValidInstance()
    {
        var cli = NewClap(["first", "--invalid"]);

        Assert.Throws<UnexpectedArgumentException>(() => cli.Parse());
    }

    [Fact]
    public void Cli_ParseFirstSubcommandWithShortFlagsAndRootOptions_ReturnsValidInstance()
    {
        var cli = NewClap(["--help", "first", "-wfoo", "-sbar"]);

        var (schema, subcommand) = cli.Parse();

        Assert.NotNull(schema);
        Assert.True(schema.Help);

        Assert.NotNull(subcommand);
        Assert.IsType<FirstOptions>(subcommand);

        FirstOptions firstOptions = (FirstOptions)subcommand;
        Assert.Equal("foo", firstOptions.WorkDir);
        Assert.Equal("bar", firstOptions.SubDir);
    }
}

internal class RootOptions
{
    [Flag('h')]
    public bool Help { get; set; }

    [Flag('L')]
    public string LogLevel { get; set; } = string.Empty;
}

[Subcommand]
internal class FirstOptions
{
    [Flag('w')]
    public string WorkDir { get; set; } = string.Empty;

    [Flag('s')]
    public string SubDir { get; set; } = string.Empty;
}


[Subcommand]
internal class SecondOptions
{
    [Flag('d')]
    public bool DryRun { get; set; }

    [Flag('e')]
    public bool Explain { get; set; }
}
