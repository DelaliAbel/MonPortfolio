using System.Globalization;
using MonPortfolio.Domain.Common;

namespace MonPortfolio.Domain.Tests.Common;

/// <summary>Textes bilingues : le français fait foi, l'anglais absent se rabat sur le français.</summary>
public class LocalizedTextTests
{
    [Fact]
    public void In_returns_the_translation_or_falls_back_to_french()
    {
        var translated = new LocalizedText("Projets", "Projects");
        var untranslated = new LocalizedText("Fintech");

        Assert.Equal("Projects", translated.In(Languages.English));
        Assert.Equal("Projets", translated.In(Languages.French));
        Assert.Equal("Fintech", untranslated.In(Languages.English));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_translation_means_no_translation(string? english)
    {
        var text = new LocalizedText("Titre", english);

        Assert.Null(text.En);
        Assert.False(text.HasEnglish);
    }

    [Fact]
    public void ToString_follows_the_current_interface_language()
    {
        var text = new LocalizedText("Voir le projet", "View project");
        var original = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            Assert.Equal("View project", text.ToString());
            Assert.Equal(Languages.English, Languages.Current);

            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            Assert.Equal("Voir le projet", text.ToString());
            Assert.Equal(Languages.French, Languages.Current);
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    [Fact]
    public void Plain_string_converts_to_a_french_text()
    {
        LocalizedText text = "Portfolio";
        LocalizedText? none = (string?)null;

        Assert.Equal(new LocalizedText("Portfolio"), text);
        Assert.Null(none);
    }

    [Fact]
    public void Lists_compare_by_content_and_fall_back_to_french()
    {
        var list = new LocalizedList(["Livraison"], ["Delivery"]);
        var untranslated = LocalizedList.Of(["Livraison"]);

        Assert.Equal(new LocalizedList(["Livraison"], ["Delivery"]), list);
        Assert.NotEqual(untranslated, list);
        Assert.Equal(["Delivery"], list.In(Languages.English));
        Assert.Equal(["Livraison"], untranslated.In(Languages.English));
        Assert.False(untranslated.HasEnglish);
    }
}
