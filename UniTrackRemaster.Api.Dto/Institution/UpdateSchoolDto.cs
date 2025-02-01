namespace UniTrackRemaster.Api.Dto.Institution;

public record UpdateSchoolDto(
    string? Name,
    Guid SchoolId,
    string? Description,
    IList<Guid>? StudentIds,
    IList<Guid>? TeacherIds,
    IList<Guid>? MajorIds);