using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.User;

public record PrivacySettingsDto(
    bool DataAnalytics,
    bool EmailUpdates,
    bool MarketingEmails,
    ProfileVisibility ProfileVisibility);
