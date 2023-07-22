namespace Kokoabim.CommandLineTools;

/// <summary>
/// Shell script command line interface (CLI). For use by <see cref="GotOptsCli"/>.
/// </summary>
public class ShellScriptCli : CommandLineTool
{
    /// <summary>
    /// Creates a new instance of <see cref="ShellScriptCli"/>.
    /// </summary>
    public ShellScriptCli(CommandLineToolSettings settings) : base(settings) { }

    protected override int Execute()
    {
        var inputs = GetInputs(optionPredicate: o => o.HasValue());

        var inputsWithFailedFunctions = inputs.Where(i => i.ValueExists && i.UserData != null).SelectNotNull(i => RunFunction(i));
        if (inputsWithFailedFunctions.Any())
        {
            inputsWithFailedFunctions.ForEach(f => Error.WriteLine($"{Settings.Name}: {f}"));
            return 1;
        }

        Out.Write("[");
        inputs.ForEach(i => Out.WriteLine($"{(i.InputType.HasFlag(CommandLineInputType.Argument) ? "arg" : "opt")}_{i.Name}=\"{string.Join(',', i.ValuesAsObjects())}\""));
        Out.Write("]");

        return 0;
    }

    private string? RunFunction(ICommandLineInput input)
    {
        var value = input.ValueAs<string>();

        string? failure;
        try
        {
            failure = input.UserData switch
            {
                "d" => Directory.Exists(value) ? null : $"directory '{value}' does not exist",
                "!d" => Directory.Exists(value) ? $"directory '{value}' already exists" : null,
                "f" => File.Exists(value) ? null : $"file '{value}' does not exist",
                "!f" => File.Exists(value) ? $"file '{value}' already exists" : null,
                "fail" => "FAIL.",
                _ => null,
            };
        }
        catch (Exception ex)
        {
            failure = $"Error: {ex.Message}";
        }

        return failure != null ? $"{(input.InputType.HasFlag(CommandLineInputType.Argument) ? "argument" : "option")} {input.Name}: {failure}" : null;
    }
}
