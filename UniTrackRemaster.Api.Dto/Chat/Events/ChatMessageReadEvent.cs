namespace UniTrackRemaster.Api.Dto.Chat.Events;

public class ChatMessageReadEvent
{
    public List<Guid> MessageIds { get; set; } = new();
    public Guid ReadByUserId { get; set; }
    public DateTime ReadAt { get; set; }
}