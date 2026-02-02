namespace PowerManager.Core.Services;

public interface IUiDispatcher
{
    void TryEnqueue(Action action);
}
