using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Api.Dto.Request;

public record CreateInstitutionApplicationDto(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string InstitutionName,
    InstitutionType InstitutionType,
    AddressDto Address)
{
    static string GenerateApplicationCode(int length = 8)
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
    public static Application ToEntity(CreateInstitutionApplicationDto dto, Guid institutionId) => new()
    {
        Id = Guid.NewGuid(),
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        Phone = dto.Phone,
        InstitutionId = institutionId,
        Status = ApplicationStatus.Pending,
        Code = GenerateApplicationCode()
    };
}
