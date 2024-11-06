using UniTrackRemaster.Data.Models.Images;

namespace UniTrackRemaster.Api.Dto.Request;

public record CreateSchoolImageDto
{
    public required Guid SchoolId { get; init; }
    public required string Url { get; init; }

    public static SchoolImage ToEntity(CreateSchoolImageDto dto) => new SchoolImage
    {
        SchoolId = dto.SchoolId,
        Url = dto.Url,
    };
}