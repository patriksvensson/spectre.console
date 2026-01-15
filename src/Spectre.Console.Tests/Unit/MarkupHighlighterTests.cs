using Spectre.Console;

namespace Namespace;

public class HighlightTests
{
    private readonly Style _highlightStyle = new(
        foreground: Color.Default,
        background: Color.Yellow,
        Decoration.Bold);

    [Fact]
    public void Should_Return_Same_Value_When_SearchText_Is_Empty()
    {
        // Given
        var text = "Sample text";

        // When
        var result = MarkupHighlighter.Highlight(text, string.Empty, new Style());

        // Then
        result.ShouldBe(text);
    }

    [Fact]
    public void Should_Highlight_Matched_Text()
    {
        // Given
        var text = "Sample text with test word";

        // When
        var result = MarkupHighlighter.Highlight(text, "test", _highlightStyle);

        // Then
        result.ShouldBe("Sample text with [bold on yellow]test[/] word");
    }

    [Fact]
    public void Should_Not_Match_Text_Across_Tokens()
    {
        // Given
        var text = "[red]Sample text[/] with test word";

        // When
        var result = MarkupHighlighter.Highlight(text, "text with", _highlightStyle);

        // Then
        result.ShouldBe(text);
    }

    [Fact]
    public void Should_Highlight_Only_First_Matched_Text()
    {
        // Given
        var text = "Sample text with test word";

        // When
        var result = MarkupHighlighter.Highlight(text, "te", _highlightStyle);

        // Then
        result.ShouldBe("Sample [bold on yellow]te[/]xt with test word");
    }

    [Fact]
    public void Should_Not_Match_Text_Outside_Of_Text_Tokens()
    {
        // Given
        var text = "[red]Sample text with test word[/]";

        // When
        var result = MarkupHighlighter.Highlight(
            text, "red", _highlightStyle);

        // Then
        result.ShouldBe(text);
    }
}