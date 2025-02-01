using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Api.Dto.Institution;

public record InstitutionDto(
    Guid Id,
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
    public static InstitutionDto FromEntity(Data.Models.Organizations.Institution entity, IList<string> imageUrls, string logoUrl) => new(
        entity.Id,
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
