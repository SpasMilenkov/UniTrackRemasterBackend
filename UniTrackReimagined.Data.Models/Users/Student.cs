using UniTrackReimagined.Data.Models.Analytics;
using UniTrackReimagined.Data.Models.Events;
using UniTrackReimagined.Data.Models.Organizations;

namespace UniTrackReimagined.Data.Models.Users;

public class Student
{
    #region Properties
    
        //PrimaryKey
        public required Guid StudentId { get; set; }

        public required bool IsSchoolStudent { get; set; }
        
        public required bool IsUniversityStudent { get; set; }
    
    #endregion

    #region NavigationProperties

        //User relation
        public required Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }
        

        //Events relation
        public ICollection<Event>? Events { get; set; }
        
        //School relation
        public Guid? SchoolId { get; set; }
        public School? School { get; set; }

        //University relation
        public Guid UniversityId { get; set; }
        public University? University { get; set; }
        
        //Grade relation
        public Guid GradeId { get; set; }
        public Grade? Grade { get; set; }
        
        //Mark relation
        public ICollection<Mark> Marks { get; set; }
        
        //Report relation
        public Guid? PersonalReportId { get; set; }
        public PersonalReport? PersonalReport { get; set; }
    
    #endregion
}