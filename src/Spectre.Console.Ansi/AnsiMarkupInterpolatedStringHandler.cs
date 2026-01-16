using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Spectre.Console;

[InterpolatedStringHandler]
public struct AnsiMarkupInterpolatedStringHandler
{
    StringBuilder builder;

    public AnsiMarkupInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        builder = new StringBuilder(literalLength);
    }

    public void AppendLiteral(string s)
    {
        builder.Append(s);
    }

    public void AppendFormatted<T>(T t)
    {
        var text = AnsiMarkup.Escape(t?.ToString());
        builder.Append(text);
    }

    public void AppendFormatted<T>(T t, string format)
        where T : IFormattable
    {
        var text = AnsiMarkup.Escape(t?.ToString(format, CultureInfo.CurrentCulture));
        builder.Append(text);
    }

    public string GetFormattedString()
    {
        return builder.ToString();
    }
}