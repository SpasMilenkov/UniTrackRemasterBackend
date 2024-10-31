using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Location;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Api.Dto.Request;

public record CreateSchoolApplicationDto
{
    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name should be between 2 and 50 characters.")]
    public required string FirstName { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name should be between 2 and 50 characters.")]
    public required string LastName { get; init; }

    [Required]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public required string Email { get; init; }

    [Required]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public required string Phone { get; init; }

    [Required]
    public AddressRequestDto Address { get; init; }
    
    [Required]
    public required string SchoolName { get; init; }
    
    public Application ToEntity(Guid schoolId) => new Application
    {
        Id = Guid.NewGuid(),
        FirstName = this.FirstName,
        LastName = this.LastName,
        Email = this.Email,
        Phone = this.Phone,
        Address = Address.ToEntity(schoolId)
    };
}