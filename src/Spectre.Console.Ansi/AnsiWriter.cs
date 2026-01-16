using System.IO;

namespace Spectre.Console;

public sealed class AnsiWriter
{
    private readonly TextWriter _writer;
    private readonly ColorSystem _system;
    private readonly List<byte> _codes;

    public AnsiWriter(TextWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _system = ColorSystem.TrueColor;
        _codes = [];
    }

    public AnsiWriter Flush()
    {
        _writer.Flush();
        return this;
    }

    public AnsiWriter Write(string text)
    {
        _writer.Write(text);
        return this;
    }

    public AnsiWriter Write(int number)
    {
        _writer.Write(number);
        return this;
    }

    public AnsiWriter WriteLine()
    {
        _writer.WriteLine();
        return this;
    }

    public AnsiWriter WriteLine(string text)
    {
        _writer.WriteLine(text);
        return this;
    }

    public AnsiWriter ResetStyle()
    {
        WriteSgr(0);
        return this;
    }

    public AnsiWriter Decoration(Decoration decoration)
    {
        _codes.Clear();
        _codes.AddRange(AnsiDecorationBuilder.GetAnsiCodes(decoration));

        WriteSgr(_codes);
        return this;
    }

    public AnsiWriter Background(Color color)
    {
        _codes.Clear();
        _codes.AddRange(AnsiColorBuilder.GetAnsiCodes(_system, color, false));

        WriteSgr(_codes);
        return this;
    }

    public AnsiWriter Foreground(Color color)
    {
        _codes.Clear();
        _codes.AddRange(AnsiColorBuilder.GetAnsiCodes(_system, color, true));

        WriteSgr(_codes);
        return this;
    }

    public AnsiWriter Style(Style style)
    {
        _codes.Clear();
        _codes.AddRange(AnsiDecorationBuilder.GetAnsiCodes(style.Decoration));
        _codes.AddRange(AnsiColorBuilder.GetAnsiCodes(_system, style.Foreground, true));
        _codes.AddRange(AnsiColorBuilder.GetAnsiCodes(_system, style.Background, false));

        WriteSgr(_codes);
        return this;
    }

    // CUP
    public AnsiWriter CursorPosition(int row, int column)
    {
        Write("\e[");
        Write(row);
        Write(";");
        Write(column);
        Write("H");
        return this;
    }

    // CUU
    public AnsiWriter CursorUp(int steps)
    {
        Write("\e[");
        Write(steps);
        Write("A");
        return this;
    }

    // CUD
    public AnsiWriter CursorDown(int steps)
    {
        Write("\e[");
        Write(steps);
        Write("B");
        return this;
    }

    // CUF
    public AnsiWriter CursorRight(int steps)
    {
        Write("\e[");
        Write(steps);
        Write("C");
        return this;
    }

    // CUB
    public AnsiWriter CursorLeft(int steps)
    {
        Write("\e[");
        Write(steps);
        Write("D");
        return this;
    }

    // SM
    public AnsiWriter ShowCursor()
    {
        Write("\e?25h");
        return this;
    }

    // RM
    public AnsiWriter HideCursor()
    {
        Write("\e?25l");
        return this;
    }

    // EL(0)
    public AnsiWriter ClearLineFromCursor()
    {
        Write("\e?0K");
        return this;
    }

    // EL(1)
    public AnsiWriter ClearLineToCursor()
    {
        Write("\e?1K");
        return this;
    }

    // EL(2)
    public AnsiWriter ClearLine()
    {
        Write("\e?2K");
        return this;
    }

    private void WriteSgr(params List<byte> codes)
    {
        if (codes.Count == 0)
        {
            return;
        }

        Write("\e[");
        Write(string.Join(";", codes));
        Write("m");
    }
}