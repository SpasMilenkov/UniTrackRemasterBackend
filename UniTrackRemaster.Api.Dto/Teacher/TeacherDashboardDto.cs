using UniTrackRemaster.Api.Dto.Absence;
using UniTrackRemaster.Api.Dto.Mark;
using UniTrackRemaster.Api.Dto.Student;
using UniTrackRemaster.Api.Dto.Subject;

namespace UniTrackRemaster.Api.Dto.Teacher;

/// <summary>
/// Teacher dashboard with semester context and grading system integration
/// </summary>
public class TeacherDashboardDto
{
    /// <summary>Teacher ID</summary>
    public Guid TeacherId { get; }

    /// <summary>Teacher's full name</summary>
    public string TeacherName { get; }

    /// <summary>Number of subjects taught by the teacher</summary>
    public int SubjectsCount { get; }

    /// <summary>Total number of students taught by the teacher</summary>
    public int TotalStudents { get; }

    /// <summary>Total number of marks given by the teacher in the current semester</summary>
    public int TotalMarks { get; }

    /// <summary>Total number of absences recorded by the teacher in the current semester</summary>
    public int TotalAbsences { get; }

    /// <summary>List of subjects taught by the teacher</summary>
    public List<SubjectResponseDto> Subjects { get; }

    /// <summary>Students grouped by grade/class</summary>
    public List<StudentsByGradeDto> StudentsByGrade { get; }

    /// <summary>Recent marks given by the teacher (enhanced with grading system)</summary>
    public List<MarkResponseDto> RecentMarks { get; }

    /// <summary>Recent absences recorded by the teacher</summary>
    public List<AbsenceResponseDto> RecentAbsences { get; }

    /// <summary>Average marks by subject (raw 0-100 scores for consistency)</summary>
    public Dictionary<string, decimal> AverageMarksBySubject { get; }

    /// <summary>ID of the current semester this dashboard represents</summary>
    public Guid? SemesterId { get; }

    /// <summary>Name of the current semester this dashboard represents</summary>
    public string? SemesterName { get; }

    public TeacherDashboardDto(
        Guid teacherId,
        string teacherName,
        int subjectsCount,
        int totalStudents,
        int totalMarks,
        int totalAbsences,
        List<SubjectResponseDto> subjects,
        List<StudentsByGradeDto> studentsByGrade,
        List<MarkResponseDto> recentMarks,
        List<AbsenceResponseDto> recentAbsences,
        Dictionary<string, decimal> averageMarksBySubject,
        Guid? semesterId = null,
        string? semesterName = null)
    {
        TeacherId = teacherId;
        TeacherName = teacherName;
        SubjectsCount = subjectsCount;
        TotalStudents = totalStudents;
        TotalMarks = totalMarks;
        TotalAbsences = totalAbsences;
        Subjects = subjects;
        StudentsByGrade = studentsByGrade;
        RecentMarks = recentMarks;
        RecentAbsences = recentAbsences;
        AverageMarksBySubject = averageMarksBySubject;
        SemesterId = semesterId;
        SemesterName = semesterName;
    }
}