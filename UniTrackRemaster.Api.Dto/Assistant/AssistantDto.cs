using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Assistant;


public class SendMessageRequest
{
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public string? ConversationId { get; set; }
    
    public ChatContextDto? Context { get; set; }
}



public class ChatContextDto
{
    public string? CurrentPage { get; set; }
    public string? UserRole { get; set; }
    public string? InstitutionId { get; set; }
    public string? UserId { get; set; }
    public object? ErrorDetails { get; set; }
}

public class SendMessageResponse
{
    public AssistantChatMessageDto Message { get; set; } = null!;
    public string ConversationId { get; set; } = string.Empty;
}

public class AssistantChatMessageDto
{
    public string Id { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool Error { get; set; }
}