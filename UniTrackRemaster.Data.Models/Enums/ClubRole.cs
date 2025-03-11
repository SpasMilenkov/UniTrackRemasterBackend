using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum ClubRole
{
    [Display(Name = "Member")]
    Member,
    [Display(Name = "Leader")]
    Leader,
    [Display(Name = "ViceLeader")]
    ViceLeader,
    [Display(Name = "Secretary")]
    Secretary,
    [Display(Name = "Treasurer")]
    Treasurer
}