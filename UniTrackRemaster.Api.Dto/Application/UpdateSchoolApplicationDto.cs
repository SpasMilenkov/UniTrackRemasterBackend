using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Api.Dto.Request;

public record UpdateInstitutionApplicationDto(
    string FirstName,
    string LastName,
    string Phone)
{
    public static Application ToEntity(UpdateInstitutionApplicationDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Phone = dto.Phone,
        Email = null,
        Code = null
    };
}