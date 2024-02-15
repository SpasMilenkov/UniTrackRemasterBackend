using System.ComponentModel.DataAnnotations;

namespace UniTrackReimagined.Data.Models.Events;

public class Event
{
    #region Properties
        
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
        public required ICollection<Attender> Attenders { get; set; }
        public required ICollection<Participant> Participants { get; set; }


    #endregion
}