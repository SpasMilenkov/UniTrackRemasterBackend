using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Api.Dto.Response;

public record TeacherResponseDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Title,
    Guid InstitutionId,
    Guid? ClassGradeId)
{
    public static TeacherResponseDto FromEntity(Teacher teacher, ApplicationUser user) => new(
        teacher.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        teacher.Title,
        teacher.InstitutionId,
        teacher.ClassGradeId
    );
}
