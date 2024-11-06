using UniTrackRemaster.Data.Models.Location;

namespace UniTrackRemaster.Api.Dto.Response;

public class SchoolAddressResponseDto
{
    public string Country { get; set; }
    public string Settlement { get; set; }
    public string PostalCode { get; set; }
    public string Street { get; set; }
    public Guid SchoolId { get; set; }

    // Constructor
    public SchoolAddressResponseDto(string country, string settlement, string postalCode, string street, Guid schoolId)
    {
        Country = country;
        Settlement = settlement;
        PostalCode = postalCode;
        Street = street;
        SchoolId = schoolId;
    }

    // Static method to map from entity to DTO
    public static SchoolAddressResponseDto FromEntity(SchoolAddress schoolAddress, Guid schoolId)
    {
        return new SchoolAddressResponseDto(
            country: schoolAddress.Country,
            settlement: schoolAddress.Settlement,
            postalCode: schoolAddress.PostalCode,
            street: schoolAddress.Street,
            schoolId: schoolId
        );
    }
}
