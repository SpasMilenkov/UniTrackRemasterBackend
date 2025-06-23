using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Chat;


public class ChatMessage : BaseEntity
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public ApplicationUser Sender { get; set; } = null!;

    // For direct messages
    public Guid? RecipientId { get; set; }
    public ApplicationUser? Recipient { get; set; }

    // For group messages (institution/class)
    public Guid? GroupId { get; set; }

    public string Content { get; set; } = string.Empty;
    public MessageType MessageType { get; set; }
    public MessageStatus Status { get; set; }

    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }

    // For replies (you already have this!)
    public Guid? ParentMessageId { get; set; }
    public ChatMessage? ParentMessage { get; set; }
    public ICollection<ChatMessage> Replies { get; set; } = new List<ChatMessage>();

    // For attachments (future feature)
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }

    // For editing and deletion
    public string? OriginalContent { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; } // Who deleted it (could be sender or admin)
    public ApplicationUser? DeletedByUser { get; set; }

    // Navigation property for reactions
    public ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();

    // Navigation property for edit history
    public ICollection<MessageEditHistory> EditHistory { get; set; } = new List<MessageEditHistory>();
}
