using System.IO;
using Spectre.Console;

namespace InfoExample
{
    public static class Program
    {
        public static void Main()
        {
            var bytes = File.ReadAllBytes("data.bin");

            var tree = new Tree("📁 Files");
            var node = tree.AddNode("📄 data.bin");

            node.AddNode(new HexView()
            {
                BytesPerRow = 256,
                Bytes = bytes,
            });

            AnsiConsole.Render(tree);
        }
    }
}
