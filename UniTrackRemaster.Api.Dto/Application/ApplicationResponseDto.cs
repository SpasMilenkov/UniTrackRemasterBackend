using UniTrackRemaster.Api.Dto.Address;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Application;

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
    public static ApplicationResponseDto FromEntity(Data.Models.Organizations.Application entity, AddressDto address) => new(
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