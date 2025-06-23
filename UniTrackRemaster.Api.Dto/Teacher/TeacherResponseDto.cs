using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Api.Dto.Teacher;

public record TeacherResponseDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Title,
    Guid InstitutionId,
    Guid? ClassGradeId,
    ProfileStatus Status)
{
    public static TeacherResponseDto FromEntity(Data.Models.Users.Teacher teacher, ApplicationUser user) => new(
        teacher.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        teacher.Title,
        teacher.InstitutionId,
        teacher.ClassGradeId,
        teacher.Status
    );
}
