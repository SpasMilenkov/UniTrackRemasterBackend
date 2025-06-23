using System;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Event;

public record EventFilterDto(
    DateTime? StartDate,
    DateTime? EndDate,
    EventType? Type,
    EventStatus? Status,
    Guid? OrganizerId,
    Guid? InstitutionId,
    string? SearchTerm,
    bool? IsPublic,
    int Page = 1,
    int PageSize = 10);

