using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console.Rendering;

namespace Spectre.Console
{
    /// <summary>
    /// Represents a hex viewer.
    /// </summary>
    public sealed class HexView : Renderable
    {
        private const int Extra = 5;

        /// <summary>
        /// Gets or sets the requested bytes per row.
        /// </summary>
        public int BytesPerRow { get; set; } = 16;

        /// <summary>
        /// Gets or sets the data to visualize.
        /// </summary>
        public byte[]? Bytes { get; set; }

        /// <inheritdoc/>
        protected override IEnumerable<Segment> Render(RenderContext context, int maxWidth)
        {
            if (Bytes == null)
            {
                return Enumerable.Empty<Segment>();
            }

            var totalBytes = CalculateBytesPerRow(maxWidth);

            // Create the table
            var table = new Table()
                .HideHeaders()
                .RoundedBorder()
                .AddColumn(new TableColumn(string.Empty).Padding(1, 0))
                .AddColumn(new TableColumn(string.Empty).Padding(0, 0));

            var hex = new Paragraph();
            var representation = new Paragraph();

            foreach (var (_, _, lastRow, row) in Batch(Bytes, totalBytes).Enumerate())
            {
                var columns = row.Select(b => GetMarkup(context, b));

                foreach (var (columnIndex, _, lastColumn, column) in columns.Enumerate())
                {
                    hex.Append(column.Hex, new Style(foreground: column.Color));
                    representation.Append(column.Repr, new Style(foreground: column.Color));

                    if (!lastColumn)
                    {
                        hex.Append(" ", Style.Plain);
                    }
                }

                if (!lastRow)
                {
                    hex.Append("\n", Style.Plain);
                    representation.Append("\n", Style.Plain);
                }
            }

            // Add everything as single row.
            // Could of course add multiple rows here, but
            // for large files, it's too slow since it
            // involves a lot of computation. We should optimize that...
            table.AddRow(hex, representation);

            // Render the table
            return ((IRenderable)table).Render(context, maxWidth);
        }

        private int CalculateBytesPerRow(int maxWidth)
        {
            var total = maxWidth - Extra;

            var totalBytes = 0;
            for (var i = BytesPerRow; i > 0; i--)
            {
                var bytes = ((i * 2) + i - 1) + i;
                if (bytes <= total)
                {
                    totalBytes = i;
                    break;
                }
            }

            return totalBytes;
        }

        private (string Hex, string Repr, Color Color) GetMarkup(RenderContext context, byte b)
        {
            var hex = b.ToString("X2");

            if (b == 0)
            {
                return (hex, "0", Color.Grey);
            }
            else if ((b >= 1 && b <= 8) || (b >= 14 && b <= 31) || b == 11)
            {
                return (hex, ".", Color.Purple);
            }
            else if (b >= 9 && b <= 13 && b != 11)
            {
                return (hex, "_", Color.Green);
            }
            else if (b >= 32 && b <= 126)
            {
                return (hex, ((char)b).ToString(), Color.Blue);
            }

            //var c = (char)b;
            //if (context.Unicode && char.IsLetterOrDigit(c))
            //{
            //    return (hex, ((char)b).ToString(), Color.Yellow);
            //}

            return (hex, "×", Color.Yellow);
        }

        // https://stackoverflow.com/a/15414467/936
        private static IEnumerable<IEnumerable<T>> Batch<T>(IEnumerable<T> source, int size)
        {
            T[]? bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                {
                    bucket = new T[size];
                }

                bucket[count++] = item;

                if (count != size)
                {
                    continue;
                }

                yield return bucket.Select(x => x);

                bucket = null;
                count = 0;
            }

            // Return the last bucket with all remaining elements
            if (bucket != null && count > 0)
            {
                Array.Resize(ref bucket, count);
                yield return bucket.Select(x => x);
            }
        }
    }
}
