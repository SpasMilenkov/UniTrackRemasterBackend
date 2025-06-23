using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.User;

public record UpdateUserProfileDto(
    [StringLength(100, MinimumLength = 2, ErrorMessage = "First name should be between 2 and 100 characters.")]
    string FirstName,
    
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name should be between 2 and 100 characters.")]
    string LastName,
    
    [StringLength(100, MinimumLength = 0, ErrorMessage = "Display name should be less than 100 characters.")]
    string? DisplayName,
    
    [EmailAddress]
    string Email,
    
    [Phone]
    string? Phone
);