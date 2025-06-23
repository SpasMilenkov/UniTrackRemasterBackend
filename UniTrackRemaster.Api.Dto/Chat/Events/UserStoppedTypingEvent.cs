namespace UniTrackRemaster.Api.Dto.Chat.Events;

public class UserStoppedTypingEvent
{
    public Guid UserId { get; set; }
    public Guid? RecipientId { get; set; }
    public Guid? GroupId { get; set; }
    public string? GroupType { get; set; }
}