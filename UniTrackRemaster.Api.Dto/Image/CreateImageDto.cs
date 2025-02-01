using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Image;
public record CreateImageDto(
    [Required]
    Guid InstitutionId,
    
    [Required]
    [Url(ErrorMessage = "Please provide a valid URL")]
    string Url)
{
    public static Data.Models.Images.Image ToEntity(CreateImageDto dto) => new()
    {
        Id = Guid.NewGuid(),
        InstitutionId = dto.InstitutionId,
        Url = dto.Url
    };
}