using UniTrackBackend.Api.Dto.Response;
using UniTrackReimagined.Data.Models.Academical;

namespace Mappings;

public interface IMapper
{
    public LoginResponseDto? MapLoginResult(Guid userId, string role);
}