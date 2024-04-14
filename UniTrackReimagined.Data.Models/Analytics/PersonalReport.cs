using UniTrackReimagined.Data.Models.Users;

namespace UniTrackReimagined.Data.Models.Analytics;

public class PersonalReport
{
    #region Properties

    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required DateTime From { get; set; }
    public required DateTime To { get; set; }
    public required string ShortDescription { get; set; }
    public required string DetailedDescription { get; set; }
    public required decimal NumericalRating { get; set; }

    #endregion
    #region NavigationProperties

    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    #endregion
}