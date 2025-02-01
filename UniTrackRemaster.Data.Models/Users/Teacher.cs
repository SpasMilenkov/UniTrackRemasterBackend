using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.JunctionEntities;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Users;

public class Teacher: BaseEntity
{
    #region Properties
    
        [Length(0, 50)]
        public string? Title { get; set; }
        
        //PrimaryKey
        public required Guid Id { get; set; }
        
    #endregion

    #region NavigationProperties

        //User relation
        public required Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }

        //Events relation
        public IList<Event>? Events { get; set; }

        //School relations
        public Guid InstitutionId { get; set; }
        public Institution? Institution { get; set; }

        //Class grade relation
        public Guid? ClassGradeId { get; set; }
        public Grade? ClassGrade { get; set; }
        
        public IList<HomeRoomTeacher> HomeRoomAssignments { get; set; }
        public IList<Club> SupervisedClubs { get; set; }
        public IList<ElectiveSubject> ElectiveSubjects { get; set; }

    #endregion

}