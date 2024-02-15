using System.ComponentModel.DataAnnotations;
using UniTrackReimagined.Data.Models.Users;

namespace UniTrackReimagined.Data.Models.Organizations;

public class School
{
    #region Properties
    
        public required Guid SchoolId { get; set; }
        [Length(0, 100)]
        public required string Name { get; set; }
        [Length(0, 255)]
        public string? Description { get; set; }

    #endregion

    #region NavigationProperties

        public ICollection<Student>? Students { get; set; }
        public ICollection<Teacher>? Teachers { get; set; }
        public ICollection<Grade>? Grades { get; set; }
        public ICollection<Subject>? Subjects { get; set; }
    
    #endregion
}