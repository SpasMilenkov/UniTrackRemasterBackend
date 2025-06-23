using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Api.Dto.Address;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Institution;

public record UpdateInstitutionDto(
    [Required]
    [StringLength(100, MinimumLength = 2)]
    string Name,
    
    [StringLength(255)]
    string? Description,
    
    InstitutionType Type,
    LocationType Location,
    List<AccreditationType> Accreditations,
    
    [Required]
    AddressDto Address,
    
    [Url]
    string? Website,
    
    [Required]
    [EmailAddress]
    string Email,
    
    [Required]
    [Phone]
    string Phone,
    
    string? Motto)
{
    public void UpdateEntity(Data.Models.Organizations.Institution entity)
    {
        entity.Name = Name;
        entity.Description = Description;
        entity.Type = Type;
        entity.Location = Location;
        entity.Accreditations = Accreditations;
        entity.Address = AddressDto.ToEntity(Address);
        entity.Website = Website;
        entity.Email = Email;
        entity.Phone = Phone;
        entity.Motto = Motto;
    }
}