using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Auth;

public record ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; init; } = string.Empty;
    
    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; init; } = string.Empty;

    public ChangePasswordDto(string currentPassword, string newPassword, string confirmPassword)
    {
        CurrentPassword = currentPassword;
        NewPassword = newPassword;
        ConfirmPassword = confirmPassword;
    }
}