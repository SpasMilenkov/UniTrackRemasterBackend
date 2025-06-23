using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Chat;
using System.Security.Claims;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.Messaging;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    // ========== BASIC CHAT OPERATIONS ==========
    [HttpGet("conversations")]
    public async Task<ActionResult<GetConversationsResponseDto>> GetConversations()
    {
        var userId = GetUserId();
        var result = await _chatService.GetConversationsAsync(userId);
        return Ok(result);
    }

    [HttpGet("conversation")]
    public async Task<ActionResult<ConversationDto>> GetConversation(
        [FromQuery] string conversationType,
        [FromQuery] Guid? otherUserId = null,
        [FromQuery] Guid? groupId = null)
    {
        var userId = GetUserId();
        var result = await _chatService.GetConversationAsync(userId, conversationType, otherUserId, groupId);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("messages")]
    public async Task<ActionResult<GetMessagesResponseDto>> GetMessages([FromQuery] GetMessagesDto dto)
    {
        var userId = GetUserId();
        var result = await _chatService.GetMessagesAsync(userId, dto);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _chatService.GetUnreadCountAsync(userId);
        return Ok(count);
    }

    [HttpPost("search")]
    public async Task<ActionResult<List<ChatMessageDto>>> SearchMessages([FromBody] SearchMessagesDto dto)
    {
        var userId = GetUserId();
        var results = await _chatService.SearchMessagesAsync(userId, dto);
        return Ok(results);
    }

    // ========== MESSAGE SENDING ==========
    [HttpPost("messages/direct")]
    public async Task<ActionResult<ChatMessageDto>> SendDirectMessage([FromBody] SendDirectMessageDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _chatService.SendDirectMessageAsync(userId, dto);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending direct message for user {UserId}", GetUserId());
            return StatusCode(500, "An error occurred while sending the message");
        }
    }

    [HttpPost("messages/group")]
    public async Task<ActionResult<ChatMessageDto>> SendGroupMessage([FromBody] SendGroupMessageDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _chatService.SendGroupMessageAsync(userId, dto);
            return Ok(result);
        }
        catch (ForbiddenException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending group message for user {UserId}", GetUserId());
            return StatusCode(500, "An error occurred while sending the message");
        }
    }

    [HttpPost("messages/{messageId:guid}/read")]
    public async Task<ActionResult> MarkMessagesAsRead([FromRoute] Guid messageId)
    {
        try
        {
            var userId = GetUserId();
            await _chatService.MarkMessagesAsReadAsync(userId, new List<Guid> { messageId });
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message as read for user {UserId}", GetUserId());
            return StatusCode(500, "An error occurred while marking message as read");
        }
    }

    [HttpPost("messages/read")]
    public async Task<ActionResult> MarkMultipleMessagesAsRead([FromBody] MarkMessagesReadDto dto)
    {
        try
        {
            var userId = GetUserId();
            await _chatService.MarkMessagesAsReadAsync(userId, dto.MessageIds);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read for user {UserId}", GetUserId());
            return StatusCode(500, "An error occurred while marking messages as read");
        }
    }

    // ========== MESSAGE EDITING ==========
    [HttpPut("messages/{messageId:guid}")]
    public async Task<ActionResult<ChatMessageDto>> EditMessage(
        [FromRoute] Guid messageId,
        [FromBody] EditMessageDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _chatService.EditMessageAsync(messageId, userId, dto);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ForbiddenException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing message {MessageId} for user {UserId}", messageId, GetUserId());
            return StatusCode(500, "An error occurred while editing the message");
        }
    }

    [HttpGet("messages/{messageId:guid}/can-edit")]
    public async Task<ActionResult<bool>> CanEditMessage([FromRoute] Guid messageId)
    {
        var userId = GetUserId();
        var canEdit = await _chatService.CanEditMessageAsync(messageId, userId);
        return Ok(canEdit);
    }

    [HttpGet("messages/{messageId:guid}/edit-history")]
    public async Task<ActionResult<GetMessageEditHistoryResponseDto>> GetMessageEditHistory([FromRoute] Guid messageId)
    {
        try
        {
            var request = new GetMessageEditHistoryDto(messageId);
            var result = await _chatService.GetMessageEditHistoryAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting edit history for message {MessageId}", messageId);
            return StatusCode(500, "An error occurred while retrieving edit history");
        }
    }

    // ========== MESSAGE DELETION ==========
    [HttpDelete("messages/{messageId:guid}")]
    public async Task<ActionResult> DeleteMessage(
        [FromRoute] Guid messageId,
        [FromQuery] bool isHardDelete = false)
    {
        try
        {
            var userId = GetUserId();
            var dto = new DeleteMessageDto(isHardDelete);
            var success = await _chatService.DeleteMessageAsync(messageId, userId, dto);

            if (!success)
                return BadRequest("Failed to delete message");

            return Ok();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ForbiddenException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId} for user {UserId}", messageId, GetUserId());
            return StatusCode(500, "An error occurred while deleting the message");
        }
    }

    [HttpGet("messages/{messageId:guid}/can-delete")]
    public async Task<ActionResult<bool>> CanDeleteMessage([FromRoute] Guid messageId)
    {
        var userId = GetUserId();
        var canDelete = await _chatService.CanDeleteMessageAsync(messageId, userId);
        return Ok(canDelete);
    }

    // ========== REACTIONS ==========
    [HttpPost("messages/{messageId:guid}/reactions")]
    public async Task<ActionResult<MessageReactionsSummaryDto>> AddReaction(
        [FromRoute] Guid messageId,
        [FromBody] AddReactionDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _chatService.AddReactionAsync(messageId, userId, dto);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding reaction to message {MessageId} for user {UserId}", messageId, GetUserId());
            return StatusCode(500, "An error occurred while adding the reaction");
        }
    }

    [HttpDelete("messages/{messageId:guid}/reactions")]
    public async Task<ActionResult<MessageReactionsSummaryDto>> RemoveReaction(
        [FromRoute] Guid messageId,
        [FromBody] AddReactionDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _chatService.RemoveReactionAsync(messageId, userId, dto);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reaction from message {MessageId} for user {UserId}", messageId, GetUserId());
            return StatusCode(500, "An error occurred while removing the reaction");
        }
    }

    [HttpGet("messages/{messageId:guid}/reactions")]
    public async Task<ActionResult<GetMessageReactionsResponseDto>> GetMessageReactions([FromRoute] Guid messageId)
    {
        try
        {
            var request = new GetMessageReactionsDto(messageId);
            var result = await _chatService.GetMessageReactionsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reactions for message {MessageId}", messageId);
            return StatusCode(500, "An error occurred while retrieving reactions");
        }
    }

    [HttpGet("messages/{messageId:guid}/reaction-counts")]
    public async Task<ActionResult<Dictionary<ReactionType, int>>> GetReactionCounts([FromRoute] Guid messageId)
    {
        try
        {
            var result = await _chatService.GetReactionCountsAsync(messageId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reaction counts for message {MessageId}", messageId);
            return StatusCode(500, "An error occurred while retrieving reaction counts");
        }
    }

    // ========== REPLIES ==========
    [HttpGet("messages/{parentMessageId:guid}/replies")]
    public async Task<ActionResult<GetMessageRepliesResponseDto>> GetMessageReplies(
        [FromRoute] Guid parentMessageId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var request = new GetMessageRepliesDto(parentMessageId, page, pageSize);
            var result = await _chatService.GetMessageRepliesAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting replies for message {ParentMessageId}", parentMessageId);
            return StatusCode(500, "An error occurred while retrieving replies");
        }
    }

    // ========== UTILITY ENDPOINTS ==========
    [HttpGet("messages/{messageId:guid}")]
    public async Task<ActionResult<ChatMessageDto>> GetMessage([FromRoute] Guid messageId)
    {
        try
        {
            var userId = GetUserId();
            var message = await _chatService.GetMessageByIdAsync(messageId);

            if (message == null)
                return NotFound("Message not found");

            // Check if user has access to this message
            var hasAccess = await _chatService.UserCanAccessMessageAsync(userId, messageId);
            if (!hasAccess)
                return Forbid("You don't have access to this message");

            var result = await _chatService.MapToDtoWithReactionsAsync(message, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message {MessageId} for user {UserId}", messageId, GetUserId());
            return StatusCode(500, "An error occurred while retrieving the message");
        }
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public ActionResult GetHealth()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }

    // ========== HELPER METHODS ==========
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        return userId;
    }
}