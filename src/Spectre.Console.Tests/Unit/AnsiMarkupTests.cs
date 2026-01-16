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
    public void Should_Escape_Markup_When_Using_Interpolated_Strings(string input1, string input2, string expected)
    {
        // Given
        var output = new StringWriter();
        var markup = new AnsiMarkup(new AnsiWriter(output));
        var date = new DateTime(2021, 2, 3);

        // When
        markup.Write($"[yellow]{input1}[/] [red]{input2}[/] {date:yyyy-MM-dd}");

        // Then
        output.ToString().ShouldBe(expected);
    }

    [Theory]
    [InlineData("Hello [[ World ]")]
    [InlineData("Hello [[ World ] !")]
    public void Should_Throw_If_Closing_Tag_Is_Not_Properly_Escaped(string input)
    {
        // Given
        var output = new StringWriter();
        var markup = new AnsiMarkup(new AnsiWriter(output));

        // When
        var result = Record.Exception(() => markup.Write(input));

        // Then
        result.ShouldNotBeNull();
        result.ShouldBeOfType<InvalidOperationException>();
        result.Message.ShouldBe("Encountered unescaped ']' token at position 16");
    }

    [Fact]
    public void Should_Escape_Markup_Blocks_As_Expected()
    {
        // Given
        var output = new StringWriter();
        var markup = new AnsiMarkup(new AnsiWriter(output));

        // When
        markup.Write("Hello [[ World ]] !");

        // Then
        output.ToString().ShouldBe("Hello [ World ] !");
    }

    [Fact]
    public void Should_Output_Expected_Ansi_For_Link_With_Url()
    {
        // Given
        var output = new StringWriter();
        var markup = new AnsiMarkup(new AnsiWriter(output));

        // When
        markup.Write("[link=https://patriksvensson.se]Click to visit my blog[/]");

        // Then
        output.ToString().ShouldMatch(
            "]8;id=[0-9]*;https:\\/\\/patriksvensson\\.se\\\\Click to visit my blog]8;;\\\\");
    }

    [Fact]
    public void Should_Output_Expected_Ansi_For_Link_With_Only_Url()
    {
        // Given
        var output = new StringWriter();
        var markup = new AnsiMarkup(new AnsiWriter(output));

        // When
        markup.Write("[link]https://patriksvensson.se[/]");

        // Then
        output.ToString().ShouldMatch(
            "]8;id=[0-9]*;https:\\/\\/patriksvensson\\.se\\\\https:\\/\\/patriksvensson\\.se]8;;\\\\");
    }

    [Fact]
    public void Should_Output_Expected_Ansi_For_Link_With_Bracket_In_Url_Only()
    {
        // Given
        var output = new StringWriter();
        var markup = new AnsiMarkup(new AnsiWriter(output));
        var path = "file://c:/temp/[x].txt";

        // When
        markup.Write($"[link]{path}[/]");

        // Then
        output.ToString().ShouldMatch(
            "]8;id=[0-9]*;file:\\/\\/c:\\/temp\\/\\[x\\].txt\\\\file:\\/\\/c:\\/temp\\/\\[x\\].txt]8;;\\\\");
    }

    [Fact]
    public void Should_Output_Expected_Ansi_For_Link_With_Bracket_In_Url()
    {
        // Given
        var output = new StringWriter();
        var markup = new AnsiMarkup(new AnsiWriter(output));

        // When
        const string Path = "file://c:/temp/[x].txt";
        markup.Write($"[link={Path.EscapeMarkup()}]{Path.EscapeMarkup()}[/]");

        // Then
        output.ToString().ShouldMatch(
            "]8;id=[0-9]*;file:\\/\\/c:\\/temp\\/\\[x\\].txt\\\\file:\\/\\/c:\\/temp\\/\\[x\\].txt]8;;\\\\");
    }

    [Fact]
    public void Should_Not_Fail_With_Brackets_On_Calls_Without_Args()
    {
        // Given
        var output = new StringWriter();
        var markup = new AnsiMarkup(new AnsiWriter(output));

        // When
        markup.WriteLine("{");

        // Then
        output.ToString()
            .NormalizeLineEndings()
            .ShouldBe("{\n");
    }
}
