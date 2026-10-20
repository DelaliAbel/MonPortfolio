using System.ComponentModel.DataAnnotations;
using MonPortfolio.Application.Experiences;

namespace MonPortfolio.Application.Tests.Experiences;

public class ExperienceInputTests
{
    private static readonly DateOnly Start = new(2022, 1, 1);

    private static ExperienceInput Input(DateOnly? endDate, bool isCurrent) => new()
    {
        Title = "Développeur .NET",
        Organization = "Contoso",
        StartDate = Start,
        EndDate = endDate,
        IsCurrent = isCurrent
    };

    private static List<ValidationResult> Validate(ExperienceInput input) =>
        input.Validate(new ValidationContext(input)).ToList();

    [Fact]
    public void Finished_experience_without_end_date_is_invalid()
    {
        var error = Assert.Single(Validate(Input(endDate: null, isCurrent: false)));

        Assert.Contains(nameof(ExperienceInput.EndDate), error.MemberNames);
    }

    [Fact]
    public void End_date_before_start_date_is_invalid()
    {
        var error = Assert.Single(Validate(Input(endDate: Start.AddDays(-1), isCurrent: false)));

        Assert.Contains(nameof(ExperienceInput.EndDate), error.MemberNames);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(365)]
    public void End_date_on_or_after_start_date_is_valid(int days) =>
        Assert.Empty(Validate(Input(endDate: Start.AddDays(days), isCurrent: false)));

    [Fact]
    public void Current_position_needs_no_end_date() =>
        Assert.Empty(Validate(Input(endDate: null, isCurrent: true)));

    [Fact]
    public void Current_position_ignores_an_inconsistent_end_date()
    {
        var input = Input(endDate: Start.AddYears(-1), isCurrent: true);

        Assert.Empty(Validate(input));
        Assert.Null(input.EffectiveEndDate);
    }

    [Fact]
    public void EffectiveEndDate_is_end_date_when_not_current() =>
        Assert.Equal(Start.AddDays(10), Input(endDate: Start.AddDays(10), isCurrent: false).EffectiveEndDate);
}
