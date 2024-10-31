using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Location;

namespace UniTrackRemaster.Api.Dto.Request;


public record AddressRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Country name should be between 2 and 100 characters.")]
    public required string Country { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Settlement name should be between 2 and 100 characters.")]
    public required string Settlement { get; init; }

    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Street name should be between 2 and 200 characters.")]
    public required string Street { get; init; }

    [Required]
    [RegularExpression(@"^\d{4,10}$", ErrorMessage = "Postal code must be between 4 and 10 digits.")]
    public required string PostalCode { get; init; }
    
    public SchoolAddress ToEntity(Guid schoolId)
    {
        return new SchoolAddress()
        {
            Id = Guid.NewGuid(),
            Country = Country,
            Settlement = Settlement,
            Street = Street,
            PostalCode = PostalCode,
            SchoolId = schoolId
        };
    }
}
