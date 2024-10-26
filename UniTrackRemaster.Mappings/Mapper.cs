using UniTrackRemaster.Api.Dto.Response;

namespace UniTrackRemaster.Mappings;

public class Mapper : IMapper
{
    public LoginResponseDto? MapLoginResult(Guid userId, string role)
    {
        var model = new LoginResponseDto
        {
            UserId = userId.ToString(),
            UserRole = role,
            AvatarUrl = "example.png",
        };
        return model;

    }
}