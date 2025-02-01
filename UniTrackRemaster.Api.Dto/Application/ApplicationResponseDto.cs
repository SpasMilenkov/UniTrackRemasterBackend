using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Api.Dto.Response;

public record ApplicationResponseDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Code,
    ApplicationStatus Status,
    InstitutionDetails Institution)
{
    public static ApplicationResponseDto FromEntity(Application entity, AddressDto address) => new(
        entity.Id,
        entity.FirstName,
        entity.LastName,
        entity.Email,
        entity.Phone,
        entity.Code,
        entity.Status,
        new InstitutionDetails(
            entity.Institution.Id,
            entity.Institution.Name,
            entity.Institution.Type,
            address
        )
    );
}

public record InstitutionDetails(
    Guid Id,
    string Name,
    InstitutionType Type,
    AddressDto Address
);