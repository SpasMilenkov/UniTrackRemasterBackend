using System;

namespace UniTrackRemaster.Data.Models.Vector;

public record InstitutionRanking(
    Guid InstitutionId,
    string InstitutionName,
    decimal Score,
    int Rank,
    decimal ChangeFromPrevious,
    string? Category = null
);