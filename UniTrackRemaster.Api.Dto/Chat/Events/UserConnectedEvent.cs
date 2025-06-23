namespace UniTrackRemaster.Api.Dto.Chat.Events;

public class UserConnectedEvent
{
    public Guid UserId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
}