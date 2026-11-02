namespace BlogKrafter.Tests.Content;

public class SlugsTests
{
    [Theory]
    [InlineData("Hello, World!", "hello-world")]
    [InlineData("  Café crème  ", "cafe-creme")]
    [InlineData("Don't panic", "dont-panic")]
    [InlineData("C# 14 & .NET 10", "c-14-net-10")]
    [InlineData("---", "")]
    [InlineData(null, "")]
    public void Generate_folds_text_into_a_url_safe_slug(string? text, string expected)
    {
        Assert.Equal(expected, Slugs.Generate(text));
    }

    [Fact]
    public void Generate_cuts_long_text_without_a_trailing_hyphen()
    {
        string slug = Slugs.Generate("alpha beta gamma", maxLength: 11);

        Assert.Equal("alpha-beta", slug);
    }

    [Fact]
    public void WithSuffix_keeps_the_result_inside_the_column_length()
    {
        string longSlug = new('a', Constants.MaxLengths.Slug);

        string suffixed = Slugs.WithSuffix(longSlug, 12);

        Assert.Equal(Constants.MaxLengths.Slug, suffixed.Length);
        Assert.EndsWith("-12", suffixed);
    }
}
