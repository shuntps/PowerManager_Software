# PowerManager - WinUI 3 App for WinGet Management

**.NET 10** (C# 13) | **WinUI 3** (Windows App SDK 1.8) | **MVVM Toolkit 8.3.2** | **MSTest 4.0.1** | **v0.1.0**

## 🎯 Critical Patterns (Must Follow)

### MVVM with Source Generators

```csharp
// ViewModels: partial class + primary constructors + [ObservableProperty]/[RelayCommand]
public partial class CatalogViewModel(IWingetService winget, ILogger<CatalogViewModel> logger) : ObservableObject
{
    [ObservableProperty] private ObservableCollection<Package> _packages = [];
    [RelayCommand] private async Task LoadPackagesAsync() { }
}

// Views: Retrieve ViewModel via DI in code-behind
public CatalogPage() {
    InitializeComponent();
    ViewModel = ((App)App.Current).Services.GetRequiredService<CatalogViewModel>();
}
```

### UI Thread Marshaling (CRITICAL)

Service events → ViewModel handlers **MUST** marshal to UI thread via `IUiDispatcher`:

```csharp
_queueService.ItemAdded += (s, item) => _dispatcher.TryEnqueue(() => Queue.Add(item));
```

**WHY**: `ObservableCollection<T>` throws if modified from background threads.

### Startup Sequence (Non-Standard)

1. `Program.cs` sets `DISABLE_XAML_GENERATED_MAIN` + `DispatcherQueueSynchronizationContext`
2. `App.xaml.cs` builds DI container (Singletons: services, Transient: ViewModels)
3. **`MainWindow.ctor` MUST initialize `UiDispatcher`** or marshaling fails:
   ```csharp
   var dispatcher = Services.GetRequiredService<IUiDispatcher>();
   if (dispatcher is UiDispatcher ui) ui.Initialize(this.DispatcherQueue);
   ```

### WinGet Process Execution

```csharp
// MUST use UTF8 encoding + async output reading + kill on cancellation
var psi = new ProcessStartInfo {
    FileName = "winget",
    StandardOutputEncoding = Encoding.UTF8  // Non-ASCII package names
};
process.BeginOutputReadLine(); // Not ReadToEnd()!
catch (OperationCanceledException) {
    if (!process.HasExited) process.Kill();
}
```

### Logging (Compile-Time)

```csharp
public partial class MyService {  // MUST be partial
    [LoggerMessage(LogLevel.Error, "Failed: {Reason}")]
    private static partial void LogError(ILogger logger, string reason, Exception ex);
}
```

## 📁 Project Structure

- `PowerManager.Core/` - Business logic (NO UI dependencies, ALL services have interfaces)
- `PowerManager.UI/` - WinUI 3 (Views retrieve VMs via DI, use `x:Bind` not `Binding`)
- `PowerManager.Tests/` - **MSTest 4.0.1** (NOT xUnit)

## 🔧 Commands

```powershell
dotnet restore
dotnet build -c Release          # Defaults to x64
cd src/PowerManager.UI && dotnet run
dotnet test
```

## ⚠️ Non-Negotiables

- **English only** (code, comments, logs, UI, commits)
- **No `.Wait()` / `.Result`** (use async/await)
- **ViewModels NEVER reference UI types** (Page, DispatcherQueue, XamlRoot)
- **Models need parameterless ctor + public setters** (YAML/JSON serialization)
- **Collection expressions**: `[]` not `new List<T>()`

## 🚧 v0.1.0 Status

**Implemented**: NavigationView shell, MVVM, DI, queue execution, IUiDispatcher, [LoggerMessage], basic WinGet
**NOT Implemented**: Catalog persistence, splash screen, history logs, status cache, bulk ops, search/filter

---

## Extended Documentation

For comprehensive architectural details, see the full project documentation in `README.md` and individual source files. The patterns above represent the **minimum critical knowledge** to avoid common pitfalls. When in doubt, follow examples from existing code in `CatalogViewModel.cs`, `QueueViewModel.cs`, `WingetService.cs`, and `QueueService.cs`.
