namespace UniTrackRemaster.Data.Models.Images;

public class SchoolImage: BaseEntity
{
    #region Properties

    public string Id { get; set; }
    public string Url { get; set; }
    public Guid SchoolId { get; set; }
    #endregion
}