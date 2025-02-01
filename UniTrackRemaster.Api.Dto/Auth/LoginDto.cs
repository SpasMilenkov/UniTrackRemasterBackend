using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Auth;

public record LoginDto(
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(30, ErrorMessage = "Email cannot be longer than 30 characters.")]
    string Email,

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters long.")]
    string Password
);
