using System;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Event;

public record EventStatsDto(
    int TotalEvents,
    int UpcomingEvents,
    int OngoingEvents,
    int CompletedEvents,
    int CancelledEvents,
    Dictionary<EventType, int> EventsByType,
    Dictionary<string, int> EventsByMonth);
