namespace Spectre.Console.Tests.Unit;

public sealed class AnsiMarkupTests
{
    public sealed class TheEscapeMethod
    {
        [Theory]
        [InlineData("Hello World", "Hello World")]
        [InlineData("Hello World [", "Hello World [[")]
        [InlineData("Hello World ]", "Hello World ]]")]
        [InlineData("Hello [World]", "Hello [[World]]")]
        [InlineData("Hello [[World]]", "Hello [[[[World]]]]")]
        public void Should_Escape_Markup_As_Expected(string input, string expected)
        {
            // Given, When
            var result = AnsiMarkup.Escape(input);

            // Then
            result.ShouldBe(expected);
        }
    }

    public sealed class TheRemoveMethod
    {
        [Theory]
        [InlineData("Hello World", "Hello World")]
        [InlineData("Hello [blue]World", "Hello World")]
        [InlineData("Hello [blue]World[/]", "Hello World")]
        public void Should_Remove_Markup_From_Text(string input, string expected)
        {
            // Given, When
            var result = AnsiMarkup.Remove(input);

            // Then
            result.ShouldBe(expected);
        }
    }

    [Theory]
    [InlineData("Hello", "World", "\e[38;5;11mHello\e[0m \e[38;5;9mWorld\e[0m 2021-02-03")]
    [InlineData("Hello", "World [", "\e[38;5;11mHello\e[0m \e[38;5;9mWorld [\e[0m 2021-02-03")]
    [InlineData("Hello", "World ]", "\e[38;5;11mHello\e[0m \e[38;5;9mWorld ]\e[0m 2021-02-03")]
    [InlineData("[Hello]", "World", "\e[38;5;11m[Hello]\e[0m \e[38;5;9mWorld\e[0m 2021-02-03")]
    [InlineData("[[Hello]]", "[World]", "\e[38;5;11m[[Hello]]\e[0m \e[38;5;9m[World]\e[0m 2021-02-03")]
    public void Should_Escape_Markup_When_Using_MarkupInterpolated(string input1, string input2, string expected)
    {
        // Given
        var output = new StringWriter();
        var markup = new AnsiMarkup(new AnsiWriter(output));
        var date = new DateTime(2021, 2, 3);

        // When
        markup.Write($"[yellow]{input1}[/] [red]{input2}[/] {date:yyyy-MM-dd}");

        // Then
        output.Flush();
        output.ToString().ShouldBe(expected);
    }
}
