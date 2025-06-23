namespace UniTrackRemaster.Api.Dto.Chat.Events;

public class ChatMessageDeliveredEvent
{
    public Guid MessageId { get; set; }
    public Guid RecipientId { get; set; }
    public DateTime DeliveredAt { get; set; }
}