using UniTrackRemaster.Commons.Repositories;

namespace UniTrackRemaster.Commons;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IAdminRepository Admins { get; }
    IParentRepository Parents { get; }
    IAcademicYearRepository AcademicYears { get; }
    IGradingSystemRepository GradingSystems { get; }
    IApplicationRepository Applications { get; }
    IAbsenceRepository Absences { get; }
    IUniversityRepository Universities { get; }
    IDepartmentRepository Departments { get; }
    IFacultyRepository Faculties { get; }
    IInstitutionRepository Institutions { get; }
    IGradeRepository Grades { get; }
    IImageRepository Images { get; }
    IMajorRepository Majors { get; }
    ISchoolRepository Schools { get; }
    ISemesterRepository Semesters { get; }
    IStudentRepository Students { get; }
    ISubjectRepository Subjects { get; }
    ITeacherRepository Teachers { get; }
    IMarkRepository Marks { get; }
    IEventRepository Events { get; }
    IParticipantRepository Participants { get; }
    IOrganizerRepository Organizers { get; }
    IAttenderRepository Attenders { get; }
    IEventNotificationRepository EventNotifications { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}