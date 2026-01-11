namespace Spectre.Console.Abstractions;

public sealed class AnsiMarkup
{
    private readonly AnsiWriter _writer;

    public AnsiMarkup(AnsiWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public static IEnumerable<(string Text, Style Style)> Parse(string markup)
    {
        return MarkupParser.Parse(markup);
    }

    public void Write(string markup)
    {
        foreach (var (text, style) in MarkupParser.Parse(markup))
        {
            if (!style.Equals(Style.Plain))
            {
                _writer.Style(style);
            }

            _writer.Write(text);

            if (!style.Equals(Style.Plain))
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