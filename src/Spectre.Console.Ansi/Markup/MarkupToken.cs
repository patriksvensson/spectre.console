namespace Spectre.Console;

internal sealed class MarkupToken
{
    public MarkupTokenKind Kind { get; }
    public string Lexeme { get; }
    public int Position { get; set; }

    public MarkupToken(MarkupTokenKind kind, string value, int position)
    {
        Kind = kind;
        Lexeme = value ?? throw new ArgumentNullException(nameof(value));
        Position = position;
    }
}

public enum MarkupTokenKind
{
    Text = 0,
    Open,
    Close,
}