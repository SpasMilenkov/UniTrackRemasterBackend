namespace UniTrackRemaster.Commons.Services;

public interface IConnectionManager
{
    Task AddConnectionAsync(Guid userId, string connectionId);
    Task<bool> RemoveConnectionAsync(Guid userId, string connectionId);
    Task<List<string>> GetConnectionsAsync(Guid userId);
    Task<List<Guid>> GetOnlineUsersAsync();
    Task SetTypingTimeoutAsync(Guid userId, Guid? recipientId, Guid? groupId, string? groupType);
    Task ClearTypingTimeoutAsync(Guid userId);
}
