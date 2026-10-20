using MonPortfolio.Domain.Enums;

namespace MonPortfolio.Application.Activities;

public sealed record CommunityActivityDto(
    Guid Id,
    string Title,
    CommunityActivityKind Kind,
    DateOnly Date,
    string? Organizer,
    string? Location,
    string? Description,
    string? PublicationUrl,
    string? ImageUrl)
{
    public string KindLabel => ActivityKinds.Label(Kind);
}

public static class ActivityKinds
{
    public static IReadOnlyList<CommunityActivityKind> All { get; } = Enum.GetValues<CommunityActivityKind>();

    public static string Label(CommunityActivityKind kind) => kind switch
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
