namespace UniTrackRemaster.Api.Dto.Institution;

public class SchoolAddressResponseDto
(
 string Country,
 string Settlement,
 string PostalCode,
 string Street,
 Guid SchoolId
)
{
    public static SchoolAddressResponseDto FromEntity(Data.Models.Location.Address address, Guid schoolId)
    {
        return new SchoolAddressResponseDto(
            address.Country,
            address.Settlement,
            address.PostalCode, 
            address.Street,
            schoolId
        );
    }
}
