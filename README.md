# PowerManager Software - Windows Application Manager via WinGet

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![WinUI 3](https://img.shields.io/badge/WinUI-3-0078D4?logo=windows)
![License](https://img.shields.io/badge/license-MIT-green)

Modern Windows application (WinUI 3) to install, uninstall, and update software in bulk via WinGet.

## 🚀 Features

### ✅ Implemented (v0.1.0)

- 📦 **Basic Package Management** - View installed packages via WinGet
- 📊 **Execution Queue** - Sequential task execution with status tracking
- ⚡ **Install/Uninstall Operations** - Queue-based package management
- 📝 **Comprehensive Logging** - Detailed operation logs with [LoggerMessage] attributes
- 🔄 **Async Operations** - Non-blocking UI with proper threading

### 🚧 Planned Features

- 🎯 **Smart Splash Screen** - Loading with WinGet verification at startup (planned)
- 📦 **Application Catalog** - Pre-configured application catalog with 40+ popular apps (planned)
- 🗑️ **Bulk Operations** - Multi-select installation/uninstallation (planned)
- 🔄 **Automatic Updates** - Detection and installation of available updates (planned)
- 📜 **Complete History** - Persistent operation history (planned)
- 🔍 **Search and Filters** - Find applications by name or category (planned)
- ⚙️ **Status Cache** - Fast detection of installed apps without rerunning WinGet (planned)

## 📋 Prerequisites

### For Execution

- **Windows 10/11** (version 1809 minimum)
- **WinGet** installed (included by default on Windows 11)
- **Microsoft Edge WebView2 Runtime**

### For Development

- **Visual Studio 2026** (or Visual Studio 2022 17.8+) with:
  - "Universal Windows Platform development" workload
  - ".NET Desktop Development" workload
  - Windows App SDK 1.8.260101001
- **.NET 10 SDK** (required)

## 🛠️ Installation

### � From Source (Only Option for v0.1.0)

```powershell
# Clone the repository
git clone https://github.com/shuntps/PowerManager_Software.git
cd PowerManager_Software/src

# Restore packages
dotnet restore

# Build (defaults to x64 platform)
dotnet build -c Release

# Run the UI project
cd PowerManager.UI
dotnet run
```

**Note**: The project targets `net10.0-windows10.0.19041.0` (Windows App SDK 1.8.260101001) and defaults to x64 platform.

## 📖 Usage

### Current Implementation (v0.1.0)

### 1. Catalog Page

- View packages (basic implementation)
- Select packages for installation
- Queue operations for execution

### 2. Queue Page

- Monitor queued operations
- View operation status (Pending, Running, Completed, Failed, Canceled)
- Track sequential execution progress
- Cancel pending operations

### Planned Features

- **Splash Screen**: WinGet verification at startup
- **Full Catalog**: 40+ pre-configured applications with search/filter
- **History Page**: Persistent operation logs with export functionality
- **Status Cache**: Fast installation detection with TTL

## 🏗️ Architecture

```
PowerManager/
├── PowerManager.Core/          # Core business logic layer
│   ├── Models/                 # Data models
│   │   ├── Package.cs          # Package representation
│   │   └── QueueItem.cs        # Queue item with status and logs
│   ├── Enums/                  # Enumerations
│   │   └── QueueItemStatus.cs  # Queue status (Pending/Running/Completed/Failed/Canceled)
│   └── Services/               # Business services
│       ├── IWingetService.cs           # WinGet command interface
│       ├── IQueueService.cs            # Queue management interface
│       ├── IUiDispatcher.cs            # UI thread marshaling interface
│       └── Implementations/
│           ├── WingetService.cs        # WinGet process execution
│           └── QueueService.cs         # Sequential queue processing
├── PowerManager.UI/            # WinUI 3 presentation layer
│   ├── ViewModels/             # MVVM ViewModels (CommunityToolkit.Mvvm)
│   │   ├── CatalogViewModel.cs # Catalog management
│   │   ├── QueueViewModel.cs   # Queue monitoring
│   │   └── MainViewModel.cs    # Main window coordination
│   ├── Views/                  # WinUI 3 Pages (XAML)
│   │   ├── CatalogPage.xaml    # Package catalog
│   │   └── QueuePage.xaml      # Execution queue
│   ├── Services/
│   │   └── UiDispatcher.cs     # UI thread marshaling implementation
│   ├── App.xaml.cs             # DI container configuration
│   ├── MainWindow.xaml         # Main window with NavigationView
│   └── Program.cs              # Custom entry point (DISABLE_XAML_GENERATED_MAIN)
└── PowerManager.Tests/         # MSTest test project
    ├── MSTestSettings.cs       # Test configuration
    └── PowerManager.Tests.csproj # MSTest 4.0.1 project
```

### Technologies Used

- **Framework**: .NET 10 (C# 13)
- **UI**: WinUI 3 (Windows App SDK 1.8.260101001)
- **Pattern**: Strict MVVM with source generators
- **MVVM Toolkit**: CommunityToolkit.Mvvm 8.3.2
- **Testing**: MSTest 4.0.1 (NOT xUnit)
- **Logging**: Microsoft.Extensions.Logging with [LoggerMessage] attributes
- **DI**: Microsoft.Extensions.DependencyInjection
- **YAML**: YamlDotNet 16.2.1 (planned for catalog persistence)

### Design Principles

- ✅ **Strict MVVM Pattern** - Complete UI/Logic separation
- ✅ **Dependency Injection** - All services are injected
- ✅ **Async/Await Everywhere** - No blocking operations on the UI thread
- ✅ **DispatcherQueue Marshaling** - Safe threading management for UI
- ✅ **Comprehensive Logging** - All important operations are logged
- ✅ **Robust Error Handling** - Try-catch with clear messages in English
- ✅ **Nullable Enabled** - Null reference safety with C# 12

## ⚙️ Configuration

### Current Implementation (v0.1.0)

The application currently operates directly with WinGet commands. Configuration features are planned for future releases.

### Planned: Dual Catalog System

#### Default Catalog (Built-in Apps)

Will contain 40+ pre-configured popular applications, auto-generated and saved to:

```
%LocalAppData%\PowerManager Software\catalog_default.yaml
```

#### Custom Catalog (User Additions)

User-specific applications will persist in:

```
%LocalAppData%\PowerManager Software\catalog_custom.yaml
```

Both catalogs will merge at runtime for a unified experience.

To find the WinGet ID of an application:

```powershell
winget search "app name"
```

## 🔧 Troubleshooting

### WinGet Not Found

```powershell
# Check if WinGet is installed
winget --version

# Install WinGet if necessary
# Download from https://github.com/microsoft/winget-cli/releases
```

### Permission Issues

- Some applications require administrator rights
- Run PowerManager Software as administrator if necessary

### Data Files

Application data will be stored in `%LocalAppData%\PowerManager Software\` (not yet implemented in v0.1.0):

```
catalog_default.yaml  # Default catalog (planned)
catalog_custom.yaml   # Custom user additions (planned)
status_cache.json     # Installation status cache with 5-min TTL (planned)
history.json          # Operation history (planned)
```

**Current Logging**: All logs are output to Visual Studio Debug Output window using `[LoggerMessage]` attributes. File logging infrastructure is designed to be added without refactoring.

## ❓ FAQ

**Q: Can I use PowerManager Software without an Internet connection?**
A: No, WinGet requires an Internet connection to download applications. However, browsing the catalog works offline.

**Q: Do installations require administrator rights?**
A: Some applications require it. PowerManager Software will ask for privilege elevation if necessary.

**Q: Can I cancel an ongoing installation?**
A: Yes, each operation in the execution queue can be cancelled via the "Cancel" button.

**Q: Are logs automatically cleaned up?**
A: Currently (v0.1.0), logs are only available in Visual Studio Debug Output during development. Persistent logging is planned for future releases.

**Q: Can I add my own applications to the catalog?**
A: Catalog functionality is planned but not yet implemented in v0.1.0. The app currently works directly with WinGet commands.

**Q: How does the status cache work?**
A: Status caching is planned for future releases to avoid repeated WinGet calls and improve performance.

## 🗺️ Roadmap

### Version 0.2 (Next)

- [ ] Splash screen with WinGet verification
- [ ] Application catalog (40+ pre-configured apps)
- [ ] Catalog persistence (YAML-based)
- [ ] Status cache with TTL
- [ ] History page with persistent logs
- [ ] Search and filter functionality

### Version 1.0

- [ ] Bulk operations (multi-select)
- [ ] Automatic update detection
- [ ] File-based logging
- [ ] Comprehensive error handling UI
- [ ] Full test coverage (MSTest)

### Version 1.1

- [ ] Predefined application packs (Dev Pack, Office Pack, Gaming Pack)
- [ ] Import/Export configurations (.pmconfig files)
- [ ] Automatic dark/light mode
- [ ] Parallel execution (configurable limit)

### Version 1.2

- [ ] Multi-language support (EN, FR, ES, DE)
- [ ] System notifications for completed operations
- [ ] Custom icons for applications

### Version 2.0

- [ ] Automatic PowerManager Software updates
- [ ] Scheduled installations/updates
- [ ] Usage statistics

## 🤝 Contributing

**Contributions are highly welcome!** This project is in active development and needs help.

### Ways to Contribute

- 🐛 **Report bugs** - Open an issue with detailed reproduction steps
- 💡 **Suggest features** - Share your ideas in GitHub Discussions
- 📝 **Improve docs** - Fix typos, add examples, translate content
- 🔧 **Submit PRs** - Fix bugs, implement features, refactor code

### Development Workflow

1. Fork the project
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Follow the coding guidelines in `.github/copilot-instructions.md`
4. Commit your changes (`git commit -m 'Add AmazingFeature'`)
5. Push to your branch (`git push origin feature/AmazingFeature`)
6. Open a Pull Request with a clear description

### Known Issues

Check [GitHub Issues](https://github.com/shuntps/PowerManager_Software/issues) for current bugs and planned work.

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

## 🙏 Acknowledgments

- [WinGet](https://github.com/microsoft/winget-cli) - Microsoft Package Manager
- [Windows App SDK](https://github.com/microsoft/WindowsAppSDK) - WinUI 3 Framework
- [CommunityToolkit](https://github.com/CommunityToolkit) - MVVM Toolkit

---

**Made with ❤️ and .NET by [Shunt](https://github.com/shuntps)**
