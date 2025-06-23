using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Chat.Events;
using UniTrackRemaster.Commons.Services;

namespace UniTrackRemaster.Services.Messaging;

public class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();
    private readonly ConcurrentDictionary<string, Timer> _typingTimers = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger<ConnectionManager> _logger;

    public ConnectionManager(IEventBus eventBus, ILogger<ConnectionManager> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task AddConnectionAsync(Guid userId, string connectionId)
    {
        _userConnections.AddOrUpdate(userId,
            new HashSet<string> { connectionId },
            (key, connections) =>
            {
                lock (connections)
                {
                    connections.Add(connectionId);
                }
                return connections;
            });

        await _eventBus.PublishAsync(new UserConnectedEvent
        {
            UserId = userId,
            ConnectionId = connectionId,
            ConnectedAt = DateTime.UtcNow
        });

        _logger.LogDebug("Added connection {ConnectionId} for user {UserId}", connectionId, userId);
    }

    public async Task<bool> RemoveConnectionAsync(Guid userId, string connectionId)
    {
        var isLastConnection = false;

        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                    isLastConnection = true;
                }
            }
        }

        await _eventBus.PublishAsync(new UserDisconnectedEvent
        {
            UserId = userId,
            ConnectionId = connectionId,
            IsLastConnection = isLastConnection,
            DisconnectedAt = DateTime.UtcNow
        });

        _logger.LogDebug("Removed connection {ConnectionId} for user {UserId}, last connection: {IsLast}",
            connectionId, userId, isLastConnection);

        return isLastConnection;
    }

    public Task<List<string>> GetConnectionsAsync(Guid userId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                return Task.FromResult(connections.ToList());
            }
        }

        return Task.FromResult(new List<string>());
    }

    public Task<List<Guid>> GetOnlineUsersAsync()
    {
        return Task.FromResult(_userConnections.Keys.ToList());
    }

    public async Task SetTypingTimeoutAsync(Guid userId, Guid? recipientId, Guid? groupId, string? groupType)
    {
        var key = $"{userId}_{recipientId ?? groupId ?? Guid.Empty}";

        // Cancel existing timer
        if (_typingTimers.TryRemove(key, out var existingTimer))
        {
            await existingTimer.DisposeAsync();
        }

        // Create new timer that stops typing after 3 seconds
        var timer = new Timer(async _ =>
        {
            await _eventBus.PublishAsync(new UserStoppedTypingEvent
            {
                UserId = userId,
                RecipientId = recipientId,
                GroupId = groupId,
                GroupType = groupType
            });

            _typingTimers.TryRemove(key, out var _);
        }, null, TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);

        _typingTimers.TryAdd(key, timer);
    }

    public Task ClearTypingTimeoutAsync(Guid userId)
    {
        // Remove all typing timers for this user
        var keysToRemove = _typingTimers.Keys.Where(k => k.StartsWith($"{userId}_")).ToList();

        foreach (var key in keysToRemove)
        {
            if (_typingTimers.TryRemove(key, out var timer))
            {
                timer.Dispose();
            }
        }

        return Task.CompletedTask;
    }
}
