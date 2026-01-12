using System.Text;

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
        if (text == null)
        {
            return string.Empty;
        }

        return text
            .ReplaceExact("[", "[[")
            .ReplaceExact("]", "]]");
    }

    /// <summary>
    /// Removes markup from the specified string.
    /// </summary>
    /// <param name="text">The text to remove markup from.</param>
    /// <returns>A string that does not have any markup.</returns>
    public static string RemoveMarkup(this string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var result = new StringBuilder();

        var tokenizer = new MarkupTokenizer(text);
        while (tokenizer.MoveNext() && tokenizer.Current != null)
        {
            if (tokenizer.Current.Kind == MarkupTokenKind.Text)
            {
                result.Append(tokenizer.Current.Value);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Highlights the first text match in provided value.
    /// </summary>
    /// <param name="value">Input value.</param>
    /// <param name="searchText">Text to search for.</param>
    /// <param name="highlightStyle">The style to apply to the matched text.</param>
    /// <returns>Markup of input with the first matched text highlighted.</returns>
    public static string Highlight(this string value, string searchText, Style? highlightStyle)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(searchText);

        if (searchText.Length == 0)
        {
            return value;
        }

        var foundSearchPattern = false;
        var builder = new StringBuilder();
        using var tokenizer = new MarkupTokenizer(value);
        while (tokenizer.MoveNext())
        {
            var token = tokenizer.Current!;

            switch (token.Kind)
            {
                case MarkupTokenKind.Text:
                    {
                        var tokenValue = token.Value;
                        if (tokenValue.Length == 0)
                        {
                            break;
                        }

                        if (foundSearchPattern)
                        {
                            builder.Append(tokenValue);
                            break;
                        }

                        var index = tokenValue.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
                        if (index == -1)
                        {
                            builder.Append(tokenValue);
                            break;
                        }

                        foundSearchPattern = true;
                        var before = tokenValue.Substring(0, index);
                        var match = tokenValue.Substring(index, searchText.Length);
                        var after = tokenValue.Substring(index + searchText.Length);

                        builder
                            .Append(before)
                            .AppendWithStyle(highlightStyle, match)
                            .Append(after);

                        break;
                    }

                case MarkupTokenKind.Open:
                    {
                        builder.Append("[" + token.Value + "]");
                        break;
                    }

                case MarkupTokenKind.Close:
                    {
                        builder.Append("[/]");
                        break;
                    }

                default:
                    {
                        throw new InvalidOperationException("Unknown markup token kind.");
                    }
            }
        }

        return builder.ToString();
    }
}