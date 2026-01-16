using System.Runtime.CompilerServices;
using System.Text;

namespace Spectre.Console;

[InterpolatedStringHandler]
public struct AnsiMarkupInterpolatedStringHandler
{
    private readonly StringBuilder _builder;

    public AnsiMarkupInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        _builder = new StringBuilder(literalLength);
    }

    public void AppendLiteral(string s)
    {
        _builder.Append(s);
    }

    public void AppendFormatted<T>(T t)
    {
        var text = AnsiMarkup.Escape(t?.ToString());
        _builder.Append(text);
    }

    public void AppendFormatted<T>(T t, string format)
        where T : IFormattable
    {
        var text = AnsiMarkup.Escape(t?.ToString(format, CultureInfo.CurrentCulture));
        _builder.Append(text);

    }

    public string GetFormattedString()
    {
        return _builder.ToString();
    }
}