using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Images;

public class Image: BaseEntity
{
    #region Properties

    public Guid Id { get; set; }
    public string Url { get; set; }
    public Guid InstitutionId { get; set; }
    public Institution Institution { get; set; }
    #endregion
}