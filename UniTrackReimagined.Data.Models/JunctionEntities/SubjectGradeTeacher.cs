using UniTrackReimagined.Data.Models.Academical;
using UniTrackReimagined.Data.Models.Users;

namespace UniTrackReimagined.Data.Models.JunctionEntities;

public class SubjectGradeTeacher
{
    #region Properties

        public required Guid Id { get; set; }

    #endregion

    #region NavigationProperties
    
        public required Guid TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        public required Guid GradeId { get; set; }
        public Grade? Grade { get; set; }
        public required Guid SubjectId { get; set; }
        public Subject? Subject { get; set; }
        
    #endregion
}