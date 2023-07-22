namespace Kokoabim.CommandLineTools.Tests;

public class GotOptsCliTest
{
    [Fact]
    public void Run_GotOptsCli_ExitCode0_ShouldShowHelp()
    {
        // assign
        using StringWriter stdErr = new StringWriter();
        using StringWriter stdOut = new StringWriter();
        GotOptsCli.DefaultSettings.Error = stdErr;
        GotOptsCli.DefaultSettings.Out = stdOut;

        GotOptsCli target = new();
        string[] args = new string[0];

        // act
        int actual = target.Run(args);
        string stdErrText = stdErr.ToString();
        string stdOutText = stdOut.ToString();

        // assert
        Assert.Equal(0, actual);
        Assert.Equal("", stdErrText);
        Assert.Equal(StripVerbatimString(@"Parse Shell Script Command Line Options and Arguments 1.0

Usage: gotopts [arguments] [options]

Arguments:
  name   Script name*
  title  Script title*
  args   Script arguments*

Options:
  -a           Script argument+
  -b           Script bottom help text§
  -d           Script arguments delimiter§
  -h           Script help
  -o           Script option+
  -p           Script argument prefix§
  -v           Script version§
  --help       Show help information
  --opts-args  Show options and arguments and exit.
  --version    Show version information

Patterns:
  Arguments  'name;description(;r:required)?(;e:canBeEmpty)?(;t:type)?(;d:default)?'
  Options    'template;description;optionType(;t:type)?(;d:default)?'

Created by Spencer James — https://github.com/kokoabim/gotopts — MIT License
"), stdOutText);
    }

    [Fact]
    public void Run_ShellScriptCli_ExitCode0_ShouldShowHelp()
    {
        // assign
        using StringWriter stdErr = new StringWriter();
        using StringWriter stdOut = new StringWriter();
        GotOptsCli.DefaultSettings.Error = stdErr;
        GotOptsCli.DefaultSettings.Out = stdOut;

        GotOptsCli target = new();
        string[] args = new string[] {
            "foo", "Does all sorts of Foo'ing Things", "",
            "-v", "1.0",
            "-a", "arg1;Argument 1",
            "-a", "arg2;Argument 2;e:1;t:i",
            "-a", "arg3;Argument 3;r:0;d:three",
            "-o", "-m,--multiple;Multiple-value option;m",
            "-o", "-n,--no;No-value option;n",
            "-o", "-s,--single;Single-value option;s",
        };

        // act
        int actual = target.Run(args);
        string stdErrText = stdErr.ToString();
        string stdOutText = stdOut.ToString();

        // assert
        Assert.Equal(0, actual);
        Assert.Equal("", stdErrText);
        Assert.Equal(StripVerbatimString(@"Does all sorts of Foo'ing Things 1.0

Usage: foo [arguments] [options]

Arguments:
  arg1  Argument 1*
  arg2  Argument 2*
  arg3  Argument 3

Options:
  -m|--multiple  Multiple-value option+
  -n|--no        No-value option
  -s|--single    Single-value option§
  --help         Show help information
  --version      Show version information

"), stdOutText);
    }

    [Fact]
    public void Run_ShellScriptCli_()
    {
        // assign
        using StringWriter stdErr = new StringWriter();
        using StringWriter stdOut = new StringWriter();
        GotOptsCli.DefaultSettings.Error = stdErr;
        GotOptsCli.DefaultSettings.Out = stdOut;

        GotOptsCli target = new();
        string[] args = new string[] {
            "foo", "Does all sorts of Foo'ing Things", "[-m|xyz|-m|abc|foo bar|2]",
            "-v", "1.0",
            "-a", "arg1;Argument 1",
            "-a", "arg2;Argument 2;e:1;t:i",
            "-a", "arg3;Argument 3;r:0;d:three",
            "-o", "-m,--multiple;Multiple-value option;o:m",
            "-o", "-n,--no;No-value option;o:n",
            "-o", "-s,--single;Single-value option;o:s",
        };

        // act
        int actual = target.Run(args);
        string stdErrText = stdErr.ToString();
        string stdOutText = stdOut.ToString();

        // assert
        Assert.Equal(0, actual);
        Assert.Equal("", stdErrText);
        Assert.Equal(StripVerbatimString("opt_m=\"xyz,abc\"\narg_arg1=\"foo bar\"\narg_arg2=\"2\"\narg_arg3=\"three\"\n"), stdOutText);
    }

    private static string StripVerbatimString(string value) => value.Replace("\r", "");
}