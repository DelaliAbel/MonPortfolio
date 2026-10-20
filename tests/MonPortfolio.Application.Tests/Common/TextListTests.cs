using MonPortfolio.Application.Common;

namespace MonPortfolio.Application.Tests.Common;

public class TextListTests
{
    [Theory]
    [InlineData("C#, Blazor; Azure")]
    [InlineData("C#,Blazor,Azure")]
    [InlineData("  C#  ;  Blazor ,Azure  ")]
    [InlineData("C#\nBlazor\r\nAzure")]
    [InlineData(",C#,, ;Blazor;;Azure,")]
    public void SplitTags_splits_on_commas_semicolons_and_new_lines(string text) =>
        Assert.Equal(["C#", "Blazor", "Azure"], TextList.SplitTags(text));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  , ; \n ")]
    public void SplitTags_returns_empty_list_for_blank_text(string? text) =>
        Assert.Empty(TextList.SplitTags(text));

    [Fact]
    public void SplitTags_keeps_spaces_inside_a_tag() =>
        Assert.Equal(["EF Core", "ASP.NET Core"], TextList.SplitTags("EF Core, ASP.NET Core"));

    [Fact]
    public void SplitLines_returns_one_entry_per_line() =>
        Assert.Equal(["Première", "Deuxième", "Troisième"], TextList.SplitLines("Première\nDeuxième\r\nTroisième"));

    [Fact]
    public void SplitLines_strips_leading_bullets_and_blank_lines() =>
        Assert.Equal(
            ["Puce ronde", "Tiret", "Astérisque", "Sans puce"],
            TextList.SplitLines("• Puce ronde\n\n- Tiret\r\n   * Astérisque  \n-\n•  \nSans puce"));

    [Fact]
    public void SplitLines_keeps_bullets_and_commas_inside_a_line() =>
        Assert.Equal(["C# - .NET, Azure"], TextList.SplitLines("- C# - .NET, Azure"));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \r\n \n ")]
    public void SplitLines_returns_empty_list_for_blank_text(string? text) =>
        Assert.Empty(TextList.SplitLines(text));
}
