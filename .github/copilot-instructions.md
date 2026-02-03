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

## 🚧 Current Status (v0.3.0)

**Implemented**:

- NavigationView shell, MVVM, DI, queue execution, IUiDispatcher, [LoggerMessage]
- **YAML Catalog System**: Dual catalog (default + custom) with persistence to %LocalAppData%
- **Real-time Search/Filtering**: Category-based filtering with live updates
- **Status Detection**: Per-package scanning with progress overlay, LastChecked timestamps
- **Source Detection**: Distinguishes winget vs msstore packages
- PowerShell versioning automation with GitHub Actions

**NOT Implemented**: Splash screen, history logs (persistent), bulk operations UI, caching with TTL

## 🔍 Key Files to Reference

- `src/PowerManager.Core/Services/Implementations/WingetService.cs` - Process execution with UTF8 encoding, WinGet output parsing
- `src/PowerManager.Core/Services/Implementations/QueueService.cs` - Event-driven queue with marshaling
- `src/PowerManager.Core/Services/Implementations/CatalogService.cs` - YAML persistence, dual catalog merging
- `src/PowerManager.UI/ViewModels/CatalogViewModel.cs` - Search/filter implementation, background scanning
- `src/PowerManager.UI/ViewModels/QueueViewModel.cs` - UI thread marshaling pattern
- `src/PowerManager.UI/Converters/StatusConverters.cs` - XAML converters for status badges
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

### YAML Catalog System (v0.3.0)

**Storage Location**: `%LocalAppData%\PowerManager Software\`

- `catalog_default.yaml` - 5 pre-configured apps (auto-generated on first launch)
- `catalog_custom.yaml` - User additions (planned for v0.4.0)

```csharp
// CatalogService merges both catalogs at runtime
public async Task<List<Package>> GetMergedCatalogAsync() {
    var defaultCatalog = await GetDefaultCatalogAsync();
    var customCatalog = await GetCustomCatalogAsync();
    // Dictionary merge (custom overrides default)
}

// Package model MUST have parameterless ctor for YamlDotNet
public class Package {
    public string Id { get; set; } = string.Empty;  // Required: public setter
    public List<string> Tags { get; set; } = [];    // Collection expressions
    public Package() { }  // Required for YAML deserialization
}
```

### XAML Data Binding Patterns

**Always use `x:Bind`** (compiled binding) over `Binding` (runtime):

```xaml
<!-- ✅ Correct: Compiled binding with Mode -->
<TextBlock Text="{x:Bind ViewModel.SearchQuery, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

<!-- ❌ Wrong: Runtime binding -->
<TextBlock Text="{Binding SearchQuery}"/>
```

**Status Converters** for conditional visibility:

```xaml
<!-- App.xaml: Register converters as static resources -->
<converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>

<!-- Page: Use in binding -->
<Grid Visibility="{x:Bind ViewModel.IsScanning, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}">
```

### Search & Filter Implementation

**Real-time filtering** via property setter + `ApplyFilters()`:

```csharp
// CatalogViewModel pattern
[ObservableProperty] private string _searchQuery = string.Empty;

partial void OnSearchQueryChanged(string value) {
    ApplyFilters();  // Triggers on every keystroke
}

private void ApplyFilters() {
    var filtered = _allPackages
        .Where(p => string.IsNullOrEmpty(SearchQuery) ||
                    p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        .Where(p => SelectedCategory == "All Categories" || p.Category == SelectedCategory);

    dispatcher.TryEnqueue(() => {  // ✅ MUST marshal to UI thread
        Packages.Clear();
        foreach (var pkg in filtered) Packages.Add(pkg);
    });
}
```

### WinGet Output Parsing Strategy

**WinGet output format varies by locale** - parser must be resilient:

```csharp
// WingetService.ParseInstalledVersion() pattern
private string ParseInstalledVersion(string output) {
    // Skip headers (can be "Name", "Nom", etc.)
    if (line.Contains("Nom") || line.Contains("Name") || line.Contains("Version"))
        continue;

    // Find ID by pattern: contains dot AND has letters (not just version number)
    if (part.Contains('.') && part.Any(char.IsLetter)) {
        // Next part is likely the version
    }
}
```

**Fallback strategy**: Try `--exact` match first, then partial match for edge cases.

### Common Pitfalls & Debugging

**Issue**: ObservableCollection throws "This type of CollectionView does not support changes"
**Fix**: Always wrap collection changes in `_dispatcher.TryEnqueue(() => { })`

**Issue**: ViewModel properties not updating UI
**Fix**: Ensure `[ObservableProperty]` on private field (not public property) and class is `partial`

**Issue**: WinGet commands fail silently
**Fix**: Check `StandardOutputEncoding = Encoding.UTF8` and verify output parsing handles localized headers

**Issue**: UiDispatcher is null in ViewModel
**Fix**: Verify `MainWindow.ctor` calls `uiDispatcher.Initialize(this.DispatcherQueue)` before any page navigation

**Debugging WinGet parsing**:

```csharp
// Add temporary logging to see actual WinGet output
logger.LogInformation("Raw WinGet output:\n{Output}", output);
```

### Error Handling Strategy

**Services**: Catch exceptions, log with `[LoggerMessage]`, return null/empty for non-critical errors:

```csharp
try {
    var result = await wingetService.GetPackageInfoAsync(id);
} catch (Exception ex) {
    LogPackageScanFailed(logger, id, ex);
    return null;  // Allow app to continue
}
```

**Queue operations**: Let exceptions bubble to QueueService which sets `Status = Failed`:

```csharp
// QueueService.ProcessQueueAsync handles all exceptions
catch (Exception ex) {
    item.Status = QueueItemStatus.Failed;
    item.Logs.AppendLine(ex.Message);
}
```

**UI layer**: Display user-friendly messages, never expose stack traces to users (planned for v1.0.0)

---

## Extended Documentation

For comprehensive architectural details, see `README.md`, `ROADMAP.md`, and individual source files. The patterns above represent the **minimum critical knowledge** to avoid common pitfalls. When in doubt, follow examples from existing code in `CatalogViewModel.cs`, `CatalogService.cs`, `WingetService.cs`, and `QueueService.cs`.
