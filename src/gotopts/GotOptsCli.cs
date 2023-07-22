using System.Text.RegularExpressions;
using Microsoft.Extensions.CommandLineUtils;

namespace Kokoabim.CommandLineTools;

/// <summary>
/// GotOpts command line tool.
/// </summary>
public class GotOptsCli : CommandLineTool
{
    /// <summary>
    /// Settings used to create instances of <see cref="GotOptsCli"/> when parameterless constructor is used.
    /// </summary>
    public static readonly CommandLineToolSettings DefaultSettings = new("Parse Shell Script Command Line Options and Arguments")
    {
        BottomHelpText = string.Concat("Option (-o) and Argument (-a) Patterns:\n",
            "  Option    'template;description(;o:optionType)?(;f:func)?(;t:type)?(;d:default)?'\n",
            "  Argument  'name;description(;r:required)?(;e:canBeEmpty)?(;f:func)?(;t:type)?(;d:default)?'\n",
          "\n  Defaults:\n",
            "    Option    optionType=no-value(off|on), type=string, default=null\n",
           $"    Argument  required={Argument.DefaultRequired.ToString().ToLowerInvariant()}, canBeEmpty={Argument.DefaultCanBeEmpty.ToString().ToLowerInvariant()}, type=string, default=null\n",
          "\n  Value types:\n",
            "    default              String\n",
            "    type                 Option/Argument value type: number=n, string=s\n",
            "    required|canBeEmpty  Boolean: true=1|t, false=0|f\n",
            "    optionType           Option value type: multiple-values=m, no-value(off|on)=n, single-value=s\n",
          "\nCreated by Spencer James — https://github.com/kokoabim/gotopts — MIT License"),
        UseAnsiColors = false,
        Version = "1.0",
    };

    private static readonly Regex ArgumentPattern = new Regex(@"^(?<name>[a-z0-9_]*?);(?<desc>.*?)(;r:(?<req>[01ft]))?(;e:(?<cbe>[01ft]))?(;f:(?<func>.*?))?(;t:(?<type>.*?))?(;d:(?<def>.*?))?$", RegexOptions.IgnoreCase);

    private static readonly Regex OptionPattern = new Regex(@"^(?<temp>[a-z0-9_\-,]*?);(?<desc>.*?)(;o:(?<opt>.*?))?(;f:(?<func>.*?))?(;t:(?<type>.*?))?(;d:(?<def>.*?))?$", RegexOptions.IgnoreCase);

    /// <summary>
    /// Creates a new instance of <see cref="GotOptsCli"/> using <see cref="DefaultSettings"/>.
    /// </summary>
    public GotOptsCli() : base(DefaultSettings) { }

    protected override (IEnumerable<Option> options, IEnumerable<Argument> arguments) CreateOptionsAndArguments()
    {
        return (new[]
        {
            new Option("-a", "Script argument", CommandOptionType.MultipleValue),
            new Option("-b", "Script bottom help text", CommandOptionType.SingleValue),
            new Option("-d", "Script arguments delimiter", CommandOptionType.SingleValue, defaultValue: "|"),
            new Option("-h", "Script help", CommandOptionType.NoValue),
            new Option("-o", "Script option", CommandOptionType.MultipleValue),
            new Option("-p", "Script argument prefix", CommandOptionType.SingleValue),
            new Option("-v", "Script version", CommandOptionType.SingleValue),
        },
        new[]
        {
            new Argument("name", "Script name"),
            new Argument("title", "Script title"),
            new Argument("args", "Script provided arguments encapsulated in box brackets ([]) and delimited with pipes (|)", canBeEmpty: true),
        });
    }

    protected override int Execute()
    {
        using StringWriter stdErrAndOut = new StringWriter();

        // name, title
        CommandLineToolSettings settings = new(Arguments.Value("name"), Arguments.Value("title"))
        {
            Error = stdErrAndOut,
            Out = stdErrAndOut,
            UseAnsiColors = false,
        };

        // -b: bottom help text, -v: version
        if (Options.TryValueAs<string>("b", out string? bht)) settings.BottomHelpText = bht;
        if (Options.TryValueAs<string>("v", out string? ver)) settings.Version = ver;

        // -a: arguments
        Options.Values("a").ForEach(a =>
        {
            Match m = ArgumentPattern.Match(a);
            if (m.Success) settings.Arguments.Add(new Argument(
                    m.Value("name")!,
                    m.Value("desc")!,
                    required: m.Value("req")?.EqualTo("1", "t") ?? true,
                    canBeEmpty: m.Value("cbe")?.EqualTo("1", "t") ?? false,
                    defaultValue: m.Value("def"),
                    valueType: GetTypeCode(m.Value("type")),
                    userData: m.Value("func")));
        });

        // -o: options
        Options.Values("o").ForEach(o =>
        {
            Match m = OptionPattern.Match(o);
            if (m.Success) settings.Options.Add(new Option(
                m.Value("temp")!.Replace(",", "|"),
                m.Value("desc")!,
                optionType: GetOptionType(m.Value("opt")),
                defaultValue: m.Value("def"),
                valueType: GetTypeCode(m.Value("type")),
                userData: m.Value("func")));
        });

        // -h: help
        List<string> cliArguments = new();
        if (Options.HasValue("h")) cliArguments.Add(settings.HelpOptionTemplate!);

        // args: script provided arguments
        string arguments = Arguments.Value("args");
        if (arguments != "")
        {
            if (!arguments.StartsWith('[') || !arguments.EndsWith(']'))
            {
                Error.WriteLine("Invalid arguments provided to script.");
                return 1;
            }
            cliArguments.AddRange(arguments[1..^1].Split(Options.Value("d")));
        }

        ShellScriptCli shellScriptCli = new(settings);
        int cliExitCode = shellScriptCli.Run(cliArguments.ToArray());
        string cliText = stdErrAndOut.ToString();

        if (cliExitCode != 0)
        {
            Out.Write(cliText);
            return cliExitCode;
        }

        // is text not script argument output?
        if (!cliText.StartsWith('[') || !cliText.EndsWith(']'))
        {
            Out.Write(cliText);
            return 1;
        }

        // text is script argument output
        string scriptArgPrefix = Options.ValueOrNull("p") is { } p ? $"{p}_" : "";
        cliText[1..^1].Split('\n', StringSplitOptions.RemoveEmptyEntries).ForEach(l => Out.WriteLine($"{scriptArgPrefix}{l}"));

        return 0;
    }

    private static CommandOptionType GetOptionType(string? type) => type switch
    {
        "m" => CommandOptionType.MultipleValue,
        "s" => CommandOptionType.SingleValue,
        _ => CommandOptionType.NoValue, // "n"
    };

    private static TypeCode GetTypeCode(string? type) => type switch
    {
        "n" => TypeCode.Decimal,
        _ => TypeCode.String, // "s"
    };
}
