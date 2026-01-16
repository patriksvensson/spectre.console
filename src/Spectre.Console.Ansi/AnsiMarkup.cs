using System.Text;

namespace Spectre.Console;

public sealed class AnsiMarkup(AnsiWriter writer)
{
    private readonly AnsiWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));

    public static IEnumerable<MarkupSegment> Parse(string markup, Style? style = null)
    {
        return Merge(MarkupParser.Parse(markup, style));

        static IEnumerable<MarkupSegment> Merge(IEnumerable<(string Text, Style Style)> items)
        {
            var result = new List<MarkupSegment>();

            foreach (var (item, index) in items.Select((item, index) => (item, index)))
            {
                if (index > 0 && result.Count > 0)
                {
                    if (result[^1].Style.Equals(item.Style))
                    {
                        result[^1].Text += item.Text;
                    }
                    else
                    {
                        result.Add(new MarkupSegment
                        {
                            Text = item.Text,
                            Style = item.Style,
                        });
                    }
                }
                else
                {
                    result.Add(new MarkupSegment
                    {
                        Text = item.Text,
                        Style = item.Style,
                    });
                }
            }

            return result;
        }
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
        var previousStyle = default(Style);
        var shouldReset = false;

        foreach (var segment in Parse(markup, style))
        {
            if (previousStyle != null && !previousStyle.Equals(segment.Style) && shouldReset)
            {
                _writer.ResetStyle();
                shouldReset = false;
            }

            if (previousStyle == null || !previousStyle.Equals(segment.Style))
            {
                if (!segment.Style.Equals(Style.Plain))
                {
                    _writer.Style(segment.Style);
                    shouldReset = true;
                }
            }

            _writer.Write(segment.Text);

            previousStyle = segment.Style;
        }

        if (shouldReset)
        {
            _writer.ResetStyle();
        }
    }

    public void Write(ref AnsiMarkupInterpolatedStringHandler markup, Style? style = null)
    {
        Write(markup.GetFormattedString(), style);
    }

    public void WriteLine(string markup)
    {
        Write(markup);
        _writer.WriteLine();
    }
}


public class MarkupSegment
{
    public required string Text { get; set; }
    public required Style Style { get; init; }

    public string Render()
    {
        return !Style.Equals(Style.Plain)
            ? $"[{Style.ToMarkup()}]{Text}[/]"
            : Text;
    }

    public static IEnumerable<MarkupSegment> Merge(IEnumerable<MarkupSegment> items)
    {
        var result = new List<MarkupSegment>();
        foreach (var (item, index) in items.Select((item, index) => (item, index)))
        {
            if (index > 0 && result.Count > 0)
            {
                if (result[^1].Style.Equals(item.Style))
                {
                    result[^1].Text += item.Text;
                }
                else
                {
                    result.Add(item);
                }
            }
            else
            {
                result.Add(item);
            }
        }

        return result;
    }
}