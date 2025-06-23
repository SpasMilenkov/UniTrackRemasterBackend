using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Application;

/// <summary>
/// Request DTO for code verification
/// </summary>
public class VerifyCodeRequestDto
{
    [Required]
    [StringLength(10, MinimumLength = 4, ErrorMessage = "Code must be between 4 and 10 characters")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    public string Email { get; set; } = string.Empty;
}