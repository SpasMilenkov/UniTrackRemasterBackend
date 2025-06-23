using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Commons.Services;

namespace UniTrackRemaster.Services.Messaging;

public class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly ILogger<InMemoryEventBus> _logger;
    private readonly object _lock = new object();

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger)
    {
        _logger = logger;
    }

    public async Task PublishAsync<T>(T eventData) where T : class
    {
        lock (_lock)
        {
            _logger.LogInformation("Publishing event {EventType} with data: {EventData}", typeof(T).Name, eventData);
        }
        var eventType = typeof(T);

        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            _logger.LogDebug("Found {HandlerCount} handlers for event {EventType}", handlers.Count, eventType.Name);
            
            var tasks = handlers.Select(handler =>
            {
                var typedHandler = (Func<T, Task>)handler;
                return Task.Run(async () =>
                {
                    try
                    {
                        await typedHandler(eventData);
                        _logger.LogDebug("Successfully handled event {EventType}", eventType.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling event {EventType}", eventType.Name);
                    }
                });
            });

            await Task.WhenAll(tasks);
        }
        else
        {
            _logger.LogWarning("No handlers registered for event type {EventType}", eventType.Name);
        }
    }

    public void Subscribe<T>(Func<T, Task> handler) where T : class
    {
        var eventType = typeof(T);
        
        lock (_lock)
        {
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Delegate>();
            }
            
            _handlers[eventType].Add(handler);
            _logger.LogInformation("Subscribed handler for event type {EventType}. Total handlers: {HandlerCount}", 
                eventType.Name, _handlers[eventType].Count);
        }
    }

    public void Unsubscribe<T>(Func<T, Task> handler) where T : class
    {
        var eventType = typeof(T);
        
        lock (_lock)
        {
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
                _logger.LogInformation("Unsubscribed handler for event type {EventType}. Remaining handlers: {HandlerCount}", 
                    eventType.Name, handlers.Count);
                
                if (handlers.Count == 0)
                {
                    _handlers.TryRemove(eventType, out _);
                    _logger.LogDebug("Removed empty handler list for event type {EventType}", eventType.Name);
                }
            }
        }
    }

    public int GetHandlerCount<T>() where T : class
    {
        var eventType = typeof(T);
        return _handlers.TryGetValue(eventType, out var handlers) ? handlers.Count : 0;
    }

    public int GetTotalHandlerCount()
    {
        return _handlers.Values.Sum(h => h.Count);
    }
}