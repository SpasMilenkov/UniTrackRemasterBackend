using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Events;

public class Event : BaseEntity
{
    #region Properties
    public required Guid Id { get; set; }

    [Length(0, 255)]
    public required string Title { get; set; }

    [Length(0, 255)]
    public required string Topic { get; set; }

    [Length(0, 500)]
    public string? Description { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public EventType Type { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Scheduled;
    public bool IsAllDay { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrencePattern? RecurrencePattern { get; set; }
    public int? MaxParticipants { get; set; }
    public bool RequiresApproval { get; set; }
    public string? MeetingLink { get; set; }
    public string? Notes { get; set; }
    #endregion

    #region NavigationProperties
    public required Guid OrganizerId { get; set; }
    public Organizer? Organizer { get; set; }

    public Guid? InstitutionId { get; set; }
    public Institution? Institution { get; set; }

    public IList<Attender> Attenders { get; set; } = new List<Attender>();
    public IList<Participant> Participants { get; set; } = new List<Participant>();
    public IList<EventNotification> Notifications { get; set; } = new List<EventNotification>();
    #endregion
}