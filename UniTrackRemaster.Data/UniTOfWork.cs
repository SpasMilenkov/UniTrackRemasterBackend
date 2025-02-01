using Microsoft.EntityFrameworkCore.Storage;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Context;

namespace UniTrackRemaster.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly UniTrackDbContext _dbContext;
    private IDbContextTransaction _transaction;
    private bool _disposed;

    public IAdminRepository Admins { get; }
    public IApplicationRepository Applications { get; }
    public IInstitutionRepository Institutions { get; }
    public IAttendanceRepository Attendances { get; }
    public IUniversityRepository Universities { get; }
    public ICourseRepository Courses { get; }
    public IDepartmentRepository Departments { get; }
    public IFacultyRepository Faculties { get; }
    public IGradeRepository Grades { get; }
    public IImageRepository Images { get; }
    public IMajorRepository Majors { get; }
    public ISchoolRepository Schools { get; }
    public ISemesterRepository Semesters { get; }
    public IStudentRepository Students { get; }
    public ISubjectRepository Subjects { get; }
    public ITeacherRepository Teachers { get; }

    public UnitOfWork(
        UniTrackDbContext dbContext,
        IAdminRepository adminRepository,
        IApplicationRepository applicationRepository,
        IAttendanceRepository attendanceRepository,
        ICourseRepository courseRepository,
        IDepartmentRepository departmentRepository,
        IFacultyRepository facultyRepository,
        IGradeRepository gradeRepository,
        IImageRepository imageRepository,
        IMajorRepository majorRepository,
        ISchoolRepository schoolRepository,
        ISemesterRepository semesterRepository,
        IStudentRepository studentRepository,
        ISubjectRepository subjectRepository,
        ITeacherRepository teacherRepository,
        IUniversityRepository universityRepository,
        IInstitutionRepository institutions)
    {
        _dbContext = dbContext;
        Admins = adminRepository;
        Applications = applicationRepository;
        Attendances = attendanceRepository;
        Courses = courseRepository;
        Departments = departmentRepository;
        Faculties = facultyRepository;
        Grades = gradeRepository;
        Images = imageRepository;
        Majors = majorRepository;
        Schools = schoolRepository;
        Semesters = semesterRepository;
        Students = studentRepository;
        Subjects = subjectRepository;
        Teachers = teacherRepository;
        Universities = universityRepository;
        Institutions = institutions;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;
        
        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _dbContext.Dispose();
            _transaction?.Dispose();
        }
        _disposed = true;
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (_disposed) return;
        
        await _dbContext.DisposeAsync();
        if (_transaction is not null)
            await _transaction.DisposeAsync();
        
        _disposed = true;
    }
}