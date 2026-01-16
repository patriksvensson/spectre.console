#if NET10_0_OR_GREATER
using System.Threading;

namespace Spectre.Console;

public static class SystemConsoleExtensions
{
    private static readonly Lock _lock = new();
    private static AnsiWriter? _writer;
    private static AnsiMarkup? _markup;

    private static (AnsiWriter, AnsiMarkup) GetAnsiWriter()
    {
        _writer ??= new AnsiWriter(System.Console.Out);
        _markup ??= new AnsiMarkup(_writer);
        return (_writer, _markup);
    }

    extension(System.Console)
    {
        public static void Ansi(Action<AnsiWriter> action)
        {
            lock (_lock)
            {
                var (writer, _) = GetAnsiWriter();
                action(writer);
            }
        }

        public static void Markup(string markup)
        {
            lock (_lock)
            {
                var (_, writer) = GetAnsiWriter();
                writer.Write(markup);
            }
        }

        public static void MarkupLine(string markup)
        {
            lock (_lock)
            {
                var (_, writer) = GetAnsiWriter();
                writer.WriteLine(markup);
            }
        }
    }
}
#endif