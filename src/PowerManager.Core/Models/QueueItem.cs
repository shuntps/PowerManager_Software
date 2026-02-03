using System.Text;
using PowerManager.Core.Enums;

namespace PowerManager.Core.Models;

public class QueueItem
{
    public string PackageId { get; set; } = string.Empty;
    public string Action { get; set; } = "Install"; // Install, Uninstall, Update
    public QueueItemStatus Status { get; set; } = QueueItemStatus.Pending;
    public double Progress { get; set; }
    public StringBuilder Logs { get; set; } = new StringBuilder();

    // Non-serialized runtime properties
    public CancellationTokenSource? CancellationTokenSource { get; set; }

    public QueueItem() { }
}
