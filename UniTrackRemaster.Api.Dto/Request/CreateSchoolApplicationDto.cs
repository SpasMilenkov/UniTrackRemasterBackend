using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Api.Dto.Request;

public record CreateSchoolApplicationDto(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    AddressRequestDto Address,
    string SchoolName)
{
    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name should be between 2 and 50 characters.")]
    public required string FirstName { get; init; } = FirstName;

    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name should be between 2 and 50 characters.")]
    public required string LastName { get; init; } = LastName;

    [Required]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public required string Email { get; init; } = Email;

    [Required]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public required string Phone { get; init; } = Phone;

    [Required]
    public required AddressRequestDto Address { get; init; } = Address;

    [Required]
    public required string SchoolName { get; init; } = SchoolName;

    public static Application ToEntity(CreateSchoolApplicationDto application, Guid schoolId) => new Application
    {
        Id = Guid.NewGuid(),
        FirstName = application.FirstName,
        LastName = application.LastName,
        Email = application.Email,
        Phone = application.Phone,
        SchoolId = schoolId,
        Code = GenerateCode(),
    }; 
    static string GenerateCode(int length = 8)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var code = new StringBuilder(length);
        using (var rng = RandomNumberGenerator.Create())
        {
            var randomBytes = new byte[4];
            for (var i = 0; i < length; i++)
            {
                rng.GetBytes(randomBytes);
                var randomIndex = BitConverter.ToInt32(randomBytes, 0) % chars.Length;
                randomIndex = Math.Abs(randomIndex);
                code.Append(chars[randomIndex]);
            }
        }
        return code.ToString();
    }
}