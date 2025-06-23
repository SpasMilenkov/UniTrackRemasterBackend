namespace UniTrackRemaster.Api.Dto.Chat.Events;

public class UserDisconnectedEvent
{
    public Guid UserId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public bool IsLastConnection { get; set; }
    public DateTime DisconnectedAt { get; set; }
}