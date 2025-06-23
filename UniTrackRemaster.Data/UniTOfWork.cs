using Microsoft.EntityFrameworkCore.Storage;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Data.Context;

namespace UniTrackRemaster.Data;

public sealed class UnitOfWork(
    UniTrackDbContext dbContext,
    IAdminRepository adminRepository,
    IParentRepository parentRepository,
    IGradingSystemRepository gradingSystemRepository,
    IAcademicYearRepository academicYearRepository,
    IApplicationRepository applicationRepository,
    IAbsenceRepository absenceRepository,
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
    IMarkRepository markRepository,
    IEventRepository eventRepository,
    IParticipantRepository participantRepository,
    IOrganizerRepository organizerRepository,
    IAttenderRepository attenderRepository,
    IEventNotificationRepository eventNotificationRepository,
    IInstitutionRepository institutions) : IUnitOfWork
{
    private readonly UniTrackDbContext _dbContext = dbContext;
    private IDbContextTransaction _transaction;
    private bool _disposed;

    public IAdminRepository Admins { get; } = adminRepository;
    public IParentRepository Parents { get; } = parentRepository;
    public IAcademicYearRepository AcademicYears { get; } = academicYearRepository;
    public IGradingSystemRepository GradingSystems { get; } = gradingSystemRepository;
    public IMarkRepository Marks { get; } = markRepository;
    public IApplicationRepository Applications { get; } = applicationRepository;
    public IInstitutionRepository Institutions { get; } = institutions;
    public IAbsenceRepository Absences { get; } = absenceRepository;
    public IUniversityRepository Universities { get; } = universityRepository;
    public IDepartmentRepository Departments { get; } = departmentRepository;
    public IFacultyRepository Faculties { get; } = facultyRepository;
    public IGradeRepository Grades { get; } = gradeRepository;
    public IImageRepository Images { get; } = imageRepository;
    public IMajorRepository Majors { get; } = majorRepository;
    public ISchoolRepository Schools { get; } = schoolRepository;
    public ISemesterRepository Semesters { get; } = semesterRepository;
    public IStudentRepository Students { get; } = studentRepository;
    public ISubjectRepository Subjects { get; } = subjectRepository;
    public ITeacherRepository Teachers { get; } = teacherRepository;
    public IEventRepository Events { get; } = eventRepository;
    public IParticipantRepository Participants { get; } = participantRepository;
    public IOrganizerRepository Organizers { get; } = organizerRepository;
    public IAttenderRepository Attenders { get; } = attenderRepository;
    public IEventNotificationRepository EventNotifications { get; } = eventNotificationRepository;


    private int _transactionCount = 0;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // If this is the first transaction request, create a new transaction
        if (_transactionCount == 0)
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        // Increment the transaction counter
        _transactionCount++;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        // Decrement the transaction counter
        _transactionCount--;

        // Only commit when all nested transactions are complete
        if (_transactionCount == 0)
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
    }


    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;

        // Reset the counter since we're rolling back
        _transactionCount = 0;

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