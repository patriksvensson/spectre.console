using Spectre.Console;

namespace Namespace;

public sealed class HighlightTests
{
    private readonly Style _highlightStyle = new(
        foreground: Color.Default,
        background: Color.Yellow,
        Decoration.Bold);

    [Fact]
    public void Should_Return_Same_Value_When_SearchText_Is_Empty()
    {
        // Given, When
        var result = "Sample text"
            .HighlightMarkup(string.Empty, _highlightStyle);

        // Then
        result.ShouldBe("Sample text");
    }

    [Fact]
    public void Should_Highlight_Matched_Text()
    {
        // Given, When
        var result = "Sample text with test word"
            .HighlightMarkup("test", _highlightStyle);

        // Then
        result.ShouldBe("Sample text with [bold on yellow]test[/] word");
    }

    [Fact]
    public void Should_Match_Text_Across_Tokens()
    {
        // Given, When
        var result = "[red]Sample text[/] with test word"
            .HighlightMarkup("text with", _highlightStyle);

        // Then
        result.ShouldBe("[red]Sample [/][bold on yellow]text with[/] test word");
    }

    [Fact]
    public void Should_Highlight_Only_First_Matched_Text()
    {
        // Given, When
        var result = "Sample text with test word"
            .HighlightMarkup("te", _highlightStyle);

        // Then
        result.ShouldBe("Sample [bold on yellow]te[/]xt with test word");
    }

    [Fact]
    public void Should_Not_Match_Text_Outside_Of_Text_Tokens()
    {
        // Given, When
        var result = "[red]Sample text with test word[/]"
            .HighlightMarkup("red", _highlightStyle);

        // Then
        result.ShouldBe("[red]Sample text with test word[/]");
    }

    [Theory]
    [InlineData("Bar Baz", "[red]Foo Bar[/] [blue]Baz[/]", "[red]Foo [/][bold on yellow]Bar Baz[/]")]
    [InlineData("Bar", "[red]Foo Bar[/] [blue]Baz[/]", "[red]Foo [/][bold on yellow]Bar[/] [blue]Baz[/]")]
    [InlineData("Baz Qux", "Foo [red]Bar Baz[/] [green]Qux Corgi[/]", "Foo [red]Bar [/][bold on yellow]Baz Qux[/][green] Corgi[/]")]
    [InlineData("test", "Sample text with test word", "Sample text with [bold on yellow]test[/] word")]
    public void Should_Highlight_Markup_As_Expected(string query, string markup, string expected)
    {
        // Given, When
        var result = markup.HighlightMarkup(
            query, _highlightStyle);

        // Then
        result.ShouldBe(expected);
    }
}