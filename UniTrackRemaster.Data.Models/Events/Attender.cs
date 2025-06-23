using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Events;

public class Attender : BaseEntity
{
    #region Properties
    public Guid Id { get; set; }
    public AttendanceStatus AttendanceStatus { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? AttendanceNotes { get; set; }
    #endregion

    #region NavigationProperties
    public required Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public IList<Event>? Events { get; set; }
    #endregion
}
