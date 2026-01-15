#if NET10_0_OR_GREATER
using System.Threading;

namespace Spectre.Console;

public static class SystemConsoleExtensions
{
    private static readonly Lock _lock = new();
    private static AnsiWriter? _writer;

    private static AnsiWriter GetAnsiWriter()
    {
        _writer ??= new AnsiWriter(System.Console.Out);
        return _writer;
    }

    extension(System.Console)
    {
        public static void Ansi(Action<AnsiWriter> action)
        {
            lock (_lock)
            {
                var writer = GetAnsiWriter();
                action(writer);
            }
        }

        public static void Markup(string markup)
        {
            lock (_lock)
            {
                var writer = GetAnsiWriter();
                writer.Markup(markup);
            }
        }

        public static void MarkupLine(string markup)
        {
            lock (_lock)
            {
                var writer = GetAnsiWriter();
                writer.MarkupLine(markup);
            }
        }
    }
}
#endif