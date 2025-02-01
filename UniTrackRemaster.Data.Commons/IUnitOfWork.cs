namespace UniTrackRemaster.Commons;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IAdminRepository Admins { get; }
    IApplicationRepository Applications { get; }
    IAttendanceRepository Attendances { get; }
    IUniversityRepository Universities { get; }
    ICourseRepository Courses { get; }
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

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}