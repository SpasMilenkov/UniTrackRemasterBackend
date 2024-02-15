using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniTrackReimagined.Data.Models.Organizations;
using UniTrackReimagined.Data.Models.Users;

namespace UniTrackReimagined.Data.Context;

public class UniTrackDbContext : IdentityDbContext<ApplicationUser>
{
    public UniTrackDbContext(DbContextOptions<UniTrackDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Set the context to generate new GUIDS as primary keys for each table
        #region PrimaryKeyConfiguration

            #region Users
    
            modelBuilder.Entity<Teacher>()
                .Property(t => t.TeacherId)
                .HasDefaultValueSql("uuid_generate_v4()");

            modelBuilder.Entity<Student>()
                .Property(s => s.StudentId)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<Parent>()
                .Property(p => p.ParentId)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<Admin>()
                .Property(a => a.AdminId)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<SuperAdmin>()
                .Property(s => s.SuperAdminId)
                .HasDefaultValueSql("uuid_generate_v4()");

            #endregion

            #region Organizations

            modelBuilder.Entity<School>()
                .Property(s => s.SchoolId)
                .HasDefaultValueSql("uuid_generate_v4()");
            
            modelBuilder.Entity<University>()
                .Property(u => u.UniversityId)
                .HasDefaultValueSql("uuid_generate_v4()");

            #endregion

            #region Events

            

            #endregion

            #region Analytics

            

            #endregion

            #region Academical

            

            #endregion

            #region JunctionEntities

            

            #endregion
        #endregion

        #region Relations

        

        #endregion
        
        base.OnModelCreating(modelBuilder);
    }
}