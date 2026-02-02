# PowerManager Software – AI‑Optimized Coding Instructions

> **Audience**: AI coding agents and human contributors
> **Goal**: Produce clean, testable, production‑grade WinUI 3 code with strict architectural guarantees.

---

## 1. Project Overview

**PowerManager Software** is a WinUI 3 desktop application for bulk Windows software management using **WinGet**.

- Target framework: **.NET 10** (Windows App SDK 1.8.260101001)
- UI framework: **WinUI 3** with **CommunityToolkit.Mvvm 8.3.2**
- Platform: Windows 10 1809+ / Windows 11
- Testing: **MSTest 4.0.1** (NOT xUnit)

---

## ⚠️ IMPORTANT NOTE

> **ALL source code, comments, documentation, logs, UI text, and commit messages MUST be written in English.**
>
> This is a NON‑NEGOTIABLE project requirement to ensure:
>
> - International collaboration
> - Code maintainability
> - Consistent developer experience
> - AI tooling compatibility
>
> **No exceptions.**

---

## 2. Architecture & Core Rules (NON‑NEGOTIABLE)

### 2.1 Strict MVVM Separation

#### ViewModels

- MUST inherit from `ObservableObject` (`CommunityToolkit.Mvvm 8.3.2`)
- MUST NOT reference any UI types (`Page`, `DispatcherQueue`, `XamlRoot`, etc.)
- MUST expose state only via observable properties
- MUST use `[ObservableProperty]` and `[RelayCommand]`
- MUST NOT manually implement `INotifyPropertyChanged`
- Use **primary constructors** for DI (see [CatalogViewModel.cs](../src/PowerManager.UI/ViewModels/CatalogViewModel.cs))
- Declare as `partial class` to support MVVM Toolkit source generators

#### Views

- XAML pages only
- Code‑behind limited to:
  - `InitializeComponent()`
  - UI‑only event handlers (e.g., button clicks that forward to ViewModel commands)
  - ViewModel retrieval via DI (`((App)App.Current).Services.GetRequiredService<T>()`)
- MUST NOT contain business logic
- Use `x:Bind` (compile-time) instead of `Binding` (runtime) where possible

#### Models

- Plain C# classes in `Models/`
- No framework or UI dependencies
- Parameterless constructors + public setters (required for YAML/JSON)

#### Services

- Business logic layer only
- **EVERY service MUST expose an interface**
  - Example: `IWingetService`, `ICatalogService`, `IQueueService`

- No UI dependencies
- All services injected via DI

---

## 3. Threading, Async & UI Marshaling

### 3.1 Async Rules

- ALL asynchronous operations MUST use `async/await`
- FORBIDDEN:
  - `.Wait()`
  - `.Result`
  - Blocking calls on async methods

### 3.2 UI Thread Marshaling

- Services run on background threads
- ViewModels are responsible for updating UI‑bound properties
- UI updates MUST be marshaled using `IUiDispatcher` abstraction

```csharp
// In services or ViewModels
_uiDispatcher.TryEnqueue(() =>
{
    SomeObservableProperty = newValue;
});
```

- `UiDispatcher` implementation: [UiDispatcher.cs](../src/PowerManager.UI/Services/UiDispatcher.cs)
- Automatically uses `DispatcherQueue.GetForCurrentThread()` if not explicitly initialized

### 3.3 Dispatcher Ownership Rule

- Services SHOULD remain UI‑agnostic
- Preferred pattern:
  - Services return results
  - ViewModels perform UI marshaling via `IUiDispatcher`
- If UI dispatching is required in a service, inject `IUiDispatcher` (registered as Singleton)

---

## 4. Dependency Injection

- DI configured in `App.xaml.cs`
- Framework: `Microsoft.Extensions.DependencyInjection`

### 4.1 Registration Rules

- Services: Singleton or Scoped (explicitly chosen)
- ViewModels: **Transient by default**
- Global state ViewModels MAY be Singletons (exception only)

### 4.2 Constructor Injection (MANDATORY)

```csharp
public CatalogViewModel(
    ICatalogService catalogService,
    IWingetService wingetService)
{
    _catalogService = catalogService;
    _wingetService = wingetService;
}
```

- Property injection is NOT allowed
- Service locator allowed ONLY for navigation scenarios

---

## 5. Logging

- Framework: `Microsoft.Extensions.Logging`
- Inject `ILogger<T>` into services and ViewModels
- **MANDATORY**: Use `[LoggerMessage]` attribute for compile-time logging (see examples below)

### 5.1 Log Levels

- `LogInformation`: normal operations
- `LogWarning`: recoverable issues
- `LogError`: failures and exceptions

### 5.2 Logging Pattern (REQUIRED)

- Classes with logging MUST be `partial`
- Use `[LoggerMessage]` attribute for all log statements:

```csharp
[LoggerMessage(LogLevel.Information, "Loading installed packages")]
private static partial void LogLoadingPackages(ILogger logger);

[LoggerMessage(LogLevel.Error, "Failed to load packages")]
private static partial void LogLoadPackagesFailed(ILogger logger, Exception exception);
```

- Examples: [CatalogViewModel.cs](../src/PowerManager.UI/ViewModels/CatalogViewModel.cs), [WingetService.cs](../src/PowerManager.Core/Services/Implementations/WingetService.cs)

