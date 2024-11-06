using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Location;
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
    public required string Code { get; init; }
    public required string Phone { get; init; }
    public required IntegrationStatus Status{ get; init; }
    public required SchoolAddressResponseDto Address { get; init; }
    
    public static ApplicationResponseDto FromEntity(Application application, SchoolAddressResponseDto schoolAddress)
    {
        return new ApplicationResponseDto
        {
            Id = application.Id.ToString(),
            SchoolId = application.SchoolId.ToString(),
            SchoolName = application.School?.Name ?? throw new InvalidOperationException(message: "School value is null for application"),
            Status = application.School?.IntegrationStatus ?? throw new InvalidOperationException(message: "School value is null for application"),
            FirstName = application.FirstName,
            LastName = application.LastName,
            Email = application.Email,
            Code = application.Code,
            Address = schoolAddress,
            Phone = application.Phone,
        };
    }
}
