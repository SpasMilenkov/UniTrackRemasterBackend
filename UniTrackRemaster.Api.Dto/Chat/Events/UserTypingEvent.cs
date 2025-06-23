namespace UniTrackRemaster.Api.Dto.Chat.Events;

public class UserTypingEvent
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid? RecipientId { get; set; }
    public Guid? GroupId { get; set; }
    public string? GroupType { get; set; }
}