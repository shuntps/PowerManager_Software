# PowerManager Software - Windows Application Manager via WinGet

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![WinUI 3](https://img.shields.io/badge/WinUI-3-0078D4?logo=windows)
![Version](https://img.shields.io/github/v/release/shuntps/PowerManager_Software)
![Build](https://img.shields.io/github/actions/workflow/status/shuntps/PowerManager_Software/release.yml?branch=main&label=build)
![License](https://img.shields.io/badge/license-MIT-green)

Modern Windows application (WinUI 3) to install, uninstall, and update software in bulk via WinGet.

## 🚀 Features

### ✅ Implemented (v0.3.0)

#### Core Features (v0.1.0)

- 📦 **Basic Package Management** - View installed packages via WinGet
- 📊 **Execution Queue** - Sequential task execution with status tracking
- ⚡ **Install/Uninstall Operations** - Queue-based package management
- 📝 **Comprehensive Logging** - Detailed operation logs with [LoggerMessage] attributes
- 🔄 **Async Operations** - Non-blocking UI with proper threading

#### New in v0.3.0

- 📦 **Private YAML Catalog** - Persistent package catalog with 5 default apps
- 🔍 **Search & Filters** - Real-time search and category filtering
- 🎨 **Status Badges** - Visual indicators (Installed/Update Available/Not Installed)
- ⚡ **Optimized Scanning** - Targeted per-package scanning with progress overlay
- 💾 **Catalog Persistence** - Dual YAML system (default + custom catalogs)
- 📊 **Installation Detection** - Automatic status detection with LastChecked timestamp
- 🌐 **Source Detection** - Distinguish between winget and msstore packages
- 🎯 **Smart Parsing** - Robust WinGet output parsing for all package formats

### 🚧 Planned Features

- 🎯 **Smart Splash Screen** - Loading with WinGet verification at startup
- 📦 **Expanded Catalog** - Pre-configured catalog with 40+ popular apps
- 🗑️ **Bulk Operations** - Multi-select installation/uninstallation
- 🔄 **Automatic Updates** - One-click update all outdated packages
- 📜 **Complete History** - Persistent operation history with export

## 📋 Prerequisites

### For Execution

- **Windows 10/11** (version 1809 minimum)
- **WinGet** installed (included by default on Windows 11)
- **Microsoft Edge WebView2 Runtime**

### For Development

- **Visual Studio 2022** (17.12+ recommended) or **Visual Studio Code** with:
  - "Universal Windows Platform development" workload
  - ".NET Desktop Development" workload
  - Windows App SDK 1.8.260101001
- **.NET 10 SDK** (required)

## 🛠️ Installation

> **Note:** PowerManager is currently in active development (v0.3.0). Pre-built installers are not yet available. You must build from source.

### 📦 Build from Source

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

### Current Implementation (v0.3.0)

### 1. Catalog Page

- **Browse Packages** - View 5 default apps (Chrome, Discord, VS Code, 7zip, Notepad++)
- **Real-time Search** - Filter packages by name or ID
- **Category Filter** - Browse by category (All/Browsers/Development/Communication/Utilities)
- **Status Badges** - Visual indicators showing installation status:
  - 🟢 **Green Badge**: Installed (current version)
  - 🟠 **Orange Badge**: Update Available (shows new version)
  - ⚫ **Gray Badge**: Not Installed
- **Multi-select** - Choose multiple packages for batch operations
- **Auto-refresh** - Packages scanned on page load with progress overlay

### 2. Queue Page

- Monitor queued operations
- View operation status (Pending, Running, Completed, Failed, Canceled)
- Track sequential execution progress
- Cancel pending operations

### 3. Data Persistence

Application data is stored in `%LocalAppData%\PowerManager Software\`:

- **catalog_default.yaml** - Built-in package definitions (auto-generated)
- **catalog_custom.yaml** - User-added packages (planned)
- **LastChecked** - Each package tracks last scan timestamp

### Planned Features

- **Splash Screen**: WinGet verification at startup
- **Expanded Catalog**: 40+ pre-configured applications
- **History Page**: Persistent operation logs with export functionality

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
- **YAML**: YamlDotNet 16.2.1 (for catalog persistence)

### Design Principles

- ✅ **Strict MVVM Pattern** - Complete UI/Logic separation
- ✅ **Dependency Injection** - All services are injected
- ✅ **Async/Await Everywhere** - No blocking operations on the UI thread
- ✅ **DispatcherQueue Marshaling** - Safe threading management for UI
- ✅ **Comprehensive Logging** - All important operations are logged
- ✅ **Robust Error Handling** - Try-catch with clear messages in English
- ✅ **Nullable Enabled** - Null reference safety with C# 12

## ⚙️ Configuration

### Dual Catalog System (v0.3.0)

PowerManager uses a dual YAML catalog system for flexibility:

#### Default Catalog (Built-in Apps)

Contains 5 pre-configured popular applications, auto-generated on first launch and saved to:

```
%LocalAppData%\PowerManager Software\catalog_default.yaml
```

**Default packages:**

- Google Chrome (Google.Chrome)
- Discord (Discord.Discord)
- Visual Studio Code (Microsoft.VisualStudioCode)
- 7-Zip (7zip.7zip)
- Notepad++ (Notepad++.Notepad++)

#### Custom Catalog (User Additions) - Planned

User-specific applications will persist in:

```
%LocalAppData%\PowerManager Software\catalog_custom.yaml
```

Both catalogs merge at runtime for a unified experience.

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

Application data is stored in `%LocalAppData%\PowerManager Software\`:

```
catalog_default.yaml  # ✅ Default catalog with 5 apps (implemented v0.3.0)
catalog_custom.yaml   # 🚧 Custom user additions (planned v0.4.0)
```

**File Structure (catalog_default.yaml):**

```yaml
Packages:
  - Id: Google.Chrome
    Name: Google Chrome
    Source: winget
    Category: Browsers
    Description: Fast and secure web browser
    Tags: [browser, popular, google]
    IsInstalled: true
    InstalledVersion: 144.0.7559.110
    AvailableVersion: 144.0.7559.110
    UpdateAvailable: false
    LastChecked: 2026-02-03T12:00:00Z
```

**Logging:** All logs are output to Visual Studio Debug Output window using `[LoggerMessage]` attributes. File-based logging is planned for v1.0.0.

## ❓ FAQ

**Q: Can I use PowerManager Software without an Internet connection?**
A: No, WinGet requires an Internet connection to download applications. However, browsing the catalog works offline.

**Q: Do installations require administrator rights?**
A: Some applications require it. PowerManager Software will ask for privilege elevation if necessary.

**Q: Can I cancel an ongoing installation?**
A: Yes, each operation in the execution queue can be cancelled via the "Cancel" button.

**Q: Are logs automatically cleaned up?**
A: Currently (v0.3.0), logs are only available in Visual Studio Debug Output during development. Persistent file logging is planned for future releases.

**Q: Can I add my own applications to the catalog?**
A: Custom catalog functionality is planned but not yet implemented. Currently, the app manages 5 default packages. You can manually edit `catalog_default.yaml` to add more.

**Q: How does the status detection work?**
A: Each package is scanned via WinGet on catalog page load. Results are cached in the YAML catalog with a `LastChecked` timestamp. The scan runs in the background with a progress overlay showing real-time status. v0.4.0 will add smart TTL-based caching to minimize WinGet calls.

**Q: What package sources are supported?**
A: v0.3.0 detects both `winget` (default) and `msstore` (Microsoft Store) sources automatically from WinGet output.

## � Versioning & Releases

PowerManager follows [Semantic Versioning](https://semver.org/) with automated releases.

### Version Format

**MAJOR.MINOR.PATCH** (e.g., `1.2.3`)

- **MAJOR**: Breaking changes (incompatible API changes)
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Automated Releases

Every commit to `main` is analyzed for version bumping:

| Commit Type                    | Version Bump | Example           |
| ------------------------------ | ------------ | ----------------- |
| `feat:`                        | MINOR        | `0.1.0` → `0.2.0` |
| `fix:`                         | PATCH        | `0.1.0` → `0.1.1` |
| `perf:`                        | PATCH        | `0.1.0` → `0.1.1` |
| `feat!:` or `BREAKING CHANGE:` | MAJOR        | `0.1.0` → `1.0.0` |
| `docs:`, `chore:`, `style:`    | No release   | No change         |

**GitHub Actions automatically:**

- ✅ Calculates the next version
- ✅ Updates `VERSION.txt` and `CHANGELOG.md`
- ✅ Builds and tests the project
- ✅ Creates a GitHub Release with binaries
- ✅ Publishes release notes

See [CHANGELOG.md](CHANGELOG.md) for complete version history.

### Release Assets

**Current releases (v0.3.0) include:**

- Source code (zip/tar.gz)
- Full changelog and release notes

**Planned for future releases:**

- Pre-compiled binaries (x64, self-contained)
- MSIX installer packages
- Microsoft Store listing (v1.0+)

## 🗺️ Roadmap

See our detailed [ROADMAP.md](ROADMAP.md) for planned features and release timeline.

**Next Release (v0.4.0):**

- Splash screen with WinGet verification
- Expand catalog to 40+ popular apps
- Custom catalog management UI
- Smart caching with TTL
- History page with persistent logs

**Long-term Vision:** See the [full roadmap](ROADMAP.md) for v1.0+ features including bulk operations, automation, multi-language support, and more.

## 🤝 Contributing

**Contributions are highly welcome!** This project is in active development and needs help.

### Ways to Contribute

- 🐛 **Report bugs** - Open an issue with detailed reproduction steps
- 💡 **Suggest features** - Share your ideas in GitHub Discussions
- 📝 **Improve docs** - Fix typos, add examples, translate content
- 🔧 **Submit PRs** - Fix bugs, implement features, refactor code

### Commit Message Format

This project uses [Conventional Commits](https://www.conventionalcommits.org/) for automated versioning:

```bash
<type>(<scope>): <description>

# Examples
feat(catalog): add package search functionality
fix(queue): prevent duplicate items in queue
docs: update installation instructions
chore: upgrade dependencies
```

**Valid types:**

- `feat`: New feature (triggers MINOR release)
- `fix`: Bug fix (triggers PATCH release)
- `perf`: Performance improvement (triggers PATCH release)
- `docs`: Documentation only (no release)
- `style`: Code formatting (no release)
- `refactor`: Code restructuring (no release)
- `test`: Adding tests (no release)
- `chore`: Maintenance tasks (no release)
- `ci`: CI/CD changes (no release)

**Breaking changes** (triggers MAJOR release):

```bash
feat!: redesign plugin system

BREAKING CHANGE: Plugin API has been completely redesigned
```

See [docs/SEMANTIC_RELEASE_GUIDE.md](docs/SEMANTIC_RELEASE_GUIDE.md) for complete documentation.

### Development Workflow

1. Fork the project
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Follow the coding guidelines in `.github/copilot-instructions.md`
4. **Use conventional commits** for all changes
5. Commit your changes (`git commit -m 'feat: add amazing feature'`)
6. Push to your branch (`git push origin feature/AmazingFeature`)
7. Open a Pull Request with a clear description

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
