namespace Spectre.Console;

/// <summary>
/// Contains extension methods for <see cref="char"/>.
/// </summary>
public static partial class CharExtensions
{
    internal static bool IsDigit(this char character, int min = 0)
    {
        return char.IsDigit(character) && character >= (char)min;
    }
}