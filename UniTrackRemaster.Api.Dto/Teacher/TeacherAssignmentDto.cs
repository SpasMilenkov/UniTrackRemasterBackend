using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Api.Dto.Subject;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Api.Dto.Teacher;

// Request DTOs
public record AssignTeacherToGradesDto(
    [Required] IEnumerable<Guid> GradeIds
);

public record UpdateTeacherGradeAssignmentsDto(
    [Required] IEnumerable<Guid> GradeIds
);

public record UnassignTeacherFromGradesDto(
    [Required] IEnumerable<Guid> GradeIds
);

// Response DTOs
public record TeacherWithGradeAssignmentsResponseDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Title,
    Guid InstitutionId,
    string InstitutionName,
    Guid? ClassGradeId,
    string? ClassGradeName,
    ProfileStatus Status,
    IEnumerable<GradeAssignmentResponseDto> AssignedGrades,
    IEnumerable<SubjectResponseDto> AssignedSubjects,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static TeacherWithGradeAssignmentsResponseDto FromEntity(
        Data.Models.Users.Teacher teacher,
        ApplicationUser user,
        IEnumerable<Data.Models.Academical.Grade>? assignedGrades = null,
        IEnumerable<Data.Models.Academical.Subject>? assignedSubjects = null)
    {
        return new TeacherWithGradeAssignmentsResponseDto(
            Id: teacher.Id,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Email: user.Email ?? string.Empty,
            Title: teacher.Title,
            InstitutionId: teacher.InstitutionId,
            InstitutionName: teacher.Institution?.Name ?? string.Empty,
            ClassGradeId: teacher.ClassGradeId == Guid.Empty ? null : teacher.ClassGradeId,
            ClassGradeName: teacher.ClassGrade?.Name,
            Status: teacher.Status,
            AssignedGrades: assignedGrades?.Select(g => GradeAssignmentResponseDto.FromEntity(g, g.Id == teacher.ClassGradeId)) ?? new List<GradeAssignmentResponseDto>(),
            AssignedSubjects: assignedSubjects?.Select(s => SubjectResponseDto.FromEntity(s)) ?? new List<SubjectResponseDto>(),
            CreatedAt: teacher.CreatedAt,
            UpdatedAt: teacher.UpdatedAt
        );
    }
}

public record GradeAssignmentResponseDto(
    Guid Id,
    string Name,
    Guid InstitutionId,
    string InstitutionName,
    Guid AcademicYearId,
    string AcademicYearName,
    int StudentCount,
    bool IsHomeroom,
    DateTime CreatedAt
)
{
    public static GradeAssignmentResponseDto FromEntity(Data.Models.Academical.Grade grade, bool isHomeroom = false)
    {
        return new GradeAssignmentResponseDto(
            Id: grade.Id,
            Name: grade.Name,
            InstitutionId: grade.InstitutionId,
            InstitutionName: grade.Institution?.Name ?? string.Empty,
            AcademicYearId: grade.AcademicYearId,
            AcademicYearName: grade.AcademicYear?.Name ?? string.Empty,
            StudentCount: grade.Students?.Count ?? 0,
            IsHomeroom: isHomeroom,
            CreatedAt: grade.CreatedAt
        );
    }
}

public record GradeWithTeachersResponseDto(
    Guid Id,
    string Name,
    Guid InstitutionId,
    string InstitutionName,
    Guid AcademicYearId,
    string AcademicYearName,
    Guid? HomeRoomTeacherId,
    string? HomeRoomTeacherName,
    int StudentCount,
    IEnumerable<TeacherAssignmentResponseDto> AssignedTeachers,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static GradeWithTeachersResponseDto FromEntity(
        Data.Models.Academical.Grade grade,
        IEnumerable<Data.Models.Users.Teacher>? assignedTeachers = null)
    {
        var homeRoomTeacherName = grade.HomeRoomTeacher?.Teacher?.User != null
            ? $"{grade.HomeRoomTeacher.Teacher.User.FirstName} {grade.HomeRoomTeacher.Teacher.User.LastName}".Trim()
            : null;

        return new GradeWithTeachersResponseDto(
            Id: grade.Id,
            Name: grade.Name,
            InstitutionId: grade.InstitutionId,
            InstitutionName: grade.Institution?.Name ?? string.Empty,
            AcademicYearId: grade.AcademicYearId,
            AcademicYearName: grade.AcademicYear?.Name ?? string.Empty,
            HomeRoomTeacherId: grade.HomeRoomTeacherId == Guid.Empty ? null : grade.HomeRoomTeacherId,
            HomeRoomTeacherName: homeRoomTeacherName,
            StudentCount: grade.Students?.Count ?? 0,
            AssignedTeachers: assignedTeachers?.Select(t => TeacherAssignmentResponseDto.FromEntity(t, t.ClassGradeId == grade.Id)) ?? new List<TeacherAssignmentResponseDto>(),
            CreatedAt: grade.CreatedAt,
            UpdatedAt: grade.UpdatedAt
        );
    }
}

public record TeacherAssignmentResponseDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Title,
    ProfileStatus Status,
    bool IsHomeroom,
    int SubjectCount
)
{
    public static TeacherAssignmentResponseDto FromEntity(Data.Models.Users.Teacher teacher, bool isHomeroom = false)
    {
        return new TeacherAssignmentResponseDto(
            Id: teacher.Id,
            FirstName: teacher.User?.FirstName ?? string.Empty,
            LastName: teacher.User?.LastName ?? string.Empty,
            Email: teacher.User?.Email ?? string.Empty,
            Title: teacher.Title,
            Status: teacher.Status,
            IsHomeroom: isHomeroom,
            SubjectCount: teacher.Subjects?.Count ?? 0
        );
    }
}

public record TeacherGradeAssignmentSummaryDto(
    Guid TeacherId,
    string TeacherName,
    string Email,
    string? Title,
    int TotalGradesAssigned,
    int TotalStudentsImpacted,
    Guid? HomeRoomGradeId,
    string? HomeRoomGradeName,
    IEnumerable<GradeAssignmentSummaryDto> GradeAssignments
)
{
    public static TeacherGradeAssignmentSummaryDto FromEntity(
        Data.Models.Users.Teacher teacher,
        IEnumerable<Data.Models.Academical.Grade> assignedGrades)
    {
        var gradeAssignments = assignedGrades.Select(g => GradeAssignmentSummaryDto.FromEntity(g, g.Id == teacher.ClassGradeId));
        var totalStudents = assignedGrades.Sum(g => g.Students?.Count ?? 0);

        return new TeacherGradeAssignmentSummaryDto(
            TeacherId: teacher.Id,
            TeacherName: $"{teacher.User?.FirstName} {teacher.User?.LastName}".Trim(),
            Email: teacher.User?.Email ?? string.Empty,
            Title: teacher.Title,
            TotalGradesAssigned: assignedGrades.Count(),
            TotalStudentsImpacted: totalStudents,
            HomeRoomGradeId: teacher.ClassGradeId == Guid.Empty ? null : teacher.ClassGradeId,
            HomeRoomGradeName: teacher.ClassGrade?.Name,
            GradeAssignments: gradeAssignments
        );
    }
}

public record GradeAssignmentSummaryDto(
    Guid GradeId,
    string GradeName,
    int StudentCount,
    bool IsHomeroom,
    string AssignmentType
)
{
    public static GradeAssignmentSummaryDto FromEntity(Data.Models.Academical.Grade grade, bool isHomeroom)
    {
        return new GradeAssignmentSummaryDto(
            GradeId: grade.Id,
            GradeName: grade.Name,
            StudentCount: grade.Students?.Count ?? 0,
            IsHomeroom: isHomeroom,
            AssignmentType: isHomeroom ? "Homeroom" : "Teaching"
        ); 
    }
}

// Success response DTOs
public record TeacherGradeAssignmentResultDto(
    bool Success,
    string Message,
    Guid TeacherId,
    IEnumerable<Guid> AssignedGradeIds,
    int TotalGradesAssigned,
    int TotalStudentsImpacted
);

public record BulkTeacherGradeAssignmentResultDto(
    bool Success,
    string Message,
    int TotalTeachersProcessed,
    int SuccessfulAssignments,
    int FailedAssignments,
    IEnumerable<string> Errors
);