using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Api.Dto.Response;

public record SchoolResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string Motto { get; init; }
    public string Type { get; init; }
    public string[] Programs { get; init; }
    public string[] Images { get; set; }
    
    public static SchoolResponseDto FromEntity(School school)
    {
        return new SchoolResponseDto
        {
            Id = school.Id,
            Name = school.Name,
            Description = school.Description,
            Motto = school.Motto,
            Type = school.Type,
            Programs = school.Programs.Length == 0 ? school.Programs.ToArray() : [],   
            Images = school.Images.Count == 0 ? school.Images.Select(img => img.Url).ToArray() : []
        };
    }
}
