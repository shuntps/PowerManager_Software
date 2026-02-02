# PowerManager Software - Windows Application Manager via WinGet

![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet)
![WinUI 3](https://img.shields.io/badge/WinUI-3-0078D4?logo=windows)
![License](https://img.shields.io/badge/license-MIT-green)

Modern Windows application (WinUI 3) to install, uninstall, and update software in bulk via WinGet.

## 🚀 Features

- 🎯 **Smart Splash Screen** - Loading with WinGet verification at startup
- 📦 **Application Catalog** - 40+ popular pre-configured applications
- ⚡ **Bulk Installation** - Select multiple apps and install them with one click
- 🗑️ **Bulk Uninstallation** - Manage your installed apps easily
- 🔄 **Automatic Updates** - Detection and installation of available updates
- 📊 **Execution Queue** - Real-time tracking of operation progress
- 📜 **Complete History** - Detailed logs of all operations
- 🔍 **Search and Filters** - Quickly find applications by name or category
- ⚙️ **Status Cache** - Fast detection of installed apps without rerunning WinGet

## 📋 Prerequisites

### For Execution

- **Windows 10/11** (version 1809 minimum)
- **WinGet** installed (included by default on Windows 11)
- **Microsoft Edge WebView2 Runtime**

### For Development

- **Visual Studio 2026** (or Visual Studio 2022 17.8+) with:
  - "Universal Windows Platform development" workload
  - ".NET Desktop Development" workload
  - Windows App SDK 1.8+
- **.NET 10 SDK** (recommended) or **.NET 8 SDK** (minimum)

## 🛠️ Installation

### 📥 From Installer (Recommended)

**Inno Setup Installer** (Windows 7+)

1. Download `PowerManagerSetup-x.x.x-x64.exe` from [Releases](https://github.com/shuntps/PowerManager-Software/releases)
2. Run the installer
3. Follow the setup wizard
4. The app will be available in your Start Menu

**MSIX Package** (Windows 10 1809+)

1. Download `.msixbundle` from [Releases](https://github.com/shuntps/PowerManager-Software/releases)
2. Install the certificate (see [MSIX_QUICKSTART.md](MSIX_QUICKSTART.md))
3. Double-click to install
4. Find it in Windows Apps

> 💡 **See [installer/README.md](installer/README.md) for complete installer documentation**
> 📦 **Inno Setup Guide**: [installer/QUICKSTART.md](installer/QUICKSTART.md)
> 📦 **MSIX Guide**: [installer/msix/README.md](installer/msix/README.md)

### 🔨 From Source

```powershell
# Clone the repository
git clone https://github.com/shuntps/PowerManager-Software.git
cd PowerManager-Software/src/PowerManager_Software

# Restore packages
dotnet restore

# Build (defaults to x64 platform)
dotnet build -c Release

# Run
dotnet run
```

**Note**: The project targets `net8.0-windows10.0.19041.0` (compatible with .NET 8, 9, and 10) and defaults to x64 platform.

## 📖 Usage

### 0. First Launch

- The application automatically checks for WinGet presence
- If WinGet is not installed, a message guides you to installation
- The catalog is loaded and the installation status of apps is detected

### 1. Catalog Page

- Browse the 40+ available applications
- Use the search bar or category filters
- Check the applications you want to install
- Click **"Install Selection"**

### 2. Execution Queue

- Follow the progress of each installation in real-time
- View detailed logs
- Cancel an operation if necessary

### 3. History

- View the complete history of all operations
- Export logs to text file
- Analyze installation failures

## 🏗️ Architecture

```
PowerManager_Software/
├── Models/          # Data models
│   ├── AppInfo.cs           # Application representation
│   ├── Operation.cs         # WinGet operation (Install/Uninstall/Update)
│   ├── OperationType.cs     # Operation type enum (Install/Uninstall/Update)
│   ├── OperationStatus.cs   # Operation status (Pending/Running/Completed/Failed)
│   └── HistoryEntry.cs      # History entry
├── Services/        # Business services
│   ├── WingetService.cs         # WinGet command execution
│   ├── CatalogService.cs        # Application catalog management (default + custom)
│   ├── QueueService.cs          # Sequential operation queue execution
│   ├── HistoryService.cs        # Operation history management
│   └── AppStatusCacheService.cs # Installation status cache (5-min TTL)
├── ViewModels/      # MVVM ViewModels (CommunityToolkit.Mvvm)
│   ├── MainViewModel.cs     # Main window ViewModel
│   ├── SplashViewModel.cs   # Splash screen ViewModel
│   ├── CatalogViewModel.cs  # Catalog ViewModel with filtering
│   ├── QueueViewModel.cs    # Execution queue ViewModel
│   └── HistoryViewModel.cs  # History ViewModel
├── Views/           # WinUI 3 Pages (XAML)
│   ├── MainWindow.xaml      # Main window with NavigationView
│   ├── SplashPage.xaml      # Loading page with WinGet verification
│   ├── CatalogPage.xaml     # Application catalog with search/filter
│   ├── QueuePage.xaml       # Execution queue with real-time progress
│   └── HistoryPage.xaml     # History page with export
├── Converters/      # XAML Value Converters
│   └── ValueConverters.cs   # BoolToVisibility, InverseBool, etc.
└── Assets/          # Resources (icons, images)
```

### Technologies Used

- **Framework**: .NET 8+ (C# 12) - Compatible with .NET 8, 9, and 10
- **UI**: WinUI 3 (Windows App SDK 1.8)
- **Pattern**: Strict MVVM
- **MVVM Toolkit**: CommunityToolkit.Mvvm 8.3.2
- **Logging**: Microsoft.Extensions.Logging
- **DI**: Microsoft.Extensions.DependencyInjection
- **YAML**: YamlDotNet 16.2.1

### Design Principles

- ✅ **Strict MVVM Pattern** - Complete UI/Logic separation
- ✅ **Dependency Injection** - All services are injected
- ✅ **Async/Await Everywhere** - No blocking operations on the UI thread
- ✅ **DispatcherQueue Marshaling** - Safe threading management for UI
- ✅ **Comprehensive Logging** - All important operations are logged
- ✅ **Robust Error Handling** - Try-catch with clear messages in English
- ✅ **Nullable Enabled** - Null reference safety with C# 12

## ⚙️ Configuration

### Adding Applications to the Catalog

The application uses a **dual catalog system**:

#### Default Catalog (Built-in Apps)

Edit `Services/CatalogService.cs` in the `CreateDefaultCatalog()` method:

```csharp
private static List<AppInfo> CreateDefaultCatalog()
{
    return
    [
        new AppInfo
        {
            Id = "YourApp.YourApp",        // WinGet ID (required)
            Name = "Your Application",      // Display name (required)
            Category = "Category",          // Category for filtering (required)
            Description = "Description",    // Description (optional)
            Tags = ["tag1", "tag2"]        // Tags for search (optional)
        },
        // ... more apps
    ];
}
```

**Note**: The default catalog is auto-generated and saved to `%LocalAppData%\PowerManager Software\catalog_default.yaml` on every launch.

#### Custom Catalog (User Additions)

For user-specific apps, the application creates `%LocalAppData%\PowerManager Software\catalog_custom.yaml` which persists across updates. Both catalogs merge at runtime.

To find the WinGet ID of an application:

```powershell
winget search "app name"
```

## � Building Installers

PowerManager Software provides two installer formats:

### Inno Setup (Recommended)

Create a traditional Windows .exe installer:

```powershell
# Install Inno Setup
winget install JRSoftware.InnoSetup

# Build the installer
.\installer\scripts\build-inno.ps1
```

Output: `output/inno/PowerManagerSetup-x.x.x-x64.exe`

### MSIX Package

Create a Microsoft Store compatible package:

```powershell
# Build MSIX (requires Visual Studio 2026)
.\installer\scripts\build-msix.ps1
```

Output: `output/msix/PowerManager_Software_x.x.x.x_x64.msixbundle`

**Complete guides**:

- 📖 [Main Installer Documentation](installer/README.md)
- ⚡ [Quick Start](installer/QUICKSTART.md)
- 🔧 [Environment Setup](installer/SETUP_ENVIRONMENT.md)
- 📦 [Inno Setup Guide](installer/inno/README.md)
- 📦 [MSIX Guide](installer/msix/README.md)

## �🔧 Troubleshooting

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

Application data is stored in `%LocalAppData%\PowerManager Software\`:

```
catalog_default.yaml  # Default catalog (auto-generated on startup)
catalog_custom.yaml   # Custom user additions (preserved)
status_cache.json     # Installation status cache (5-min TTL)
history.json          # Operation history
```

**Note**: Debug logs are currently output to Visual Studio Debug Output window, not files.

## ❓ FAQ

**Q: Can I use PowerManager Software without an Internet connection?**
A: No, WinGet requires an Internet connection to download applications. However, browsing the catalog works offline.

**Q: Do installations require administrator rights?**
A: Some applications require it. PowerManager Software will ask for privilege elevation if necessary.

**Q: Can I cancel an ongoing installation?**
A: Yes, each operation in the execution queue can be cancelled via the "Cancel" button.

**Q: Are logs automatically cleaned up?**
A: Logs are currently only available in Visual Studio Debug Output during development. No persistent log files are created.

**Q: Can I add my own applications to the catalog?**
A: Yes! For built-in apps, edit `Services/CatalogService.cs` in the `CreateDefaultCatalog()` method. The app also supports `catalog_custom.yaml` for user-specific additions that persist across updates.

**Q: How does the status cache work?**
A: The app caches installation status for 5 minutes to avoid repeated WinGet calls. Click "Refresh Status" to force a re-check.

## 🗺️ Roadmap

### Version 1.1

- [ ] Predefined application packs (Dev Pack, Office Pack, Gaming Pack)
- [ ] Import/Export configurations (.pmconfig files)
- [ ] Automatic dark/light mode

### Version 1.2

- [ ] Multi-language support (EN, FR, ES, DE)
- [ ] System notifications for completed operations
- [ ] Custom icons for applications

### Version 2.0

- [ ] Automatic PowerManager Software updates
- [ ] Automatic detection of new app versions
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

Check [GitHub Issues](https://github.com/shuntps/PowerManager-Software/issues) for current bugs and planned work.

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

## 🙏 Acknowledgments

- [WinGet](https://github.com/microsoft/winget-cli) - Microsoft Package Manager
- [Windows App SDK](https://github.com/microsoft/WindowsAppSDK) - WinUI 3 Framework
- [CommunityToolkit](https://github.com/CommunityToolkit) - MVVM Toolkit

---

**Made with ❤️ and .NET by [Shunt](https://github.com/shuntps)**
