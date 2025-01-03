using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Request;

public record LoginDto(string Email, string Password)
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(30, ErrorMessage = "Email cannot be longer than 100 characters.")]
    public required string Email { get; init; } = Email;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(30, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters long.")]
    public required string Password { get; init; } = Password;
}