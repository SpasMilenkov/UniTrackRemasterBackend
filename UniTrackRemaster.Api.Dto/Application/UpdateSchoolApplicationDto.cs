namespace UniTrackRemaster.Api.Dto.Application;

public record UpdateInstitutionApplicationDto(
    string FirstName,
    string LastName,
    string Phone)
{
    public static Data.Models.Organizations.Application ToEntity(UpdateInstitutionApplicationDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Phone = dto.Phone,
        Email = null,
        Code = null
    };
}