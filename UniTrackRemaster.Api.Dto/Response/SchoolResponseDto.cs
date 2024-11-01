using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Api.Dto.Response;

public record SchoolResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string Moto { get; init; }
    public string Type { get; init; }
    public string[] Programs { get; init; }
    public string[] Images { get; init; }
    
    public static SchoolResponseDto FromEntity(School school)
    {
        return new SchoolResponseDto
        {
            Id = school.Id,
            Name = school.Name,
            Description = school.Description,
            Moto = school.Moto,
            Type = school.Type,
            Programs = school.Programs.ToArray(),   
            Images = school.Images.Select(img => img.Url).ToArray()
        };
    }
}
