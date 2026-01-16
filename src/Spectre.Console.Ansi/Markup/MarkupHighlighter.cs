namespace Spectre.Console;

internal static class MarkupHighlighter
{
    public static string Highlight(string markup, string query, Style style)
    {
        ArgumentNullException.ThrowIfNull(markup);
        ArgumentNullException.ThrowIfNull(query);

        if (query.Length == 0)
        {
            return markup;
        }

        var parts = IndexedMarkupSegment.Parse(markup);
        var plain = string.Concat(parts.Select(p => p.Text));
        var startIndex = plain.IndexOf(query, StringComparison.Ordinal);
        var endIndex = startIndex + query.Length;

        if (startIndex == -1)
        {
            return markup;
        }

        var result = new List<MarkupSegment>();
        var partFound = false;

        foreach (var part in parts)
        {
            // Not found the part with the search expression yet?
            if (!partFound)
            {
                if (startIndex >= part.StartIndex && startIndex <= part.EndIndex)
                {
                    var beginning = part.Text[0..(startIndex - part.StartIndex)];
                    result.Add(new MarkupSegment
                    {
                        Text = beginning,
                        Style = part.Style,
                    });

                    var centerStart = startIndex - part.StartIndex;
                    var centerEnd = Math.Min(endIndex - startIndex, part.Text.Length - beginning.Length);
                    var center = part.Text.Substring(centerStart, centerEnd);
                    result.Add(new MarkupSegment
                    {
                        Text = center,
                        Style = style,
                    });

                    var endStart = part.Text.Length - center.Length - beginning.Length;
                    if (endStart > 0)
                    {
                        // Got an end as well
                        result.Add(new MarkupSegment
                        {
                            Text = part.Text[^endStart..],
                            Style = part.Style,
                        });
                    }

                    partFound = true;
                }
                else
                {
                    result.Add(new MarkupSegment
                    {
                        Text = part.Text,
                        Style = part.Style,
                    });
                }

                continue;
            }

            // Now continue with everything after the query

            if (part.StartIndex < endIndex)
            {
                var remaining = endIndex - part.StartIndex;
                if (remaining > part.Text.Length)
                {
                    result.Add(new MarkupSegment
                    {
                        Text = part.Text,
                        Style = style,
                    });
                }
                else
                {
                    result.Add(new MarkupSegment
                    {
                        Text = part.Text[..remaining],
                        Style = style,
                    });

                    if (remaining < part.Text.Length)
                    {
                        result.Add(new MarkupSegment
                        {
                            Text = part.Text[remaining..],
                            Style = part.Style,
                        });
                    }
                }
            }
            else
            {
                result.Add(new MarkupSegment
                {
                    Text = part.Text,
                    Style = part.Style,
                });
            }
        }

        // Merge and render all the segments
        return string.Concat(
            MarkupSegment.Merge(result)
                .Select(item => item.Render()));
    }
}

file sealed class IndexedMarkupSegment : MarkupSegment
{
    public required int StartIndex { get; init; }
    public int EndIndex => StartIndex + Text.Length;

    public static IndexedMarkupSegment[] Parse(string value)
    {
        var currentIndex = 0;
        return MarkupParser.Parse(value).Select(x =>
        {
            var result = new IndexedMarkupSegment
            {
                Text = x.Text,
                Style = x.Style,
                StartIndex = currentIndex,
            };

            currentIndex += x.Text.Length;
            return result;
        }).ToArray();
    }
}

file class MarkupSegment
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