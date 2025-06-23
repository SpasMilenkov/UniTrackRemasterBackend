using UniTrackRemaster.Api.Dto.Teacher;
using UniTrackRemaster.Api.Dto.Teacher.Analytics;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Commons.Repositories;

/// <summary>
///  Students grouped by grade
/// </summary>
public class StudentGradeGroup
{
    public Guid GradeId { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public List<Student> Students { get; set; } = new();
}

public interface ITeacherRepository : IRepository<Teacher>
{
    // Basic CRUD
    Task<Teacher?> GetByIdAsync(Guid id);
    Task<IEnumerable<Teacher>> GetAllAsync();
    Task<Teacher> CreateAsync(Teacher teacher);
    Task UpdateAsync(Teacher teacher);
    Task DeleteAsync(Guid id);
    Task<Teacher?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Teacher>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<Teacher?> GetByIdWithUserAsync(Guid id);
    Task<IEnumerable<Teacher>> GetAllWithUsersAsync();
    Task<IEnumerable<Teacher>> GetByInstitutionIdWithUsersAsync(Guid institutionId);
    Task<Teacher?> GetByUserIdWithUserAsync(Guid userId);
    Task<IEnumerable<Teacher>> GetByInstitutionIdAsync(Guid institutionId);

    // Search and filtering
    Task<(IEnumerable<Teacher> Teachers, int TotalCount)> SearchTeachersAsync(TeacherSearchParams searchParams);

    // Status and pending management
    Task<IEnumerable<Teacher>> GetPendingByUserIdAsync(Guid userId);
    Task<IEnumerable<Teacher>> GetByInstitutionAsync(Guid institutionId, ProfileStatus? status = null);
    Task<IEnumerable<Teacher>> GetByStatusAsync(ProfileStatus status);

    // Dashboard and analytics with semester support
    Task<(int SubjectsCount, int TotalStudents, int TotalMarks, int TotalAbsences, List<Subject> Subjects,
        List<StudentGradeGroup> StudentsByGrade, List<Mark> RecentMarks, List<Absence> RecentAbsences,
        Dictionary<string, decimal> AverageMarksBySubject)> GetTeacherDashboardDataAsync(Guid teacherId,
        Guid? semesterId);

    Task<IEnumerable<Absence>> GetTeacherAttendanceDataAsync(
        Guid teacherId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? gradeId = null,
        Guid? subjectId = null,
        Guid? semesterId = null);

    Task<IEnumerable<StudentGradeGroup>> GetStudentsBySubjectAndGradeAsync(Guid teacherId, Guid subjectId);

    // Analytics methods with semester support
    Task<IEnumerable<AtRiskStudentDto>> GetStudentAbsenceDataAsync(Guid teacherId, Guid? gradeId = null,
        Guid? subjectId = null, DateTime? fromDate = null, DateTime? toDate = null, Guid? semesterId = null);

    Task<AttendanceStatisticsDto> GetAttendanceStatisticsAsync(Guid teacherId, DateTime? fromDate = null,
        DateTime? toDate = null, Guid? gradeId = null, Guid? subjectId = null, Guid? semesterId = null);

    Task<IEnumerable<SubjectExcusedUnexcusedDto>> GetTeacherAbsencesBreakdownAsync(Guid teacherId,
        DateTime? fromDate = null, DateTime? toDate = null, Guid? gradeId = null, Guid? subjectId = null,
        Guid? semesterId = null);

    // Grade assignment management
    Task AssignGradesAsync(Guid teacherId, IEnumerable<Guid> gradeIds);
    Task UnassignGradesAsync(Guid teacherId, IEnumerable<Guid> gradeIds);
    Task<IEnumerable<Grade>> GetAssignedGradesAsync(Guid teacherId);
    Task UpdateGradeAssignmentsAsync(Guid teacherId, IEnumerable<Guid> gradeIds);
    Task<Teacher?> GetTeacherWithGradesAsync(Guid teacherId);
}