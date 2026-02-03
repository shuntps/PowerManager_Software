# PowerManager - WinUI 3 App for WinGet Management

**.NET 10** (C# 13) | **WinUI 3** (Windows App SDK 1.8) | **MVVM Toolkit 8.3.2** | **MSTest 4.0.1**

## 🎯 Critical Patterns (Must Follow)

### Versioning & Releases (AUTOMATED)

**Conventional Commits REQUIRED** - All commits MUST follow this format:

```bash
<type>(<scope>): <description>

# Version-affecting commits
feat(catalog): add search → MINOR bump (0.1.0 → 0.2.0)
fix(queue): deadlock issue → PATCH bump (0.1.0 → 0.1.1)
feat!: redesign API → MAJOR bump (0.1.0 → 1.0.0)

# Non-version commits (no release)
docs: update README
chore: upgrade dependencies
style: format code
```

**GitHub Actions fully automates releases on `main` branch:**

- Calculates version from commits → Updates `VERSION.txt` & `CHANGELOG.md`
- Builds/tests → Creates GitHub Release with binaries
- See `scripts/Get-NextVersion.ps1`, `scripts/Update-Changelog.ps1`, `scripts/New-Release.ps1`

**IMPORTANT**: Use PowerShell scripts, NOT Node.js/npm. All automation is Windows-native.

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
  - `Services/Implementations/` - Concrete service implementations
  - `Models/` - Data models with parameterless ctors + public setters (for YAML/JSON)
  - `Enums/` - Status enumerations
- `PowerManager.UI/` - WinUI 3 (Views retrieve VMs via DI, use `x:Bind` not `Binding`)
  - `ViewModels/` - MVVM ViewModels (partial classes with source generators)
  - `Views/` - XAML Pages
  - `Services/` - UI-specific services (UiDispatcher)
- `PowerManager.Tests/` - **MSTest 4.0.1** (NOT xUnit)
- `scripts/` - PowerShell automation (versioning, releases)
- `.github/workflows/` - CI/CD pipelines (release.yml, ci.yml, version-check.yml)

## 🔧 Commands

```powershell
# Development
dotnet restore
dotnet build -c Release          # Defaults to x64 platform
cd src/PowerManager.UI && dotnet run
dotnet test

# Versioning (automated via GitHub Actions)
.\scripts\Get-NextVersion.ps1                    # Calculate next version
.\scripts\Update-Changelog.ps1 -Version "0.2.0"  # Generate changelog
.\scripts\New-Release.ps1 -Version "0.2.0"       # Create GitHub Release
```

## ⚠️ Non-Negotiables

- **English only** (code, comments, logs, UI, commits)
- **No `.Wait()` / `.Result`** (use async/await everywhere)
- **ViewModels NEVER reference UI types** (Page, DispatcherQueue, XamlRoot)
- **Models need parameterless ctor + public setters** (YAML/JSON serialization)
- **Collection expressions**: `[]` not `new List<T>()`
- **Branch**: Use `main` branch (NOT master)
- **Conventional Commits**: Required for all commits (see versioning section)

## 🚧 v0.1.0 Status

**Implemented**: NavigationView shell, MVVM, DI, queue execution, IUiDispatcher, [LoggerMessage], basic WinGet, PowerShell versioning automation
**NOT Implemented**: Catalog persistence, splash screen, history logs, status cache, bulk ops, search/filter

## 🔍 Key Files to Reference

- `src/PowerManager.Core/Services/Implementations/WingetService.cs` - Process execution with UTF8 encoding
- `src/PowerManager.Core/Services/Implementations/QueueService.cs` - Event-driven queue with marshaling
- `src/PowerManager.UI/ViewModels/QueueViewModel.cs` - UI thread marshaling pattern
- `src/PowerManager.UI/App.xaml.cs` - DI container setup
- `src/PowerManager.UI/MainWindow.xaml.cs` - UiDispatcher initialization
- `scripts/Get-NextVersion.ps1` - Conventional Commits parser
- `.github/workflows/release.yml` - Automated release pipeline

## 🏗️ Architecture Deep Dive

### Service Layer Pattern

**All services follow interface-first design** - Core layer defines interfaces, implementations are concrete:

```csharp
// Core layer: PowerManager.Core/Services/IQueueService.cs
public interface IQueueService { }

// Implementation: PowerManager.Core/Services/Implementations/QueueService.cs
public partial class QueueService(ILogger<QueueService> logger, IWingetService winget) : IQueueService
```

**Why**: Enables DI, testability, and prevents circular dependencies between Core and UI.

### Event-Driven Queue Processing

`QueueService` uses **events for cross-thread communication** instead of callbacks:

```csharp
// Service raises event on background thread
ItemStatusChanged?.Invoke(this, item);

// ViewModel marshals to UI thread via IUiDispatcher
_queueService.ItemStatusChanged += (s, item) =>
    _dispatcher.TryEnqueue(() => UpdateUI(item));
```

**Why**: Decouples queue processing (background) from UI updates (main thread).

### DI Container Lifetime Rules

**Singletons**: Services that maintain state (WingetService, QueueService, UiDispatcher)
**Transient**: ViewModels (new instance per page navigation)

```csharp
// App.xaml.cs
services.AddSingleton<IQueueService, QueueService>();  // Shared queue state
services.AddTransient<CatalogViewModel>();             // Fresh instance per navigation
```

**Critical**: UiDispatcher MUST be singleton - initialized once in MainWindow, used by all VMs.

### Process Execution Pattern

WinGet commands use **async stream processing** with proper cancellation:

```csharp
process.OutputDataReceived += (sender, e) => outputBuilder.AppendLine(e.Data);
process.BeginOutputReadLine();  // Non-blocking
await process.WaitForExitAsync(cancellationToken);

// On cancel: kill process before throwing
if (!process.HasExited) process.Kill();
```

**Why**: `ReadToEnd()` blocks, breaks UI responsiveness. `BeginOutputReadLine()` is event-driven.

---

## Extended Documentation

For comprehensive architectural details, see the full project documentation in `README.md` and individual source files. The patterns above represent the **minimum critical knowledge** to avoid common pitfalls. When in doubt, follow examples from existing code in `CatalogViewModel.cs`, `QueueViewModel.cs`, `WingetService.cs`, and `QueueService.cs`.
