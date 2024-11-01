using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Request;

public record ResetPasswordDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "New password is required.")]
    [DataType(DataType.Password, ErrorMessage = "Invalid password format.")]
    public required string NewPassword { get; set; }

    [Required(ErrorMessage = "Token is required.")]
    public required string Token { get; set; }
}