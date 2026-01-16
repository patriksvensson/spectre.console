namespace Spectre.Console;

/// <summary>
/// Contains extension methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Gets the cell width of the specified text.
    /// </summary>
    /// <param name="text">The text to get the cell width of.</param>
    /// <returns>The cell width of the text.</returns>
    public static int GetCellWidth(this string text)
    {
        // TODO: Investigate why `UnicodeCalculator.GetWidth` fails for strings in Spectre.Console.
        // That version takes a lot of other things into account that we probably want.
        return UnicodeMeasurer.GetCellLength(text);
    }

    internal static string ReplaceExact(this string text, string oldValue, string? newValue)
    {
#if NETSTANDARD2_0
        return text.Replace(oldValue, newValue);
#else
        return text.Replace(oldValue, newValue, StringComparison.Ordinal);
#endif
    }

    /// <summary>
    /// Escapes text so that it won’t be interpreted as markup.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>A string that is safe to use in markup.</returns>
    public static string EscapeMarkup(this string? text)
    {
        return AnsiMarkup.Escape(text);
    }

    /// <summary>
    /// Removes markup from the specified string.
    /// </summary>
    /// <param name="text">The text to remove markup from.</param>
    /// <returns>A string that does not have any markup.</returns>
    public static string RemoveMarkup(this string? text)
    {
        return AnsiMarkup.Remove(text);
    }

    /// <summary>
    /// Highlights the first text match in provided value.
    /// </summary>
    /// <param name="markup">The markup containing text to highlight.</param>
    /// <param name="query">The text to highlight within the markup.</param>
    /// <param name="style">The style to apply to the matched text.</param>
    /// <returns>Markup of input with the first matched text highlighted.</returns>
    public static string HighlightMarkup(this string markup, string query, Style style)
    {
        return MarkupHighlighter.Highlight(markup, query, style);
    }
}