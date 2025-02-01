using UniTrackRemaster.Data.Models.Location;

namespace UniTrackRemaster.Api.Dto.Response;

public class SchoolAddressResponseDto
(
 string Country,
 string Settlement,
 string PostalCode,
 string Street,
 Guid SchoolId
)
{
    public static SchoolAddressResponseDto FromEntity(Address address, Guid schoolId)
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
