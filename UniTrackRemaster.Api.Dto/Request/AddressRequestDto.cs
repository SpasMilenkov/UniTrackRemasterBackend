using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Location;

namespace UniTrackRemaster.Api.Dto.Request;


public record AddressRequestDto
{
    public AddressRequestDto(string country, string settlement, string street, string postalCode)
    {
        Country = country;
        Settlement = settlement;
        Street = street;
        PostalCode = postalCode;
    }

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
    
    public static SchoolAddress ToEntity(AddressRequestDto addressRequest)
    {
        return new SchoolAddress()
        {
            Id = Guid.NewGuid(),
            Country = addressRequest.Country,
            Settlement = addressRequest.Settlement,
            Street = addressRequest.Street,
            PostalCode = addressRequest.PostalCode,
        };
    }
}
