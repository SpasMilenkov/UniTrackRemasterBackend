using System;
using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum EventStatus
{
    [Display(Name = "Draft")]
    Draft,
    [Display(Name = "Scheduled")]
    Scheduled,
    [Display(Name = "InProgress")]
    InProgress,
    [Display(Name = "Completed")]
    Completed,
    [Display(Name = "Cancelled")]
    Cancelled,
    [Display(Name = "Postponed")]
    Postponed
}