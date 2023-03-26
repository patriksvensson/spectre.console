namespace Spectre.Console.Cli;

internal static class StringExtensions
{
    internal static int OrdinalIndexOf(this string text, char token)
    {
        return text.IndexOf(token, System.StringComparison.Ordinal);
    }
}