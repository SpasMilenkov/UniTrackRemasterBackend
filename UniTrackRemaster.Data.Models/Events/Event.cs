using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Events;

public class Event: BaseEntity
{
    #region Properties
        public required Guid Id { get; set; }
        [Length(0, 255)]
        public required string Title { get; set; }
        [Length(0, 255)]
        public required string Topic { get; set; }
        [Length(0, 255)]
        public string? Description { get; set; }

    #endregion
    #region NavigationProperties

        public required Guid OrganizerId { get; set; }
        public Organizer? Organizer { get; set; }
        public required IList<Attender> Attenders { get; set; }
        public required IList<Participant> Participants { get; set; }


    #endregion
}