namespace Spectre.Console;

public sealed class AnsiMarkup
{
    private readonly AnsiWriter _writer;

    public AnsiMarkup(AnsiWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public static IEnumerable<(string Text, Style Style)> Parse(string markup, Style? style = null)
    {
        return MarkupParser.Parse(markup, style);
    }

    public void Write(string markup, Style? style = null)
    {
        foreach (var (segmentText, segmentStyle) in MarkupParser.Parse(markup, style))
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

    public void WriteLine(string markup)
    {
        Write(markup);
        _writer.WriteLine();
    }
}