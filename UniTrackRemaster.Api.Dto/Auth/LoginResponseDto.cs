namespace UniTrackRemaster.Api.Dto.Auth;

public record LoginResponseDto(
 Guid Id,
 string Email,
 string FirstName,
 string LastName,
 string Role,
 bool IsLinked
);
    
