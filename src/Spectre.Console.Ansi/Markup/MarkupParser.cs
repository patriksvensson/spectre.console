namespace Spectre.Console;

internal static class MarkupParser
{
    public static IEnumerable<(string Text, Style Style)> Parse(string text, Style? style = null)
    {
        ArgumentNullException.ThrowIfNull(text);

        style ??= Style.Plain;

        using var tokenizer = new MarkupTokenizer(text);

        var stack = new Stack<Style>();

        while (tokenizer.MoveNext())
        {
            var token = tokenizer.Current;
            if (token == null)
            {
                break;
            }

            if (token.Kind == MarkupTokenKind.Open)
            {
                var parsedStyle = string.IsNullOrEmpty(token.Lexeme) ? Style.Plain : Style.Parse(token.Lexeme);
                stack.Push(parsedStyle);
            }
            else if (token.Kind == MarkupTokenKind.Close)
            {
                if (stack.Count == 0)
                {
                    throw new InvalidOperationException($"Encountered closing tag when none was expected near position {token.Position}.");
                }

                stack.Pop();
            }
            else if (token.Kind == MarkupTokenKind.Text)
            {
                // Get the effective style.
                var effectiveStyle = style.Combine(stack.Reverse());
                yield return (token.Lexeme, effectiveStyle);
            }
            else
            {
                throw new InvalidOperationException("Encountered unknown markup token.");
            }
        }

        if (stack.Count > 0)
        {
            throw new InvalidOperationException("Unbalanced markup stack. Did you forget to close a tag?");
        }
    }
}