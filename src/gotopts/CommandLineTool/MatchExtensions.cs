using System.Text.RegularExpressions;

namespace Kokoabim.CommandLineTools;

public static class MatchExtensions
{
    /// <summary>
    /// Gets value of named group if it exists and is successful, otherwise returns null.
    /// </summary>
    public static string? Value(this Match m, string name) => m.Groups[name] is Group g && g.Success ? g.Value : null;
}