using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum EventType
{
    [Display(Name = "Meeting")]
    Meeting,
    [Display(Name = "Lecture")]
    Lecture,
    [Display(Name = "Conference")]
    Conference,
    [Display(Name = "Workshop")]
    Workshop,
    [Display(Name = "Seminar")]
    Seminar,
    [Display(Name = "Social")]
    Social,
    [Display(Name = "Sports")]
    Sports,
    [Display(Name = "Cultural")]
    Cultural,
    [Display(Name = "Academic")]
    Academic,
    [Display(Name = "Administrative")]
    Administrative,
    [Display(Name = "Other")]
    Other
}