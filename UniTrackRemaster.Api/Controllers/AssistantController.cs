using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Assistant;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.AI;
using ChatMessageDto = UniTrackRemaster.Api.Dto.Chat.ChatMessageDto;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssistantController : ControllerBase
{
    private readonly IAssistantService _assistantService;
    private readonly ILogger<AssistantController> _logger;

    public AssistantController(IAssistantService assistantService, ILogger<AssistantController> logger)
    {
        _assistantService = assistantService;
        _logger = logger;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<SendMessageResponse>> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            _logger.LogInformation("Processing chat message for user {UserId}", userId);

            var response = await _assistantService.ProcessChatMessageAsync(request, userId);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, "An error occurred while processing your message");
        }
    }

    [HttpGet("conversations/{conversationId}")]
    public async Task<ActionResult<List<ChatMessageDto>>> GetConversationHistory(string conversationId)
    {
        try
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var history = await _assistantService.GetConversationHistoryAsync(conversationId, userId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation history for {ConversationId}", conversationId);
            return StatusCode(500, "An error occurred while retrieving conversation history");
        }
    }

    [HttpDelete("conversations/{conversationId}")]
    public async Task<ActionResult> DeleteConversation(string conversationId)
    {
        try
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            await _assistantService.DeleteConversationAsync(conversationId, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation {ConversationId}", conversationId);
            return StatusCode(500, "An error occurred while deleting the conversation");
        }
    }
}
