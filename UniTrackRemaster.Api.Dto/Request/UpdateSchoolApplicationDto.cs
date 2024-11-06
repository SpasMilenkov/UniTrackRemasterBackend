using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Api.Dto.Request;

public class UpdateSchoolApplicationDto
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Email { get; init; }

    public required string Phone { get; init; }

    public required AddressRequestDto Address { get; init; }
    
    public required string SchoolName { get; init; }
    
    public required Guid SchoolId { get; init; }
    
    
    public static Application ToEntity(UpdateSchoolApplicationDto application) => new Application
    {
        Id = Guid.NewGuid(),
        FirstName = application.FirstName,
        LastName = application.LastName,
        Email = application.Email,
        Phone = application.Phone,
        SchoolId = application.SchoolId,
    };
}