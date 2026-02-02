using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PowerManager.Core.Models;
using PowerManager.Core.Services;

namespace PowerManager.UI.ViewModels;

public partial class QueueViewModel : ObservableObject
{
    private readonly IQueueService _queueService;
    private readonly IUiDispatcher _dispatcher;

    [ObservableProperty]
    private ObservableCollection<QueueItem> _queue = [];

    public QueueViewModel(IQueueService queueService, IUiDispatcher dispatcher)
    {
        _queueService = queueService;
        _dispatcher = dispatcher;
        
        _queueService.ItemAdded += OnItemAdded;
        _queueService.ItemStatusChanged += OnItemStatusChanged;
        
        foreach(var item in _queueService.GetQueue())
        {
            Queue.Add(item);
        }
    }

    private void OnItemAdded(object? sender, QueueItem item)
    {
        _dispatcher.TryEnqueue(() => 
        {
            Queue.Add(item);
        });
    }

    private void OnItemStatusChanged(object? sender, QueueItem item)
    {
        _dispatcher.TryEnqueue(() => 
        {
             var index = Queue.IndexOf(item);
             if (index >= 0)
             {
                 Queue[index] = item;
             }
        });
    }
}
