using Microsoft.Extensions.Logging;
using PowerManager.Core.Models;
using PowerManager.Core.Enums;

namespace PowerManager.Core.Services.Implementations;

public partial class QueueService(ILogger<QueueService> logger, IWingetService wingetService) : IQueueService
{
    private readonly List<QueueItem> _queue = [];
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public event EventHandler<QueueItem>? ItemStatusChanged;
    public event EventHandler<QueueItem>? ItemAdded;
    public event EventHandler<QueueItem>? ItemCompleted;

    public void Enqueue(QueueItem item)
    {
        _queue.Add(item);
        ItemAdded?.Invoke(this, item);
        _ = ProcessQueueAsync();
    }

    public async Task CancelItemAsync(QueueItem item)
    {
        item.CancellationTokenSource?.Cancel();
        item.Status = QueueItemStatus.Canceled;
        ItemStatusChanged?.Invoke(this, item);
        await Task.CompletedTask;
    }

    public IReadOnlyList<QueueItem> GetQueue()
    {
        return _queue.AsReadOnly();
    }

    public async Task StartQueueAsync()
    {
        await ProcessQueueAsync();
    }

    private async Task ProcessQueueAsync()
    {
        while (true)
        {
            QueueItem? item = null;
            lock (_queue)
            {
                item = _queue.FirstOrDefault(x => x.Status == QueueItemStatus.Pending);
            }

            if (item == null) return;

            await _semaphore.WaitAsync();
            try
            {
                if (item.Status != QueueItemStatus.Pending) continue;

                item.Status = QueueItemStatus.Running;
                item.CancellationTokenSource = new CancellationTokenSource();
                ItemStatusChanged?.Invoke(this, item);

                try
                {
                    LogStartingAction(logger, item.Action, item.PackageId);

                    if (item.Action.Equals("Install", StringComparison.OrdinalIgnoreCase))
                    {
                        await wingetService.InstallPackageAsync(item.PackageId, item.CancellationTokenSource.Token);
                    }
                    else if (item.Action.Equals("Uninstall", StringComparison.OrdinalIgnoreCase))
                    {
                        await wingetService.UninstallPackageAsync(item.PackageId, item.CancellationTokenSource.Token);
                    }

                    item.Status = QueueItemStatus.Completed;
                    ItemCompleted?.Invoke(this, item);
                }
                catch (OperationCanceledException)
                {
                    item.Status = QueueItemStatus.Canceled;
                }
                catch (Exception ex)
                {
                    LogActionFailed(logger, item.PackageId, ex);
                    item.Status = QueueItemStatus.Failed;
                    item.Logs.AppendLine(ex.Message);
                }
                finally
                {
                    ItemStatusChanged?.Invoke(this, item);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    [LoggerMessage(LogLevel.Information, "Starting action {Action} on {PackageId}")]
    private static partial void LogStartingAction(ILogger logger, string action, string packageId);

    [LoggerMessage(LogLevel.Error, "Action failed for {PackageId}")]
    private static partial void LogActionFailed(ILogger logger, string packageId, Exception exception);
}
