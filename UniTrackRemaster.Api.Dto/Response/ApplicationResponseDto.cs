using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Api.Dto.Response;

public record class ApplicationResponseDto
{
    public required string Id { get; init; }
    public required string SchoolId { get; init; }
    public required string SchoolName { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }

    // Static factory method to map from Application entity to ApplicationResponseDto
    public static ApplicationResponseDto FromEntity(Application application)
    {
        return new ApplicationResponseDto
        {
            Id = application.Id.ToString(),
            SchoolId = application.SchoolId.ToString(),
            SchoolName = application?.School?.Name ?? throw new InvalidOperationException(message: "School value is null for application"),        // Assuming School is a navigation property
            FirstName = application.FirstName,
            LastName = application.LastName,
            Email = application.Email
        };
    }
}
