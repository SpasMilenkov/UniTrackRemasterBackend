namespace UniTrackRemaster.Commons.Services;

public interface IEventBus
{
    Task PublishAsync<T>(T eventData) where T : class;
    void Subscribe<T>(Func<T, Task> handler) where T : class;
}