using System;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Event;


public class EventFilterParams
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public EventType? Type { get; set; }
    public EventStatus? Status { get; set; }
    public Guid? OrganizerId { get; set; }
    public Guid? InstitutionId { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsPublic { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
