using MonPortfolio.Domain.Common;
using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Activities;

public sealed record CommunityActivityDto(
    Guid Id,
    LocalizedText Title,
    CommunityActivityKind Kind,
    DateOnly Date,
    string? Organizer,
    string? Location,
    LocalizedText? Description,
    string? PublicationUrl,
    string? ImageUrl)
{
    public string KindLabel => ActivityKinds.Label(Kind);

    /// <summary>Textes traduits en anglais (organisateur et lieu, noms propres, ne sont pas traduits).</summary>
    public bool IsTranslated => Title.HasEnglish && Description?.HasEnglish != false;
}

public static class ActivityKinds
{
    public static IReadOnlyList<CommunityActivityKind> All { get; } = Enum.GetValues<CommunityActivityKind>();

    /// <summary>Libellé dans la langue de l'interface courante.</summary>
    public static string Label(CommunityActivityKind kind) => Languages.Current == Languages.English
        ? kind switch
        {
            CommunityActivityKind.Talk => "Talk",
            CommunityActivityKind.Meetup => "Meetup",
            CommunityActivityKind.Workshop => "Workshop",
            CommunityActivityKind.Training => "Training",
            CommunityActivityKind.Article => "Article",
            CommunityActivityKind.Livestream => "Live / Webinar",
            CommunityActivityKind.Hackathon => "Hackathon",
            CommunityActivityKind.Mentoring => "Mentoring",
            CommunityActivityKind.Other => "Other",
            _ => kind.ToString()
        }
        : kind switch
        {
            CommunityActivityKind.Talk => "Conférence",
            CommunityActivityKind.Meetup => "Meetup",
            CommunityActivityKind.Workshop => "Atelier",
            CommunityActivityKind.Training => "Formation",
            CommunityActivityKind.Article => "Article",
            CommunityActivityKind.Livestream => "Live / Webinaire",
            CommunityActivityKind.Hackathon => "Hackathon",
            CommunityActivityKind.Mentoring => "Mentorat",
            CommunityActivityKind.Other => "Autre",
            _ => kind.ToString()
        };
}
