using UniTrackReimagined.Data.Models.JunctionEntities;

namespace UniTrackReimagined.Data.Models.Academical;

public class Subject
{
    #region Properties

    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string ShortDescription { get; set; }
    public string? DetailedDescription { get; set; }
    
    #endregion
    #region NavigationProperties

    public SubjectGradeTeacher? SubjectGradeTeacher { get; set; }
    #endregion
}