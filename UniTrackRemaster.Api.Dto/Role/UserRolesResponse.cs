namespace UniTrackRemaster.Api.Dto.Role;

public record UserRolesResponse(
    Guid InstitutionId,
    List<string> Roles);