### 5.3 Logging Rules

- ALL logs MUST be in **English**
- Log full exception stack traces
- No UI dependencies in logging
- Current target: Visual Studio Debug Output
- Logging architecture MUST allow future file logging without refactor

---

## 6. Error Handling & UX

- ALL service methods MUST be wrapped in `try/catch`
- Errors MUST:
  - Be logged
  - Be propagated in a controlled manner

### 6.1 User‑Facing Errors

- Stored in ViewModel observable properties

- Displayed via:
  - `InfoBar`
  - `ContentDialog`

- Messages MUST be clear, concise, and in English

---

## 7. Data Persistence

### 7.1 File Locations

All data stored in:

```
%LocalAppData%\PowerManager Software\
```

| File                   | Purpose                                   |
| ---------------------- | ----------------------------------------- |
| `catalog_default.yaml` | Auto‑generated default catalog (40+ apps) |
| `catalog_custom.yaml`  | User‑defined apps (persistent)            |
| `status_cache.json`    | Install status cache (TTL: 5 minutes)     |
| `history.json`         | Operation history                         |

**Note**: File persistence not yet implemented in current codebase (v0.1.0)

---

## 8. YAML & JSON Serialization

- YAML: `YamlDotNet 16.2.1`
- JSON: `System.Text.Json`

### Serialization Rules

- Models MUST have:
  - Parameterless constructor
  - Public setters

- No custom converters unless strictly required

---

## 9. WinGet Integration

### 9.1 Process Execution Pattern

```csharp
var processInfo = new ProcessStartInfo
{
    FileName = "winget",
    Arguments = "install --id AppId --silent --accept-package-agreements --accept-source-agreements",
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
    CreateNoWindow = true
};
```

- Output MUST be read asynchronously
- Parsing is text‑based (no structured API exists)

### 9.2 Supported Commands

- `winget list --id <AppId>`
- `winget install --id <AppId>`
- `winget uninstall --id <AppId>`
- `winget search "<query>"`
- `winget upgrade --id <AppId>`

---

## 10. Execution Queue (CRITICAL)

### 10.1 Queue Model

```csharp
enum QueueItemStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Canceled
}
```

Each `QueueItem` contains:

- Status
- Progress
- Logs (StringBuilder)
- CancellationTokenSource (runtime only)

### 10.2 Current Implementation

- Sequential execution (1 task at a time) using `SemaphoreSlim(1, 1)`
- Queue processing is fire-and-forget (`_ = ProcessQueueAsync()`)
- Events: `ItemAdded`, `ItemStatusChanged`
- See [QueueService.cs](../src/PowerManager.Core/Services/Implementations/QueueService.cs)

### 10.3 Error Policy (IMPORTANT)

- FAILED installations MUST NOT be retried automatically
- On failure:
  - Status = `Failed`
  - Error logged and stored in `QueueItem.Logs`
  - User may manually retry

### 10.4 Future Enhancement

- Parallel execution limit (configurable)
  - Will use bounded task queue or additional SemaphoreSlim permits

---

## 11. UI & XAML Conventions

### Binding

- Prefer `x:Bind` over `Binding`
- Use `TwoWay` ONLY when needed
- Collections MUST be `ObservableCollection<T>`

### Navigation

- `NavigationView` based layout
- Current pages:
  - `CatalogPage` - Browse and select packages
  - `QueuePage` - View execution queue

- Planned:
  - `SplashPage` - WinGet verification at startup
  - `HistoryPage` - Operation logs

---

## 12. Startup Flow

1. App entry point: `Program.cs` (custom WinUI 3 entry with `DISABLE_XAML_GENERATED_MAIN`)
2. DI container configured in `App.xaml.cs` constructor
3. Window activated via `MainWindow`
4. Current: Direct navigation to main UI
5. Planned: `SplashPage` → verify WinGet → load catalogs → navigate to `CatalogPage`

---

## 13. Testing (MANDATORY STRUCTURE)

- Testing framework: **MSTest 4.0.1** (NOT xUnit)
- Mocking: `Moq` or `NSubstitute` (not yet implemented)

### Testing Scope

- Services
- ViewModels
- Execution Queue logic

### Folder Convention

```
src/PowerManager.Tests/
  ├── PowerManager.Tests.csproj (MSTest 4.0.1)
  └── MSTestSettings.cs
```

UI tests are OUT OF SCOPE initially

**Note**: Test implementations not yet present in v0.1.0

---

## 14. Build & Development

- Visual Studio 2022 17.8+ or VS 2026
- Required workloads:
  - .NET Desktop Development
  - UWP / Windows App SDK

- .NET SDK: **10** (current target) or 8 (minimum)

---

## 15. Code Style & Language Rules

- `<Nullable>enable</Nullable>` REQUIRED
- English everywhere (code, comments, logs, UI)
- `Async` suffix for async methods
- Use `var` when obvious
- Avoid magic strings and numbers

---

## 16. Versioning & Commits

- Semantic Versioning
- Conventional Commits:
  - `feat:`
  - `fix:`
  - `docs:`
  - `chore:`

Current version: **0.1.0**

---

## FINAL RULE

> If an implementation choice is ambiguous, prioritize:
> **stability → testability → clarity → performance**
