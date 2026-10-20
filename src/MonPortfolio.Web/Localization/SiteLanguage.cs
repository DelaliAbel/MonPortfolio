using System.Globalization;
using MonPortfolio.Domain.Common;
using OptionsDefaults = Microsoft.Extensions.Options.Options;

namespace MonPortfolio.Web.Localization;

/// <summary>
/// Langue de la vitrine, portée par l'URL : « / » en français (langue par défaut), « /en » en anglais.
/// Chaque version a donc sa propre adresse, indexable par les moteurs de recherche.
/// </summary>
public static class SiteLanguage
{
    public const string EnglishPrefix = "/en";

    /// <summary>Cookie qui mémorise le choix fait avec le sélecteur FR | EN.</summary>
    public const string CookieName = "lang";

    /// <summary>Paramètre posé par le sélecteur ; le serveur mémorise le choix puis redirige vers l'URL propre.</summary>
    public const string SwitchParameter = "lang";

    public static readonly CultureInfo French = CultureInfo.GetCultureInfo("fr-FR");
    public static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    public static bool IsEnglish => Languages.Current == Languages.English;

    /// <summary>Accueil dans la langue courante.</summary>
    public static string Home => HomeOf(Languages.Current);

    public static string HomeOf(string language) => language == Languages.English ? EnglishPrefix : "/";

    /// <summary>Lien vers une section de l'accueil : « /#projects » ou « /en#projects ».</summary>
    public static string Section(string id) => $"{Home}#{id}";

    /// <summary>Lien du sélecteur de langue vers la version demandée.</summary>
    public static string SwitchTo(string language) => $"{HomeOf(language)}?{SwitchParameter}={language}";

    /// <summary>Nom des options de profil : profile.json pour le français, complété par profile.en.json pour l'anglais.</summary>
    public static string ProfileName => IsEnglish ? Languages.English : OptionsDefaults.DefaultName;

    public static bool IsEnglishPath(PathString path) => path.StartsWithSegments(EnglishPrefix);
}
