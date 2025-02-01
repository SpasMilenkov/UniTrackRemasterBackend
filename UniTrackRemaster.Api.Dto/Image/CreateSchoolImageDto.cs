namespace UniTrackRemaster.Api.Dto.Image;

public record ImageDto(
    Guid Id,
    Guid InstitutionId,
    string Url)
{
    public static ImageDto FromEntity(Data.Models.Images.Image entity) => new(
        entity.Id,
        entity.InstitutionId,
        entity.Url
    );
}