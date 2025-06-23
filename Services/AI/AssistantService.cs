using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Assistant;
using UniTrackRemaster.Commons.Services;

namespace UniTrackRemaster.Services.AI;

public class AssistantService : IAssistantService
{
    private readonly IOllamaService _ollamaService;
    private readonly IMemoryCache _cache;
    private readonly ILogger _logger;
    private readonly TimeSpan _conversationCacheTime = TimeSpan.FromHours(24);

    public AssistantService(
        IOllamaService ollamaService,
        IMemoryCache cache,
        ILogger<AssistantService> logger)
    {
        _ollamaService = ollamaService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<SendMessageResponse> ProcessChatMessageAsync(SendMessageRequest request, string userId)
    {
        var conversationId = request.ConversationId ?? GenerateConversationId();
        var cacheKey = GetConversationCacheKey(conversationId, userId);

        // Get or create conversation history
        var conversation = _cache.Get<List<AssistantChatMessageDto>>(cacheKey) ?? new List<AssistantChatMessageDto>();

        // Add user message to conversation
        var userMessage = new AssistantChatMessageDto
        {
            Id = $"user_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Role = "user",
            Content = request.Message,
            Timestamp = DateTime.UtcNow,
            Error = false
        };

        conversation.Add(userMessage);

        try
        {
            // Generate system prompt with context
            var systemPrompt = GenerateSystemPrompt(request.Context);
            
            // Create conversation context for the AI
            var conversationContext = BuildConversationContext(conversation);
            
            // Generate response using Ollama
            var aiResponse = await _ollamaService.GenerateChatResponseAsync(
                $"{conversationContext}\n\nUser: {request.Message}",
                systemPrompt
            );

            // Create assistant message
            var assistantMessage = new AssistantChatMessageDto
            {
                Id = $"assistant_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Role = "assistant",
                Content = aiResponse.Trim(),
                Timestamp = DateTime.UtcNow,
                Error = false
            };

            conversation.Add(assistantMessage);

            // Cache the updated conversation
            _cache.Set(cacheKey, conversation, _conversationCacheTime);

            _logger.LogInformation("Processed chat message for user {UserId} in conversation {ConversationId}", 
                userId, conversationId);

            return new SendMessageResponse
            {
                Message = assistantMessage,
                ConversationId = conversationId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI response for user {UserId}", userId);
            
            var errorMessage = new AssistantChatMessageDto
            {
                Id = $"assistant_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Role = "assistant",
                Content = "I apologize, but I'm experiencing technical difficulties. Please try again in a moment or contact support if the issue persists.",
                Timestamp = DateTime.UtcNow,
                Error = true
            };

            conversation.Add(errorMessage);
            _cache.Set(cacheKey, conversation, _conversationCacheTime);

            return new SendMessageResponse
            {
                Message = errorMessage,
                ConversationId = conversationId
            };
        }
    }

    public async Task<List<AssistantChatMessageDto>> GetConversationHistoryAsync(string conversationId, string userId)
    {
        var cacheKey = GetConversationCacheKey(conversationId, userId);
        var conversation = _cache.Get<List<AssistantChatMessageDto>>(cacheKey);
        
        return await Task.FromResult(conversation ?? []);
    }

    public async Task DeleteConversationAsync(string conversationId, string userId)
    {
        var cacheKey = GetConversationCacheKey(conversationId, userId);
        _cache.Remove(cacheKey);
        
        _logger.LogInformation("Deleted conversation {ConversationId} for user {UserId}", conversationId, userId);
        
        await Task.CompletedTask;
    }

    private string GenerateSystemPrompt(ChatContextDto? context)
    {
        var systemPrompt = """
            You are UniTrack Support Assistant, an AI helper for the UniTrack educational management platform. 
            UniTrack is a comprehensive system for tracking and managing educational institutions, students, and analytics.

            ## About UniTrack:
            - **Purpose**: Educational institution management and analytics platform
            - **Key Features**: 
              - User and institution management with role-based access
              - Student tracking and progress monitoring
              - Advanced analytics and reporting
              - Privacy controls and data protection
              - AI-powered insights and recommendations
              - Vector-based search for educational content and reports

            ## User Roles in UniTrack:
            - **SuperAdmin**: Full system access, can manage all institutions
            - **InstitutionAdmin**: Manages their specific institution
            - **Teacher**: Manages classes and student data
            - **Student**: Views their own progress and data
            - **Guest**: Limited access, registration required

            ## Your Capabilities:
            1. **General Support**: Answer questions about UniTrack features and functionality
            2. **Navigation Help**: Guide users through the interface and workflows
            3. **Troubleshooting**: Help resolve common issues and errors
            4. **Best Practices**: Provide recommendations for effective platform usage
            5. **Feature Explanations**: Explain how various UniTrack features work
            6. **Account & Privacy**: Help with account settings and privacy controls

            ## Guidelines:
            - Be helpful, professional, and patient
            - Provide step-by-step instructions when appropriate
            - Ask clarifying questions if needed
            - If you cannot help with something, suggest contacting human support
            - Never ask for or handle sensitive information like passwords
            - Focus on UniTrack-related topics, but be helpful with general educational queries
            - Use clear, non-technical language unless technical details are specifically requested
            """;

        if (context == null) return systemPrompt;
        systemPrompt += $"\n\n## Current User Context:";
            
        if (!string.IsNullOrEmpty(context.CurrentPage))
        {
            systemPrompt += $"\n- Current Page: {GetPageDescription(context.CurrentPage)}";
        }
            
        if (!string.IsNullOrEmpty(context.UserRole))
        {
            systemPrompt += $"\n- User Role: {context.UserRole}";
        }
            
        if (!string.IsNullOrEmpty(context.InstitutionId))
        {
            systemPrompt += $"\n- User has an associated institution (ID: {context.InstitutionId})";
        }

        if (context.ErrorDetails == null) return systemPrompt;
        systemPrompt += $"\n- User may be experiencing an error or issue";
        systemPrompt += $"\n- Error Context: {JsonSerializer.Serialize(context.ErrorDetails)}";

        return systemPrompt;
    }

    private string GetPageDescription(string currentPage)
    {
        return currentPage.ToLower() switch
        {
            "/login" => "Login Page",
            "/register" => "Registration Page",
            "/users/dashboard" => "User Dashboard",
            "/super-admins/dashboard" => "Super Admin Dashboard",
            "/users/profile" => "User Profile Settings",
            "/users/privacy" => "Privacy Settings",
            "/institutions" => "Institution Management",
            "/analytics" => "Analytics and Reporting",
            "/support" => "Support Chat",
            var path when path.Contains("dashboard") => "Dashboard",
            var path when path.Contains("profile") => "Profile Section",
            var path when path.Contains("settings") => "Settings",
            var path when path.Contains("admin") => "Administrative Section",
            _ => currentPage
        };
    }

    private string BuildConversationContext(List<AssistantChatMessageDto> conversation)
    {
        if (conversation.Count <= 1)
            return "This is the start of a new conversation.";

        var recentMessages = conversation
            .TakeLast(6) // Last 6 messages to maintain context without overwhelming the AI
            .Where(m => !m.Error) // Exclude error messages from context
            .ToList();

        if (recentMessages.Count <= 1)
            return "This is the start of a new conversation.";

        var contextBuilder = new System.Text.StringBuilder();
        contextBuilder.AppendLine("## Recent Conversation Context:");

        foreach (var message in recentMessages.Take(recentMessages.Count - 1)) // Exclude the current message
        {
            contextBuilder.AppendLine($"{message.Role}: {message.Content}");
        }

        return contextBuilder.ToString();
    }

    private string GenerateConversationId()
    {
        return $"conv_{Guid.NewGuid():N}";
    }

    private string GetConversationCacheKey(string conversationId, string userId)
    {
        return $"conversation_{userId}_{conversationId}";
    }
}