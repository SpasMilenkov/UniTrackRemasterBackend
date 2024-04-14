using System.ComponentModel.DataAnnotations;

namespace UniTrackBackend.Api.Dto.Request;

public class RegisterDto
{
    [Required(ErrorMessage = "First name is required.")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public required string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters long.")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Confirm password is required.")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public required string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Organization type is required.")]
    public required string OrgType { get; set; }

    [Required(ErrorMessage = "Organization ID is required.")]
    public required Guid OrgId { get; set; }

    [Required(ErrorMessage = "Organization role is required.")]
    public required string OrgRole { get; set; }
}