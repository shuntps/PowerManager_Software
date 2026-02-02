using Microsoft.UI.Dispatching;
using PowerManager.Core.Services;
using System;

namespace PowerManager.UI.Services;

public class UiDispatcher : IUiDispatcher
{
    private DispatcherQueue? _dispatcherQueue;

    public void Initialize(DispatcherQueue dispatcherQueue)
    {
        _dispatcherQueue = dispatcherQueue;
    }

    public void TryEnqueue(Action action)
    {
        _dispatcherQueue ??= DispatcherQueue.GetForCurrentThread();
        
        if (_dispatcherQueue == null)
        {
            throw new InvalidOperationException("UiDispatcher must be called from the UI thread or after UI thread initialization.");
        }
        
        _dispatcherQueue.TryEnqueue(() => action());
    }
}
