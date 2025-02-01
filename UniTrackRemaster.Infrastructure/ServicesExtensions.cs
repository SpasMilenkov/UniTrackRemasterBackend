using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrganizationServices;
using StorageService;
using UniTrackBackend.Services.Student;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data;
using UniTrackRemaster.Data.Repositories;
using UniTrackRemaster.Messaging;
using UniTrackRemaster.Services.Academics;
using UniTrackRemaster.Services.Admin;
using UniTrackRemaster.Services.Authentication;

namespace Infrastructure;

public static class ServicesExtensions
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(c =>
        {
            c.AddPolicy("AllowOrigin",
                options => options.WithOrigins(
                        "https://unitrack.io:8080",
                        "http://localhost:3000",
                        "http://localhost:8080")
                    .AllowCredentials()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            );
        });
        services.AddLogging();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IImapService, ImapService>();
        services.AddScoped<ISmtpService, SmtpService>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IGradeRepository, GradeRepository>();
        services.AddScoped<IGradeService, GradeService>();
        services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
        services.AddScoped<IAcademicYearService, AcademicYearService>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IMajorRepository, MajorRepository>();
        services.AddScoped<IMajorService, MajorService>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<ISemesterRepository, SemesterRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IFacultyRepository, FacultyRepository>();
        services.AddScoped<IFacultyService, FacultyService>();
        services.AddHostedService<EmailProcessingService>();
        services.AddScoped<IInstitutionRepository, InstitutionRepository>();
        services.AddScoped<IInstitutionService, InstitutionService>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<ISchoolService, SchoolService>();
        services.AddScoped<IUniversityRepository, UniversityRepository>();
        services.AddScoped<IUniversityService, UniversityService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPasswordGenerator, PasswordGenerator>();
        services.AddSingleton<IFirebaseStorageService>(provider => 
            new FirebaseStorageService(
                configuration.GetSection("FirebaseCredentials").GetSection("CredentialsPath").Value ?? string.Empty,
                configuration.GetSection("FirebaseCredentials").GetSection("BucketPath").Value ?? string.Empty
            ));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}