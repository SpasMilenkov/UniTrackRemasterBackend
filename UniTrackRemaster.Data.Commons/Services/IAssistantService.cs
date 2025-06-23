using UniTrackRemaster.Api.Dto.Assistant;

namespace UniTrackRemaster.Commons.Services;

public interface IAssistantService
{
    Task<SendMessageResponse> ProcessChatMessageAsync(SendMessageRequest request, string userId);
    Task<List<AssistantChatMessageDto>> GetConversationHistoryAsync(string conversationId, string userId);
    Task DeleteConversationAsync(string conversationId, string userId);
}
