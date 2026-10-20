namespace MonPortfolio.Domain.Enums;

/// <summary>Nature d'une activité communautaire. Les valeurs sont persistées : ne pas les renuméroter.</summary>
public enum CommunityActivityKind
{
    Talk = 0,
    Meetup = 1,
    Workshop = 2,
    Training = 3,
    Article = 4,
    Livestream = 5,
    Hackathon = 6,
    Mentoring = 7,
    Other = 8
}
