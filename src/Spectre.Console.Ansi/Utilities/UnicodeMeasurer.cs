namespace Spectre.Console;

internal static class UnicodeMeasurer
{
    private const sbyte Sentinel = -2;

    /// <summary>
    /// UnicodeCalculator.GetWidth documents the width as (-1, 0, 1, 2). We only need space for these values and a sentinel for uninitialized values.
    /// This is only five values in total so we are storing one byte per value. We could store 2 per byte but that would add more logic to the retrieval.
    /// We should add one to char.MaxValue because the total number of characters includes \0 too so there are 65536 valid chars.
    /// </summary>
    private static readonly sbyte[] _runeWidthCache = new sbyte[char.MaxValue + 1];

    static UnicodeMeasurer()
    {
#if !NETSTANDARD2_0
        Array.Fill(_runeWidthCache, Sentinel);
#else
        for (var i = 0; i < _runeWidthCache.Length; i++)
        {
            _runeWidthCache[i] = Sentinel;
        }
#endif
    }

    public static int GetCellLength(string text)
    {
        // TODO: Investigate why `UnicodeCalculator.GetWidth` fails for strings.
        // That version takes a lot of other things into account that we probably want.
        return text.Sum(GetCellLength);
    }

    public static int GetCellLength(char character)
    {
        // TODO: We need to figure out why Segment.SplitLines fails
        // if we let wcwidth (which returns -1 instead of 1)
        // calculate the size for new line characters.
        // That is correct from a Unicode perspective, but the
        // algorithm was written before wcwidth was added and used
        // to work with string length and not cell length.
        if (character == '\n')
        {
            return 1;
        }

        var width = _runeWidthCache[character];
        if (width == Sentinel)
        {
            width = _runeWidthCache[character] = (sbyte)UnicodeCalculator.GetWidth(character);
        }

        return width;
    }
}