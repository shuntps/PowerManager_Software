# PowerManager Software - Product Roadmap

This document outlines the planned features and improvements for PowerManager Software. Our goal is to build the most user-friendly WinGet package manager for Windows.

**Current Version:** v0.3.0  
**Last Updated:** February 2026

---

## ?? Vision

PowerManager aims to simplify Windows software management by providing:
- **Intuitive UI** for WinGet package management
- **Bulk operations** for efficient multi-package workflows
- **Smart automation** for updates and installations
- **Curated catalogs** of popular, trusted applications

---

## ? Completed Milestones

### v0.1.0 - Foundation (Released)
- ? Basic package management via WinGet
- ? Execution queue with status tracking
- ? Install/Uninstall operations
- ? Comprehensive logging with [LoggerMessage]
- ? Async/await architecture
- ? MVVM pattern with source generators

### v0.3.0 - Private Catalog (Current)
- ? Private YAML catalog with 5 default apps
- ? Real-time search and category filtering
- ? Status badges (Installed/Update Available/Not Installed)
- ? Optimized per-package scanning with progress overlay
- ? Dual catalog system (default + custom YAML)
- ? Installation detection with LastChecked timestamp
- ? Source detection (winget/msstore)
- ? Robust WinGet output parsing

---

## ?? Upcoming Releases

### v0.4.0 - Enhanced Catalog (Next - Q1 2025)

**Focus:** Expand catalog and improve user experience

**Success Metrics:**
- ?? Catalog contains 40+ curated applications
- ? Catalog load time <500ms with caching
- ?? User can add/remove custom packages via UI
- ?? "Update All" reduces update time by 70%

#### Features
- [ ] **Smart Splash Screen**
  - WinGet verification on startup
  - Installation check and auto-repair
  - Progress indicator during initialization

- [ ] **Expanded Catalog (40+ Apps)**
  - Popular browsers (Chrome, Firefox, Edge, Brave, Opera)
  - Development tools (Git, Node.js, Python, Docker, VS Code, Visual Studio)
  - Communication apps (Discord, Slack, Teams, Zoom, Telegram)
  - Utilities (7zip, WinRAR, VLC, Notepad++, Paint.NET)
  - Productivity (Office 365, LibreOffice, Adobe Reader, Obsidian)

- [ ] **Custom Catalog Management UI**
  - Add packages via search
  - Remove packages from catalog
  - Edit package metadata (name, description, category)
  - Import/Export catalog YAML

- [ ] **Smart Caching**
  - Configurable TTL (default 5 minutes)
  - Cache invalidation options
  - Background refresh

- [ ] **Bulk Update**
  - "Update All" button for outdated packages
  - Progress tracking for batch updates
  - Rollback on failure

- [ ] **History Page**
  - Persistent operation logs (JSON-based)
  - Filter by date/status/package
  - Export to CSV

**Target Release:** March 2026

---

### v1.0.0 - Production Ready (Q2 2026)

**Focus:** Stability, testing, and error handling

#### Features
- [ ] **Bulk Operations**
  - Multi-select install/uninstall
  - Batch operation confirmation dialog
  - Parallel execution (configurable limit)

- [ ] **Automatic Update Detection**
  - Background scanning for outdated packages
  - Notification system
  - Auto-update scheduling

- [ ] **File-Based Logging**
  - Rotating log files (daily/weekly)
  - Configurable log levels
  - Log viewer in UI

- [ ] **Comprehensive Error Handling**
  - User-friendly error dialogs
  - Retry mechanisms
  - WinGet troubleshooting wizard

- [ ] **Full Test Coverage**
  - Unit tests for all services (MSTest)
  - Integration tests for WinGet commands
  - UI tests for critical workflows
  - Code coverage >80%

- [ ] **Performance Optimization**
  - Lazy loading for large catalogs
  - Virtualized list views
  - Memory profiling and optimization

**Target Release:** June 2026

---

### v1.1.0 - Advanced Features (Q3 2026)

**Focus:** Power user features and customization

