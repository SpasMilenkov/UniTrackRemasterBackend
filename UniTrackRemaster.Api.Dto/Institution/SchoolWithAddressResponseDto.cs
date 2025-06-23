using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Api.Dto.Institution;

public record SchoolWithAddressResponseDto(
    Guid Id,
    string Name,
    string? Motto,
    string? Description,
    DateTime EstablishedDate,
    string Type,
    string[]? Programs,
    string Email,
    string Phone,
    string Address,
    string LogoUrl,
    IList<string> Images
) : SchoolBaseResponseDto(Id, Name, Motto, Description, Type, EstablishedDate, Programs, Email, Phone)
{
    public static SchoolWithAddressResponseDto FromEntity(School school, string signedLogoUrl, IList<string> signedUrls, Data.Models.Location.Address address)
    {
        var institution = school.Institution;
        return new SchoolWithAddressResponseDto
        (
            school.Id,
            institution.Name,
            institution.Motto,
            institution.Description,
            institution.EstablishedDate,
            institution.Type.ToString(),
            school.Programs,  // This is school-specific
            institution.Email,
            institution.Phone,
            $"{address.Street}, {address.Settlement}, {address.PostalCode}, {address.Country}",
            signedLogoUrl,
            signedUrls
        );
    }
}
