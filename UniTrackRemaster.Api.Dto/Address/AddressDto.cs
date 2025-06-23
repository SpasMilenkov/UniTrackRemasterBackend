using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Address;
public record AddressDto(
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Country name should be between 2 and 100 characters.")] 
    string Country,

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Settlement name should be between 2 and 100 characters.")]
    string Settlement,

    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Street name should be between 2 and 200 characters.")]
    string Street,

    [Required]
    [RegularExpression(@"^\d{4,10}$", ErrorMessage = "Postal code must be between 4 and 10 digits.")]
    string PostalCode)
{
    public static AddressDto FromEntity(Data.Models.Location.Address entity) => new(
        entity.Country,
        entity.Settlement,
        entity.Street,
        entity.PostalCode
    );

    public static Data.Models.Location.Address ToEntity(AddressDto dto) => new()
    {
        Id = Guid.NewGuid(),
        Country = dto.Country,
        Settlement = dto.Settlement,
        Street = dto.Street,
        PostalCode = dto.PostalCode
    };
}
