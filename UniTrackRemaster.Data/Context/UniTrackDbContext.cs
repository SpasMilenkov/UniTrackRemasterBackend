using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniTrackRemaster.Data.Models;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Images;
using UniTrackRemaster.Data.Models.JunctionEntities;
using UniTrackRemaster.Data.Models.Location;
using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Data.Models.Users;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Data.Context;

public class UniTrackDbContext(DbContextOptions<UniTrackDbContext> options)
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
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Semester> Semesters { get; set; }
    public DbSet<Department> Departments { get; set; }
    #endregion

    #region Analytics

    public DbSet<AcademicalGroupReport> AcademicalReports { get; set; }
    public DbSet<PersonalReport> PersonalReports { get; set; }
    public DbSet<Recommendation> Recommendations { get; set; }
    public DbSet<SchoolReport> SchoolReports { get; set; }
    public DbSet<UniversityReport> UniversityReports { get; set; }
    

    #endregion

    #region Events

    public DbSet<Attender> Attenders { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Organizer> Organizers { get; set; }
    public DbSet<Participant> Participants { get; set; }

    #endregion

    #region JunctionEntities
    
    public DbSet<SubjectGradeTeacher> SubjectGradeTeacher { get; set; }
    
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

    #endregion
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
        });

        #region Enums

        modelBuilder.Entity<Institution>(builder =>
        {
            builder.Property(e => e.Type)
                .HasConversion<string>();

            builder.Property(e => e.Location)
                .HasConversion<string>();

            builder.Property(e => e.IntegrationStatus)
                .HasConversion<string>();
        
            builder.Property(e => e.Accreditations)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<IList<AccreditationType>>(v, (JsonSerializerOptions)null) ?? new List<AccreditationType>(),
                    new ValueComparer<IList<AccreditationType>>(
                        (c1, c2) => c1 != null && c2 != null && c1.Count == c2.Count && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
                );
        });
        
        modelBuilder.Entity<School>(builder =>
        {
            builder.Property(s => s.ExtracurricularActivities)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<IList<string>>(v, (JsonSerializerOptions)null) ?? new List<string>(),
                    new ValueComparer<IList<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.Count == c2.Count && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
                );
        });

        #endregion
        #region PrimaryKeyConfiguration

            #region Users
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            });

            modelBuilder.Entity<ApplicationRole>(b =>
            {
                b.Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            });
            modelBuilder.Entity<Teacher>()
                .Property(t => t.Id)
                .HasDefaultValueSql("uuid_generate_v4()");

            modelBuilder.Entity<Student>()
                .Property(s => s.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<Parent>()
                .Property(p => p.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<Admin>()
                .Property(a => a.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<SuperAdmin>()
                .Property(s => s.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<AcademicYear>()
                .Property(a => a.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            #endregion

            #region Organizations

            modelBuilder.Entity<School>()
                .Property(s => s.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<University>()
                .Property(u => u.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<Institution>().Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<Application>()
                .Property(a => a.Id)
                .HasDefaultValueSql("uuid_generate_v4()");

            #endregion

            #region Events
            modelBuilder.Entity<Event>()
                .Property(s => s.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<University>()
                .Property(u => u.Id)
                .HasDefaultValueSql("uuid_generate_v4()");

            #endregion

            #region Analytics
            modelBuilder.Entity<School>()
                .Property(s => s.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<University>()
                .Property(u => u.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            

            #endregion

            #region Academical
            modelBuilder.Entity<School>()
                .Property(s => s.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<University>()
                .Property(u => u.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<Image>().Property(s => s.Id).HasDefaultValueSql("uuid_generate_v4()");

            #endregion

            #region JunctionEntities
            modelBuilder.Entity<School>()
                .Property(s => s.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<University>()
                .Property(u => u.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            

            #endregion
            
            #region Location
            
            modelBuilder.Entity<Address>()
                .Property(s => s.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            #endregion
            
        #endregion

        #region CreatedAtUpadtedAt
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Property<DateTime>("CreatedAt").HasDefaultValueSql("NOW()").ValueGeneratedOnAdd();
                modelBuilder.Entity(entityType.ClrType).Property<DateTime>("UpdatedAt").HasDefaultValueSql("NOW()").ValueGeneratedOnAddOrUpdate();
            }
        }
        
        #endregion
        
        #region Relations
        modelBuilder.Entity<Institution>()
            .HasMany(s => s.Images)
            .WithOne(i => i.Institution)
            .HasForeignKey(i => i.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-one relationship with PersonalReport
            entity.HasOne(s => s.PersonalReport)
                .WithOne(pr => pr.Student)
                .HasForeignKey<PersonalReport>(pr => pr.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<HomeRoomTeacher>(entity =>
        {
            entity.HasOne(h => h.Teacher)
                .WithMany(t => t.HomeRoomAssignments)
                .HasForeignKey(h => h.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasOne(g => g.HomeRoomTeacher)
                .WithOne(t => t.Grade)
                .HasForeignKey<HomeRoomTeacher>(t => t.GradeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        // Configure Teacher relationships
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
        // Configure Parent relationships
        modelBuilder.Entity<Parent>(entity =>
        {
            entity.HasOne(p => p.User)
                  .WithOne()
                  .HasForeignKey<Parent>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Children)
                  .WithMany()
                  .UsingEntity(j => j.ToTable("ParentStudents"));
        });

        // Configure SuperAdmin relationships
        modelBuilder.Entity<SuperAdmin>(entity =>
        {
            entity.HasOne(sa => sa.User)
                  .WithOne()
                  .HasForeignKey<SuperAdmin>(sa => sa.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure School relationships
        modelBuilder.Entity<School>(entity =>
        {
            entity.HasOne(s => s.SchoolReport)
                  .WithOne()
                  .HasForeignKey<School>(s => s.SchoolReportId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure University relationships
        modelBuilder.Entity<University>(entity =>
        {
            entity.HasOne(u => u.UniversityReport)
                  .WithOne()
                  .HasForeignKey<University>(u => u.UniversityReportId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.Faculties)
                  .WithOne()
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Grade relationships
        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasMany(g => g.Students)
                  .WithOne(s => s.Grade)
                  .HasForeignKey(s => s.GradeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Mark relationships
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
        });

        // Configure Course relationships
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasOne(c => c.Major)
                .WithMany(m => m.Courses)
                .HasForeignKey(c => c.MajorId)
                .IsRequired(false);

            entity.HasOne(c => c.Subject)
                .WithMany()
                .HasForeignKey(c => c.SubjectId)
                .IsRequired();

            entity.HasOne(c => c.Semester)
                .WithMany(s => s.Courses)
                .HasForeignKey(c => c.SemesterId)
                .IsRequired();
        });
        // Configure Student Course relationships
        modelBuilder.Entity<StudentCourse>(entity =>
        {
            entity.HasKey(sc => new { sc.StudentId, sc.CourseId });

            entity.HasOne(sc => sc.Student)
                  .WithMany()
                  .HasForeignKey(sc => sc.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sc => sc.Course)
                  .WithMany(c => c.StudentCourses)
                  .HasForeignKey(sc => sc.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Faculty relationships
        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasMany(f => f.Majors)
                  .WithOne()
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Major>()
            .HasOne(m => m.Faculty)
            .WithMany(f => f.Majors)
            .HasForeignKey(m => m.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Faculty>()
            .HasOne(f => f.University)
            .WithMany(u => u.Faculties)
            .HasForeignKey(f => f.UniversityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<University>()
            .HasMany(u => u.Faculties)
            .WithOne(f => f.University)
            .HasForeignKey(f => f.UniversityId)
            .OnDelete(DeleteBehavior.Restrict);
        // Configure Image relationships
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasOne(i => i.Institution)
                  .WithMany(ei => ei.Images)
                  .HasForeignKey(i => i.InstitutionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Application relationships
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasOne(a => a.Institution)
                  .WithMany()
                  .HasForeignKey(a => a.InstitutionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<StudentElective>(entity =>
        {
            entity.HasKey(se => se.Id);
    
            entity.HasOne(se => se.Student)
                .WithMany(s => s.Electives) 
                .HasForeignKey(se => se.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        
            entity.HasOne(se => se.ElectiveSubject)
                .WithMany(es => es.StudentElectives)
                .HasForeignKey(se => se.ElectiveSubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(se => se.EnrollmentDate)
                .IsRequired();

            entity.Property(se => se.Status)
                .IsRequired();
        });
        #endregion

        #region Unique
        modelBuilder.Entity<Application>().HasIndex(a => a.Email).IsUnique();
        modelBuilder.Entity<Application>().HasIndex(a => a.Phone).IsUnique();
        #endregion
        base.OnModelCreating(modelBuilder);
        // Ensure the `uuid-ossp` extension is added when creating the database
        modelBuilder.HasPostgresExtension("uuid-ossp");
    }
}