#### Features
- [ ] **Predefined Application Packs**
  - Developer Pack (Git, VS Code, Node.js, Docker, Postman)
  - Office Pack (LibreOffice, PDF readers, note-taking apps)
  - Gaming Pack (Discord, Steam, Epic Games, OBS)
  - Media Pack (VLC, Spotify, GIMP, Audacity)
  - One-click installation of entire packs

- [ ] **Import/Export Configurations**
  - `.pmconfig` file format (JSON)
  - Share configurations between machines
  - Configuration templates

- [ ] **Automatic Dark/Light Mode**
  - Follow Windows system theme
  - Manual override option
  - Custom accent colors

- [ ] **Parallel Execution**
  - Configurable concurrent operations (1-5)
  - Smart dependency resolution
  - Bandwidth throttling

- [ ] **Advanced Filtering**
  - Filter by installation status
  - Filter by update availability
  - Filter by package source
  - Custom filter combinations

**Target Release:** September 2026

---

### v1.2.0 - Internationalization (Q4 2026)

**Focus:** Multi-language support and localization

#### Features
- [ ] **Multi-Language Support**
  - English (default)
  - French
  - Spanish
  - German
  - Community translations (via Crowdin)

- [ ] **System Notifications**
  - Toast notifications for completed operations
  - Critical error alerts
  - Update availability notifications

- [ ] **Custom Package Icons**
  - Download icons from WinGet manifest
  - Fallback to default icon set
  - User-uploaded custom icons

- [ ] **Improved Search**
  - Fuzzy search
  - Search by tags
  - Search history

**Target Release:** December 2026

---

### v2.0.0 - Automation & Intelligence (2027)

**Focus:** Smart automation and self-updating

#### Features
- [ ] **Automatic PowerManager Updates**
  - Self-update mechanism
  - Background download
  - Release notes viewer

- [ ] **Scheduled Installations**
  - Cron-like scheduling
  - Install during off-hours
  - Maintenance windows

- [ ] **Usage Statistics**
  - Anonymous telemetry (opt-in)
  - Popular package rankings
  - Installation success rates

- [ ] **AI-Powered Recommendations**
  - Suggest apps based on installed software
  - Detect missing dependencies
  - Security vulnerability alerts

- [ ] **Package Groups**
  - Create custom groups
  - Group-based operations
  - Dependency management

**Target Release:** Q2 2027

---

## ?? Future Ideas (Beyond v2.0)

These are ideas under consideration but not yet scheduled:

- **Plugin System** - Extend PowerManager with community plugins
- **Winget Source Management** - Add/remove custom WinGet sources
- **Portable Mode** - Run without installation (USB stick)
- **Command-Line Interface** - Scriptable PowerManager operations
- **Integration with Chocolatey** - Support for Chocolatey packages
- **System Restore Points** - Auto-create before major changes
- **Package Pinning** - Prevent auto-updates for specific packages
- **Delta Updates** - Download only changed files
- **Offline Mode** - Pre-download packages for offline installation
- **Corporate Features** - Group policies, centralized management

---

## ?? Feature Voting

Want a feature prioritized? Vote on [GitHub Discussions](https://github.com/shuntps/PowerManager_Software/discussions)!

---

## ?? Contributing to the Roadmap

Have ideas? We'd love to hear them!

- ?? **Suggest features** - Open a Discussion on GitHub
- ?? **Report bugs** - Help us improve quality
- ?? **Improve docs** - Clarify roadmap items
- ?? **Submit PRs** - Implement planned features

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## ?? Release Schedule

We aim for **quarterly major releases** with monthly patch updates:

| Version | Target Date | Focus                  |
| ------- | ----------- | ---------------------- |
| v0.4.0  | March 2026  | Enhanced Catalog       |
| v1.0.0  | June 2026   | Production Ready       |
| v1.1.0  | Sept 2026   | Advanced Features      |
| v1.2.0  | Dec 2026    | Internationalization   |
| v2.0.0  | Q2 2027     | Automation & Intelligence |

**Note:** Dates are estimates and may shift based on complexity and community contributions.

---

**Last Updated:** February 3, 2026  
**Next Review:** March 2026

For questions or feedback, open a [GitHub Discussion](https://github.com/shuntps/PowerManager_Software/discussions).
