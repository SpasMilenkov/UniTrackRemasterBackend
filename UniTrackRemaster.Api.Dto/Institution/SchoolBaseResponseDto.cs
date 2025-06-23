using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Api.Dto.Institution;

public record SchoolBaseResponseDto(
    Guid Id,
    string Name,
    string? Motto,
    string? Description,
    string Type,
    DateTime EstablishedDate,
    string[]? Programs,
    string Email,
    string Phone)
{
    public static SchoolBaseResponseDto FromEntity(School school)
    {
        var institution = school.Institution;
        return new SchoolBaseResponseDto
        (
            school.Id,
            institution.Name,
            institution.Motto,
            institution.Description,
            institution.Type.ToString(),
            institution.EstablishedDate,
            school.Programs,
            institution.Email,
            institution.Phone
        );
    }
}