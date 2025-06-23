using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Data.Models;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Images;
using UniTrackRemaster.Data.Models.Location;
using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Data.Models.Users;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using UniTrackRemaster.Data.Configuration;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Chat;

namespace UniTrackRemaster.Data.Context;

public class UniTrackDbContext(DbContextOptions<UniTrackDbContext> options, IConfiguration configuration)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    #region DbSets

    #region Users

    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<SuperAdmin> SuperAdmins { get; set; }
    public DbSet<Parent> Parents { get; set; }

    #endregion

    #region Academical

    public DbSet<Faculty> Faculties { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<Major> Majors { get; set; }
    public DbSet<Mark> Marks { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<AcademicYear> AcademicYears { get; set; }
    public DbSet<Absence> Absences { get; set; }
    public DbSet<Semester> Semesters { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<StudentElective> StudentElectives { get; set; }
    public DbSet<GradingSystem> GradingSystems { get; set; }
    public DbSet<GradeScale> GradeScales { get; set; }
    public DbSet<ElectiveSubject> ElectiveSubjects { get; set; }
    #endregion

    #region Analytics

    public DbSet<Recommendation> Recommendations { get; set; }

    public DbSet<InstitutionAnalyticsReport> InstitutionAnalyticsReports { get; set; }
    public DbSet<MarketAnalyticsReport> MarketAnalyticsReports { get; set; }
    public DbSet<ReportVisibilitySettings> ReportVisibilitySettings { get; set; }
    public DbSet<ReportGenerationJob> ReportGenerationJobs { get; set; }
    #endregion

    #region Events

    public DbSet<Attender> Attenders { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Organizer> Organizers { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<EventNotification> EventNotifications { get; set; }

    #endregion

    #region JunctionEntities


    #endregion

    #region Locations
    public DbSet<Address> SchoolAddress { get; set; }
    #endregion

    #region Organizations

    public DbSet<Institution> Institutions { get; set; }
    public DbSet<School> Schools { get; set; }
    public DbSet<University> Universities { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Image> Images { get; set; }
    #endregion

    #region Chat
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<MessageReaction> MessageReactions { get; set; }
    public DbSet<MessageEditHistory> MessageEditHistories { get; set; }
    #endregion 

    #endregion

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Ensure PostgreSQL UUID extension
    modelBuilder.HasPostgresExtension("uuid-ossp");

    #region Primary Key Auto-Generation
    // Configure UUID auto-generation for all entities
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey != null && primaryKey.Properties.Count == 1)
        {
            var property = primaryKey.Properties[0];
            if (property.ClrType == typeof(Guid))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasDefaultValueSql("uuid_generate_v4()");
            }
        }
    }
    #endregion

    #region BaseEntity CreatedAt/UpdatedAt Auto-Configuration
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
        {
            modelBuilder.Entity(entityType.ClrType)
                .Property<DateTime>("CreatedAt")
                .HasDefaultValueSql("NOW()")
                .ValueGeneratedOnAdd();
                
            modelBuilder.Entity(entityType.ClrType)
                .Property<DateTime>("UpdatedAt")
                .HasDefaultValueSql("NOW()")
                .ValueGeneratedOnAddOrUpdate();
        }
    }
    #endregion

    #region String Length Configuration
    // Configure common patterns with loops for consistency
    var entityTypes = modelBuilder.Model.GetEntityTypes();

    foreach (var entityType in entityTypes)
    {
        foreach (var property in entityType.GetProperties())
        {
            if (property.ClrType == typeof(string))
            {
                var propertyName = property.Name;
                
                // Apply standard lengths based on property name patterns
                switch (propertyName.ToLower())
                {
                    case "firstname":
                    case "lastname":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.PersonName);
                        break;
                        
                    case "email":
                    case "contactemail":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.Email);
                        break;
                        
                    case "phone":
                    case "contactphone":
                    case "emergencycontact":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.Phone);
                        break;
                        
                    case "code":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.Code);
                        break;
                        
                    case "name":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.EntityName);
                        break;
                        
                    case "title":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.Title);
                        break;
                        
                    case "shortdescription":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.ShortDescription);
                        break;
                        
                    case "detaileddescription":
                    case "description":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.LongDescription);
                        break;
                        
                    case "notes":
                    case "responsenote":
                    case "attendancenotes":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.Notes);
                        break;
                        
                    case "reason":
                    case "editreason":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.Reason);
                        break;
                        
                    case "website":
                    case "logourl":
                    case "avatarurl":
                    case "meetinglink":
                    case "attachmenturl":
                    case "sourcelink":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.Url);
                        break;
                        
                    case "location":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.Location);
                        break;
                        
                    case "content":
                    case "message":
                    case "previouscontent":
                    case "newcontent":
                    case "originalcontent":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.MessageContent);
                        break;
                        
                    case "country":
                    case "settlement":
                    case "street":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.Address);
                        break;
                        
                    case "postalcode":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(20);
                        break;
                        
                    case "topic":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.EntityName);
                        break;
                        
                    case "position":
                    case "department":
                    case "role":
                    case "occupation":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.EntityName);
                        break;
                        
                    case "motto":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(300);
                        break;
                    case "refreshtoken":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(2000); // JWT tokens can be long
                        break;

                    case "passwordhash":
                    case "securitystamp": 
                    case "concurrencystamp":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasColumnType("TEXT"); // These can be very long
                        break;
                    case "admissionrequirements":
                    case "careeropportunities":
                        modelBuilder.Entity(entityType.ClrType)
                            .Property(propertyName)
                            .HasMaxLength(DatabaseLengths.LongDescription);
                        break;
                }
            }
        }
    }
    #endregion

    #region Enum Conversions
    // Institution enums
    modelBuilder.Entity<Institution>(builder =>
    {
        builder.Property(e => e.Type).HasConversion<string>();
        builder.Property(e => e.Location).HasConversion<string>();
        builder.Property(e => e.IntegrationStatus).HasConversion<string>();
    
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };
    
        builder.Property(e => e.Accreditations)
            .HasConversion(
                v => JsonSerializer.Serialize(v, options),
                v => JsonSerializer.Deserialize<IList<AccreditationType>>(v, options) ?? new List<AccreditationType>(),
                new ValueComparer<IList<AccreditationType>>(
                    (c1, c2) => c1 != null && c2 != null && c1.Count == c2.Count && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                )
            );
    });

    // User profile enums
    modelBuilder.Entity<ApplicationUser>(builder =>
    {
        builder.Property(u => u.ProfileVisibility).HasConversion<string>();
    });

    modelBuilder.Entity<Student>(builder =>
    {
        builder.Property(s => s.Status).HasConversion<string>();
    });

    modelBuilder.Entity<Teacher>(builder =>
    {
        builder.Property(t => t.Status).HasConversion<string>();
    });

    modelBuilder.Entity<Admin>(builder =>
    {
        builder.Property(a => a.Status).HasConversion<string>();
        builder.Property(a => a.Role).HasConversion<string>();
    });

    modelBuilder.Entity<Parent>(builder =>
    {
        builder.Property(p => p.Status).HasConversion<string>();
    });

    // Academic structure enums
    modelBuilder.Entity<Faculty>(builder =>
    {
        builder.Property(f => f.Status).HasConversion<string>();
    });

    modelBuilder.Entity<Department>(builder =>
    {
        builder.Property(d => d.Type).HasConversion<string>();
        builder.Property(d => d.Status).HasConversion<string>();
    });

    modelBuilder.Entity<Subject>(builder =>
    {
        builder.Property(s => s.SubjectType).HasConversion<string>();
        builder.Property(s => s.AcademicLevel).HasConversion<string>();
        builder.Property(s => s.ElectiveType).HasConversion<string>();
    });

    modelBuilder.Entity<ElectiveSubject>(builder =>
    {
        builder.Property(e => e.Type).HasConversion<string>();
    });

    modelBuilder.Entity<StudentElective>(builder =>
    {
        builder.Property(se => se.Status).HasConversion<string>();
    });

    // Academic calendar enums
    modelBuilder.Entity<Semester>(builder =>
    {
        builder.Property(s => s.Type).HasConversion<string>();
        builder.Property(s => s.Status).HasConversion<string>();
    });

    // Assessment enums
    modelBuilder.Entity<Mark>(builder =>
    {
        builder.Property(m => m.Type).HasConversion<string>();
    });

    modelBuilder.Entity<Absence>(builder =>
    {
        modelBuilder.Entity<Absence>(builder =>
        {
            builder.Property(a => a.Status).HasConversion<string>();  // Now AbsenceStatus
        });
    });

    modelBuilder.Entity<GradingSystem>(builder =>
    {
        builder.Property(gs => gs.Type).HasConversion<string>();
    });

    // Event enums
    modelBuilder.Entity<Event>(builder =>
    {
        builder.Property(e => e.Type).HasConversion<string>();
        builder.Property(e => e.Status).HasConversion<string>();
        builder.Property(e => e.RecurrencePattern).HasConversion<string>();
    });

    modelBuilder.Entity<Participant>(builder =>
    {
        builder.Property(p => p.Status).HasConversion<string>();
    });

    modelBuilder.Entity<Attender>(builder =>
    {
        builder.Property(a => a.AttendanceStatus).HasConversion<string>();
    });

    modelBuilder.Entity<EventNotification>(builder =>
    {
        builder.Property(n => n.Type).HasConversion<string>();
    });

    // Chat enums
    modelBuilder.Entity<ChatMessage>(builder =>
    {
        builder.Property(m => m.MessageType).HasConversion<string>();
        builder.Property(m => m.Status).HasConversion<string>();
    });

    modelBuilder.Entity<MessageReaction>(builder =>
    {
        builder.Property(r => r.ReactionType).HasConversion<string>();
    });

    // Analytics enums
    modelBuilder.Entity<InstitutionAnalyticsReport>(builder =>
    {
        builder.Property(r => r.PeriodType).HasConversion<string>();
        builder.Property(r => r.OverallPerformanceCategory).HasConversion<string>();
    });

    modelBuilder.Entity<MarketAnalyticsReport>(builder =>
    {
        builder.Property(r => r.ReportType).HasConversion<string>();
        builder.Property(r => r.PeriodType).HasConversion<string>();
    });

    modelBuilder.Entity<ReportGenerationJob>(builder =>
    {
        builder.Property(j => j.PeriodType).HasConversion<string>();
        builder.Property(j => j.ReportType).HasConversion<string>();
        builder.Property(j => j.Status).HasConversion<string>();
    });

    // Application enums
    modelBuilder.Entity<Application>(builder =>
    {
        builder.Property(a => a.Status).HasConversion<string>();
    });
    #endregion

    #region Specific Length Overrides
    // Override specific cases that need different lengths than the convention
    modelBuilder.Entity<Event>(entity =>
    {
        // Events need shorter titles for UI purposes
        entity.Property(e => e.Title).HasMaxLength(150);
        entity.Property(e => e.Topic).HasMaxLength(150);
        entity.Property(e => e.Description).HasMaxLength(DatabaseLengths.ShortDescription);
    });

    modelBuilder.Entity<Institution>(entity =>
    {
        // Institution names can be longer
        entity.Property(i => i.Name).HasMaxLength(150);
    });

    modelBuilder.Entity<Subject>(entity =>
    {
        // Academic content can be longer
        entity.Property(s => s.DetailedDescription).HasMaxLength(3000);
    });

    modelBuilder.Entity<Teacher>(entity =>
    {
        // Academic titles can vary
        entity.Property(t => t.Title).HasMaxLength(100); // "Professor", "Dr.", "PhD", etc.
    });

    // Fields that should remain as TEXT (unlimited)
    modelBuilder.Entity<InstitutionAnalyticsReport>(entity =>
    {
        // Analytics content can be very long
        entity.Property(r => r.ExecutiveSummary).HasColumnType("TEXT");
        entity.Property(r => r.AIGeneratedInsights).HasColumnType("TEXT");
    });

    modelBuilder.Entity<MarketAnalyticsReport>(entity =>
    {
        entity.Property(r => r.MarketInsights).HasColumnType("TEXT");
        entity.Property(r => r.FutureProjections).HasColumnType("TEXT");
    });

    modelBuilder.Entity<ReportGenerationJob>(entity =>
    {
        entity.Property(j => j.ProcessingLogs).HasColumnType("TEXT");
        entity.Property(j => j.JobParameters).HasColumnType("TEXT");
        entity.Property(j => j.ErrorMessage).HasColumnType("TEXT");
    });
    #endregion

    #region Institution Configuration
    modelBuilder.Entity<Institution>(entity =>
    {
        entity.ToTable("Institutions");
    });

    modelBuilder.Entity<School>(entity =>
    {
        entity.ToTable("Schools");
        entity.HasKey(e => e.Id);
        entity.HasOne(s => s.Institution)
            .WithOne()
            .HasForeignKey<School>(s => s.InstitutionId);
    });

    modelBuilder.Entity<University>(entity =>
    {
        entity.ToTable("Universities");
        entity.HasKey(e => e.Id);
        entity.HasOne(u => u.Institution)
            .WithOne()
            .HasForeignKey<University>(u => u.InstitutionId);
        entity.HasMany(u => u.Faculties)
            .WithOne(f => f.University)
            .HasForeignKey(f => f.UniversityId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<Image>(entity =>
    {
        entity.HasOne(i => i.Institution)
            .WithMany(ei => ei.Images)
            .HasForeignKey(i => i.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<Application>(entity =>
    {
        entity.HasOne(a => a.Institution)
            .WithMany()
            .HasForeignKey(a => a.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);
    });
    #endregion

    #region User Relationships
    modelBuilder.Entity<Student>(entity =>
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students", t => t.HasCheckConstraint("CK_Student_BasicLogic", 
                $"(\"{nameof(Student.IsSchoolStudent)}\" = true AND \"{nameof(Student.UniversityId)}\" IS NULL AND \"{nameof(Student.MajorId)}\" IS NULL) OR " +
                $"(\"{nameof(Student.IsSchoolStudent)}\" = false AND \"{nameof(Student.SchoolId)}\" IS NULL)"));
        });

        entity.HasOne(s => s.User)
            .WithOne()
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(s => s.Grade)
            .WithMany(g => g.Students)
            .HasForeignKey(s => s.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(s => s.Major)
            .WithMany(m => m.Students)
            .HasForeignKey(s => s.MajorId)
            .OnDelete(DeleteBehavior.SetNull);
        entity.Property(s => s.Status)
            .HasDefaultValue(ProfileStatus.Pending);
    });

    modelBuilder.Entity<Teacher>(entity =>
    {
        entity.HasOne(t => t.User)
            .WithOne()
            .HasForeignKey<Teacher>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(t => t.ClassGrade)
            .WithMany()
            .HasForeignKey(t => t.ClassGradeId)
            .OnDelete(DeleteBehavior.SetNull);
    });

    modelBuilder.Entity<Parent>(entity =>
    {
        entity.HasKey(p => p.Id);
        entity.HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<Parent>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(p => p.Children)
            .WithMany(s => s.Parents)
            .UsingEntity("ParentStudent",
                l => l.HasOne(typeof(Student)).WithMany().HasForeignKey("StudentId"),
                r => r.HasOne(typeof(Parent)).WithMany().HasForeignKey("ParentId"),
                j => j.HasKey("ParentId", "StudentId"));
        entity.Property(p => p.Status)
            .HasDefaultValue(ProfileStatus.Pending);
    });

    modelBuilder.Entity<Admin>(entity =>
    {
        entity.HasOne(a => a.User)
            .WithOne()
            .HasForeignKey<Admin>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(a => a.Institution)
            .WithMany(i => i.Admins)
            .HasForeignKey(a => a.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<SuperAdmin>(entity =>
    {
        entity.HasOne(sa => sa.User)
            .WithOne()
            .HasForeignKey<SuperAdmin>(sa => sa.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    });
    #endregion

    #region Academic Structure
    modelBuilder.Entity<Faculty>(entity =>
    {
        entity.HasMany(f => f.Majors)
            .WithOne(m => m.Faculty)
            .HasForeignKey(m => m.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(f => f.Departments)
            .WithOne(d => d.Faculty)
            .HasForeignKey(d => d.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<Grade>(entity =>
    {
        entity.HasOne(g => g.HomeRoomTeacher)
            .WithOne(h => h.Grade)
            .HasForeignKey<HomeRoomTeacher>(h => h.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(g => g.Students)
            .WithOne(s => s.Grade)
            .HasForeignKey(s => s.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<HomeRoomTeacher>(entity =>
    {
        entity.HasOne(h => h.Teacher)
            .WithMany(t => t.HomeRoomAssignments)
            .HasForeignKey(h => h.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<Subject>(entity =>
    {
        entity.HasOne(s => s.PrimaryTeacher)
            .WithMany()
            .HasForeignKey(s => s.PrimaryTeacherId)
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasMany(s => s.Teachers)
            .WithMany(t => t.Subjects)
            .UsingEntity(j => j.ToTable("SubjectTeachers"));
        entity.HasMany(s => s.Grades)
            .WithMany(g => g.Subjects)
            .UsingEntity(j => j.ToTable("SubjectGrades"));
    });

    modelBuilder.Entity<Teacher>(entity =>
    {
        entity.HasMany(t => t.Grades)
            .WithMany(g => g.Teachers)
            .UsingEntity(j => j.ToTable("TeacherGrades"));
    });

    modelBuilder.Entity<StudentElective>(entity =>
    {
        entity.HasKey(se => se.Id);
        entity.HasOne(se => se.Student)
            .WithMany(s => s.Electives)
            .HasForeignKey(se => se.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.Property(se => se.EnrollmentDate).IsRequired();
        entity.Property(se => se.Status).IsRequired();
    });
    #endregion

    #region Assessment & Grading
    modelBuilder.Entity<Mark>(entity =>
    {
        entity.HasOne(m => m.Subject)
            .WithMany()
            .HasForeignKey(m => m.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(m => m.Teacher)
            .WithMany()
            .HasForeignKey(m => m.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(m => m.Student)
            .WithMany(s => s.Marks)
            .HasForeignKey(m => m.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(m => m.Semester)
            .WithMany()
            .HasForeignKey(m => m.SemesterId)
            .OnDelete(DeleteBehavior.SetNull);
    });

    modelBuilder.Entity<Absence>(entity =>
    {
        entity.HasOne(a => a.Student)
            .WithMany(s => s.AttendanceRecords)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(a => a.Subject)
            .WithMany()
            .HasForeignKey(a => a.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(a => a.Teacher)
            .WithMany()
            .HasForeignKey(a => a.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(a => a.Semester)
            .WithMany()
            .HasForeignKey(a => a.SemesterId)
            .OnDelete(DeleteBehavior.SetNull);
    });

    modelBuilder.Entity<GradingSystem>(entity =>
    {
        entity.HasOne(gs => gs.Institution)
            .WithMany()
            .HasForeignKey(gs => gs.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(gs => gs.GradeScales)
            .WithOne(gsc => gsc.GradingSystem)
            .HasForeignKey(gsc => gsc.GradingSystemId)
            .OnDelete(DeleteBehavior.Cascade);
    });
    #endregion

    #region Events Configuration
    modelBuilder.Entity<Event>(entity =>
    {
        entity.ToTable("Events");
        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.Organizer)
            .WithMany(o => o.OrganizedEvents)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(e => e.Institution)
            .WithMany(i => i.Events)
            .HasForeignKey(e => e.InstitutionId)
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasMany(e => e.Participants)
            .WithOne(p => p.Event)
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(e => e.Notifications)
            .WithOne(n => n.Event)
            .HasForeignKey(n => n.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<Participant>(entity =>
    {
        entity.ToTable("Participants");
        entity.HasKey(p => p.Id);

        entity.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(p => p.Event)
            .WithMany(e => e.Participants)
            .HasForeignKey(p => p.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasIndex(p => new { p.EventId, p.UserId })
            .IsUnique()
            .HasDatabaseName("IX_Participants_EventId_UserId");
    });

    modelBuilder.Entity<Attender>(entity =>
    {
        entity.ToTable("Attenders");
        entity.HasKey(a => a.Id);

        entity.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasMany(a => a.Events)
            .WithMany(e => e.Attenders)
            .UsingEntity<Dictionary<string, object>>(
                "EventAttenders",
                j => j.HasOne<Event>().WithMany().HasForeignKey("EventId"),
                j => j.HasOne<Attender>().WithMany().HasForeignKey("AttenderId"));
    });

    modelBuilder.Entity<Organizer>(entity =>
    {
        entity.ToTable("Organizers");
        entity.HasKey(o => o.Id);

        entity.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(o => o.Institution)
            .WithMany()
            .HasForeignKey(o => o.InstitutionId)
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasIndex(o => o.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Organizers_UserId");
    });

    modelBuilder.Entity<EventNotification>(entity =>
    {
        entity.ToTable("EventNotifications");
        entity.HasKey(n => n.Id);

        entity.HasOne(n => n.Event)
            .WithMany(e => e.Notifications)
            .HasForeignKey(n => n.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    });
    #endregion

    #region Chat Configuration
    modelBuilder.Entity<ChatMessage>(entity =>
    {
        entity.ToTable("ChatMessages");
        entity.HasKey(e => e.Id);

        entity.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(m => m.Recipient)
            .WithMany()
            .HasForeignKey(m => m.RecipientId)
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasOne(m => m.ParentMessage)
            .WithMany(m => m.Replies)
            .HasForeignKey(m => m.ParentMessageId)
            .OnDelete(DeleteBehavior.SetNull);
        entity.HasOne(m => m.DeletedByUser)
            .WithMany()
            .HasForeignKey(m => m.DeletedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for performance
        entity.HasIndex(e => e.SenderId);
        entity.HasIndex(e => e.RecipientId);
        entity.HasIndex(e => e.GroupId);
        entity.HasIndex(e => e.SentAt);
        entity.HasIndex(e => e.ParentMessageId);
        entity.HasIndex(e => new { e.IsDeleted, e.SentAt });
    });

    modelBuilder.Entity<MessageReaction>(entity =>
    {
        entity.ToTable("MessageReactions");
        entity.HasKey(e => e.Id);

        entity.HasOne(r => r.Message)
            .WithMany(m => m.Reactions)
            .HasForeignKey(r => r.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: one reaction type per user per message
        entity.HasIndex(e => new { e.MessageId, e.UserId, e.ReactionType }).IsUnique();
        entity.HasIndex(e => e.MessageId);
        entity.HasIndex(e => e.UserId);
    });

    modelBuilder.Entity<MessageEditHistory>(entity =>
    {
        entity.ToTable("MessageEditHistory");
        entity.HasKey(e => e.Id);

        entity.HasOne(h => h.Message)
            .WithMany(m => m.EditHistory)
            .HasForeignKey(h => h.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => e.MessageId);
        entity.HasIndex(e => e.EditedAt);
    });
    #endregion

    #region Analytics Configuration
    modelBuilder.Entity<InstitutionAnalyticsReport>(entity =>
    {
        entity.ToTable("InstitutionAnalyticsReports");
        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.Institution)
            .WithMany()
            .HasForeignKey(e => e.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);

        // JSON fields with defaults
        entity.Property(e => e.SubjectPerformanceScores).HasDefaultValue("{}");
        entity.Property(e => e.DepartmentRankings).HasDefaultValue("{}");
        entity.Property(e => e.PopularMajors).HasDefaultValue("{}");
        entity.Property(e => e.MajorGrowthRates).HasDefaultValue("{}");
        entity.Property(e => e.NationalRankings).HasDefaultValue("{}");
        entity.Property(e => e.RegionalRankings).HasDefaultValue("{}");
        entity.Property(e => e.TopAchievements).HasDefaultValue("[]");
        entity.Property(e => e.FastestGrowingAreas).HasDefaultValue("[]");
        entity.Property(e => e.StrongestSubjects).HasDefaultValue("[]");

        // Performance indexes
        entity.HasIndex(e => e.InstitutionId);
        entity.HasIndex(e => e.PeriodType);
        entity.HasIndex(e => new { e.InstitutionId, e.PeriodType, e.From });
        entity.HasIndex(e => e.OverallAcademicScore);
    });

    modelBuilder.Entity<MarketAnalyticsReport>(entity =>
    {
        entity.ToTable("MarketAnalyticsReports");
        entity.HasKey(e => e.Id);

        // JSON fields with defaults
        entity.Property(e => e.EnrollmentLeaders).HasDefaultValue("[]");
        entity.Property(e => e.AcademicLeaders).HasDefaultValue("[]");
        entity.Property(e => e.FastestGrowing).HasDefaultValue("[]");
        entity.Property(e => e.SubjectLeaders).HasDefaultValue("{}");
        entity.Property(e => e.TrendingMajors).HasDefaultValue("[]");
        entity.Property(e => e.DecliningMajors).HasDefaultValue("[]");
        entity.Property(e => e.RegionalBreakdown).HasDefaultValue("{}");

        entity.HasIndex(e => e.ReportType);
        entity.HasIndex(e => e.PeriodType);
        entity.HasIndex(e => new { e.ReportType, e.PeriodType, e.ReportPeriod });
    });

    modelBuilder.Entity<ReportVisibilitySettings>(entity =>
    {
        entity.ToTable("ReportVisibilitySettings");
        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.Institution)
            .WithOne()
            .HasForeignKey<ReportVisibilitySettings>(e => e.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.Property(e => e.HighlightedAchievements).HasDefaultValue("[]");
        entity.Property(e => e.CustomMetrics).HasDefaultValue("{}");

        entity.HasIndex(e => e.InstitutionId).IsUnique();
    });

    modelBuilder.Entity<ReportGenerationJob>(entity =>
    {
        entity.ToTable("ReportGenerationJobs");
        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.Institution)
            .WithMany()
            .HasForeignKey(e => e.InstitutionId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(e => e.Status);
        entity.HasIndex(e => e.ScheduledFor);
        entity.HasIndex(e => new { e.PeriodType, e.Status });
    });

    modelBuilder.Entity<Recommendation>(entity =>
    {
        entity.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    });
    #endregion

    #region Unique Constraints
    modelBuilder.Entity<Application>().HasIndex(a => a.Email).IsUnique();
    modelBuilder.Entity<Application>().HasIndex(a => a.Phone).IsUnique();
    #endregion

    base.OnModelCreating(modelBuilder);
}

}