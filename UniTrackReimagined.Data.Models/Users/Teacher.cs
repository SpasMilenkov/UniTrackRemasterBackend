using System.ComponentModel.DataAnnotations;
using UniTrackReimagined.Data.Models.Events;
using UniTrackReimagined.Data.Models.Organizations;

namespace UniTrackReimagined.Data.Models.Users;

public class Teacher
{
    #region Properties
    
        [Length(0, 50)]
        public string? Title { get; set; }
        
        //PrimaryKey
        public required Guid TeacherId { get; set; }
        
    #endregion

    #region NavigationProperties

        //User relation
        public required Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }

        //Subject relation
        public ICollection<Subject>? Subjects { get; set; }

        //Events relation
        public ICollection<Event>? Events { get; set; }

        //School relations
        public ICollection<School>? Schools { get; set; }

        //Class grade relation
        public Guid? ClassGradeId { get; set; }
        public Grade? ClassGrade { get; set; }

    #endregion

}