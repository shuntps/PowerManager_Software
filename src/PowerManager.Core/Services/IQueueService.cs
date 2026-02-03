using PowerManager.Core.Models;

namespace PowerManager.Core.Services;

public interface IQueueService
{
    void Enqueue(QueueItem item);
    Task CancelItemAsync(QueueItem item);
    Task StartQueueAsync();
    IReadOnlyList<QueueItem> GetQueue();
    event EventHandler<QueueItem> ItemStatusChanged;
    event EventHandler<QueueItem> ItemAdded;
    event EventHandler<QueueItem> ItemCompleted; // Fired when install/uninstall succeeds
}
