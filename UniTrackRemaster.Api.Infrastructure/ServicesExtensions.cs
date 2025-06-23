using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniTrackRemaster.Services.Organization;
using UniTrackRemaster.Services.Storage;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data;
using UniTrackRemaster.Data.Repositories;
using UniTrackRemaster.Services.Messaging;
using UniTrackRemaster.Services.Academics;
using UniTrackRemaster.Services.Authentication;
using UniTrackRemaster.Services.User;
using AspNetCoreRateLimit;
using UniTrackRemaster.Services.Organization.Strategies;
using UniTrackRemaster.Services.Analytics;
using Qdrant.Client;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.AI;
using UniTrackRemaster.Services.Reports;
using UniTrackRemaster.Services.User.Admins;
using UniTrackRemaster.Services.User.Parents;
using UniTrackRemaster.Services.User.Students;
using UniTrackRemaster.Services.User.Teachers;

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
        services.AddMemoryCache();
        
        services.AddHttpClient<IOllamaService, OllamaService>(client =>
        {
            var baseUrl = configuration.GetValue<string>("OllamaSettings:BaseUrl") ?? "http://localhost:11434";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(180);
        });

        // Configure rate limiting
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddScoped<IPdfService, PdfService>();
        // Add all your existing services (keeping them as they are)
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IImapService, ImapService>();
        services.AddScoped<ISmtpService, SmtpService>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ISemesterRepository, SemesterRepository>();
        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<IGradeRepository, GradeRepository>();
        services.AddScoped<IGradeService, GradeService>();
        services.AddScoped<IMarkRepository, MarkRepository>();
        services.AddScoped<IMarkService, MarkService>();
        services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
        services.AddScoped<IAcademicYearService, AcademicYearService>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IMajorRepository, MajorRepository>();
        services.AddScoped<IMajorService, MajorService>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IAbsenceRepository, AbsenceRepository>();
        services.AddScoped<IAbsenceService, AbsenceService>();
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
        services.AddScoped<IMarkAnalyticsService, MarkAnalyticsService>();
        services.AddScoped<IGradingSystemRepository, GradingSystemRepository>();
        services.AddScoped<GradingStrategyFactory>();
        services.AddScoped<IGradingSystemService, GradingSystemService>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IParticipantRepository, ParticipantRepository>();
        services.AddScoped<IOrganizerRepository, OrganizerRepository>();
        services.AddScoped<IAttenderRepository, AttenderRepository>();
        services.AddScoped<IEventNotificationRepository, EventNotificationRepository>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IParticipantService, ParticipantService>();
        services.AddScoped<IOrganizerService, OrganizerService>();
        services.AddScoped<IEventNotificationService, EventNotificationService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUniversityService, UniversityService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPasswordGenerator, PasswordGenerator>();
        services.AddScoped<IConnectionManager, ConnectionManager>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IParentRepository, ParentRepository>();
        services.AddScoped<IParentService, ParentService>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        services.AddScoped<IProfileInvitationService, ProfileInvitationService>();
        
        // OPTIMIZED: Add analytics services with proper configuration
        AddAnalyticsServices(services, configuration);
        
        services.AddScoped<IAssistantService, AssistantService>();
        services.AddSingleton<IFirebaseStorageService>(provider =>
            new FirebaseStorageService(
                configuration.GetSection("FirebaseCredentials").GetSection("CredentialsPath").Value ?? string.Empty,
                configuration.GetSection("FirebaseCredentials").GetSection("BucketPath").Value ?? string.Empty
            ));
    }

    public static IServiceCollection AddAnalyticsServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register QdrantClient with simple URL string
        services.AddSingleton<QdrantClient>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var host = config.GetValue<string>("QdrantSettings:Host") ?? "localhost";
            var port = config.GetValue<int>("QdrantSettings:Port", 6333);
            var useTls = config.GetValue<bool>("QdrantSettings:UseTls", false);
            var apiKey = config.GetValue<string>("QdrantSettings:ApiKey");
    
            // Convert empty string to null
            if (string.IsNullOrEmpty(apiKey))
                apiKey = null;
    
            return new QdrantClient(
                host: host,
                port: port,
                https: useTls,
                apiKey: apiKey
            );
        });

        services.AddScoped<IQdrantService, QdrantService>();

        services.AddHttpClient<OllamaEmbeddingService>(client =>
        {
            var baseUrl = configuration.GetValue<string>("OllamaSettings:BaseUrl") ?? "http://localhost:11434";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(180);
        });

        // Register the base embedding service
        services.AddScoped<OllamaEmbeddingService>();
        
        // WIRED UP: Register cached embedding service as decorator
        services.AddScoped<IEmbeddingService>(provider =>
        {
            var baseService = provider.GetRequiredService<OllamaEmbeddingService>();
            var cache = provider.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
            return new CachedEmbeddingService(baseService, cache);
        });

        // Register other AI services
        services.AddScoped<IOllamaService, OllamaService>();
        services.AddScoped<IHybridAnalyticsService, HybridAnalyticsService>();
        services.AddScoped<IAnalyticsReportService, AnalyticsReportService>();

        // Background job processor for analytics
        services.AddHostedService<AnalyticsJobProcessor>();

        return services;
    }
}