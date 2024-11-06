using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Mappings;

public interface IMapper
{
    public LoginResponseDto? MapLoginResult(Guid userId, string role);
}