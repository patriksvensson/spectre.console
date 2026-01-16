using System.Text;

namespace Spectre.Console;

public sealed class AnsiMarkup(AnsiWriter writer)
{
    private readonly AnsiWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));

    public static IEnumerable<(string Text, Style Style)> Parse(string markup, Style? style = null)
    {
        return MarkupParser.Parse(markup, style);
    }

    /// <summary>
    /// Escapes text so that it won’t be interpreted as markup.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>A string that is safe to use in markup.</returns>
    public static string Escape(string? text)
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
    public static string Remove(string? text)
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
                result.Append(tokenizer.Current.Lexeme);
            }
        }

        return result.ToString();
    }

    public void Write(string markup, Style? style = null)
    {
        var parts = Parse(markup, style).ToArray();
        foreach (var (segmentText, segmentStyle) in Parse(markup, style))
        {
            if (!segmentStyle.Equals(Style.Plain))
            {
                _writer.Style(segmentStyle);
            }

            _writer.Write(segmentText);

            if (!segmentStyle.Equals(Style.Plain))
            {
                _writer.ResetStyle();
            }
        }
    }

#if !NETSTANDARD
    public void Write(ref AnsiMarkupInterpolatedStringHandler markup, Style? style = null)
    {
        Write(markup.GetFormattedString(), style);
    }
#endif

    public void WriteLine(string markup)
    {
        Write(markup);
        _writer.WriteLine();
    }
}