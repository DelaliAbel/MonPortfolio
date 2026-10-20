using System.Globalization;
using MonPortfolio.Application.Experiences;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Web.Formatting;

/// <summary>Mise en forme française des dates et durées affichées.</summary>
public static class DisplayFormat
{
    private static readonly CultureInfo French = CultureInfo.GetCultureInfo("fr-FR");

    public static string MonthYear(DateOnly date) => Capitalize(date.ToString("MMM yyyy", French));

    /// <summary>« 12 mars 2026 ».</summary>
    public static string DayMonthYear(DateOnly date) => date.ToString("d MMMM yyyy", French);

    public static string Period(ExperienceDto experience)
    {
        if (experience.Type == ExperienceType.Education)
            return (experience.EndDate ?? experience.StartDate).Year.ToString(CultureInfo.InvariantCulture);

        var end = experience.EndDate is { } date ? MonthYear(date) : "Aujourd'hui";
        return $"{MonthYear(experience.StartDate)} — {end}";
    }

    /// <summary>"1 an 4 mois", "8 mois"… (bornes incluses, au mois près).</summary>
    public static string Duration(ExperienceDto experience, DateOnly today)
    {
        var end = experience.EndDate ?? today;
        var months = Math.Max(1, (end.Year - experience.StartDate.Year) * 12 + end.Month - experience.StartDate.Month + 1);
        var (years, rest) = (months / 12, months % 12);

        var parts = new List<string>(2);
        if (years > 0) parts.Add(years == 1 ? "1 an" : $"{years} ans");
        if (rest > 0) parts.Add($"{rest} mois");
        return string.Join(" ", parts);
    }

    public static string DateTime(DateTime utc) =>
        utc.ToLocalTime().ToString("dd MMM yyyy 'à' HH:mm", French);

    public static string Relative(DateTime utc, DateTime utcNow)
    {
        var elapsed = utcNow - utc;
        return elapsed switch
        {
            { TotalMinutes: < 1 } => "à l'instant",
            { TotalHours: < 1 } => $"il y a {(int)elapsed.TotalMinutes} min",
            { TotalDays: < 1 } => $"il y a {(int)elapsed.TotalHours} h",
            { TotalDays: < 7 } => $"il y a {(int)elapsed.TotalDays} j",
            _ => utc.ToLocalTime().ToString("dd MMM yyyy", French)
        };
    }

    private static string Capitalize(string value) =>
        value.Length == 0 ? value : char.ToUpper(value[0], French) + value[1..];
}
