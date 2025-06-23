using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.User;

public class UpdatePrivacySettingsDto
{
    public bool? DataAnalytics { get; set; }
    public bool? EmailUpdates { get; set; }
    public bool? MarketingEmails { get; set; }
    public ProfileVisibility? ProfileVisibility { get; set; }
}