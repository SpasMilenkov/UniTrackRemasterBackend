using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Chat.Events;

public class ChatMessageSentEvent
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public Guid? RecipientId { get; set; }
    public Guid? GroupId { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType MessageType { get; set; }
    public DateTime SentAt { get; set; }
}
