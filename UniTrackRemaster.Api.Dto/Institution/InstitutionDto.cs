using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Api.Dto.Address;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Institution;

public record InstitutionDto(
    Guid Id,
    Guid? SchoolId,
    Guid? UniversityId,
    [Required]
    [StringLength(100, MinimumLength = 2)]
    string Name,
    [StringLength(255)]
    string? Description,
    InstitutionType Type,
    LocationType Location,
    IList<AccreditationType> AccreditationTypes,
    AddressDto Address,
    IList<string>? Images,
    string LogoUrl,
    DateTime EstablishedDate,
    [Url]
    string? Website,
    [Required]
    [EmailAddress]
    string Email,
    [Required]
    [Phone]
    string Phone,
    string? Motto,
    IntegrationStatus IntegrationStatus)
{
    public static InstitutionDto FromEntity(Data.Models.Organizations.Institution entity, IList<string> imageUrls, string logoUrl, Guid? schoolId = null, Guid? universityId = null) => new(
        entity.Id,
        schoolId,
        universityId,
        entity.Name,
        entity.Description,
        entity.Type,
        entity.Location,
        entity.Accreditations,
        AddressDto.FromEntity(entity.Address),
        imageUrls,
        logoUrl,
        entity.EstablishedDate,
        entity.Website,
        entity.Email,
        entity.Phone,
        entity.Motto,
        entity.IntegrationStatus
    );
}
