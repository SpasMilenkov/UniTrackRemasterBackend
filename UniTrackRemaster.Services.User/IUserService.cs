using Microsoft.AspNetCore.Http;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Api.Dto.User;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.User;

public interface IUserService
{
    Task<UserDetailsResponse> GetUserDetailsAsync(Guid userId);
    public Task<ApplicationUser?> GetUserById(Guid id);
    Task<PaginatedUserListDto> GetAllUsersAsync(UserPaginationParams parameters, CancellationToken cancellationToken = default);
    Task<UserDetailsResponse> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto profileDto);
    Task<string> UploadProfileImageAsync(Guid userId, IFormFile image);
    Task<PrivacySettingsDto> UpdatePrivacySettingsAsync(Guid userId, UpdatePrivacySettingsDto settingsDto);
    Task<List<InstitutionDto>> GetUserInstitutionsAsync(Guid userId);
    Task<PrivacySettingsDto> GetPrivacySettingsAsync(Guid userId);
    Task<bool> UserHasAccessToGroupAsync(Guid userId, Guid groupId, string groupType);

    Task<bool> IsUserAdminAsync(Guid userId);
    Task<bool> IsUserSuperAdminAsync(Guid userId);
    Task<bool> CanUserManageChatMessageAsync(Guid userId, Guid? recipientId, Guid? groupId);
    Task<bool> IsUserAdminInInstitutionAsync(Guid userId, Guid institutionId);
}