using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniTrackReimagined.Data.Models.Academical;
using UniTrackReimagined.Data.Models.Analytics;
using UniTrackReimagined.Data.Models.Events;
using UniTrackReimagined.Data.Models.JunctionEntities;
using UniTrackReimagined.Data.Models.Organizations;
using UniTrackReimagined.Data.Models.Users;

namespace UniTrackReimagined.Data.Context;

public class UniTrackDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public UniTrackDbContext(DbContextOptions<UniTrackDbContext> options) : base(options)
    {
    }


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

    #region Organizations

    public DbSet<School> Schools { get; set; }
    public DbSet<University> Universities { get; set; }

    #endregion


    #endregion
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Set the context to generate new GUIDS as primary keys for each table
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

            #endregion

            #region Organizations

            modelBuilder.Entity<School>()
                .Property(s => s.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<University>()
                .Property(u => u.Id)
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
            

            #endregion

            #region JunctionEntities
            modelBuilder.Entity<School>()
                .Property(s => s.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<University>()
                .Property(u => u.Id)
                .HasDefaultValueSql("uuid_generate_v4()");
            

            #endregion
        #endregion

        #region Relations
        

        #endregion
        
        base.OnModelCreating(modelBuilder);
    }
}