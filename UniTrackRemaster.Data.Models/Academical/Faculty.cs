namespace UniTrackRemaster.Data.Models.Academical;

public class Faculty
{
    #region Properties

    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string ShortDescription { get; set; }
    public required string DetailedDescription { get; set; }
    #endregion
    #region NavigationProperties
    
    public ICollection<Major>? Majors { get; set; }
    #endregion
}