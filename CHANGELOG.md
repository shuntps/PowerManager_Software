## [0.3.5] - 2026-02-03

### ⚠️ BREAKING CHANGES
feat(catalog): implement private YAML catalog with real-time scanning and auto-refresh

### ✨ Features
- implement private YAML catalog with real-time scanning and auto-refresh
- add automated versioning system with PowerShell scripts

### 🐛 Bug Fixes
- upload only ZIP package and document SmartScreen warning
- improve changelog extraction regex in New-Release.ps1
- publish self-contained executable in GitHub releases

### ⚡ Performance Improvements
- enable dotnet cache to skip reinstallation
- remove duplicate build/test on main branch

### 📚 Documentation
- clarify conventional commits for infrastructure changes
- update README with versioning section and enhance copilot instructions


## [0.3.4] - 2026-02-03

### ⚠️ BREAKING CHANGES
feat(catalog): implement private YAML catalog with real-time scanning and auto-refresh

### ✨ Features
- implement private YAML catalog with real-time scanning and auto-refresh
- add automated versioning system with PowerShell scripts

### 🐛 Bug Fixes
- improve changelog extraction regex in New-Release.ps1
- publish self-contained executable in GitHub releases

### ⚡ Performance Improvements
- enable dotnet cache to skip reinstallation
- remove duplicate build/test on main branch

### 📚 Documentation
- update README with versioning section and enhance copilot instructions


## [0.3.3] - 2026-02-03

### ⚠️ BREAKING CHANGES
feat(catalog): implement private YAML catalog with real-time scanning and auto-refresh

### ✨ Features
- implement private YAML catalog with real-time scanning and auto-refresh
- add automated versioning system with PowerShell scripts

### 🐛 Bug Fixes
- improve changelog extraction regex in New-Release.ps1
- publish self-contained executable in GitHub releases

### ⚡ Performance Improvements
- remove duplicate build/test on main branch

### 📚 Documentation
- update README with versioning section and enhance copilot instructions


## [0.3.2] - 2026-02-03

### ⚠️ BREAKING CHANGES
feat(catalog): implement private YAML catalog with real-time scanning and auto-refresh

### ✨ Features
- implement private YAML catalog with real-time scanning and auto-refresh
- add automated versioning system with PowerShell scripts

### 🐛 Bug Fixes
- improve changelog extraction regex in New-Release.ps1
- publish self-contained executable in GitHub releases

### 📚 Documentation
- update README with versioning section and enhance copilot instructions


## [0.3.1] - 2026-02-03

### ⚠️ BREAKING CHANGES
feat(catalog): implement private YAML catalog with real-time scanning and auto-refresh

### ✨ Features
- implement private YAML catalog with real-time scanning and auto-refresh
- add automated versioning system with PowerShell scripts

### 🐛 Bug Fixes
- publish self-contained executable in GitHub releases

### 📚 Documentation
- update README with versioning section and enhance copilot instructions


## [0.3.0] - 2026-02-03

### ⚠️ BREAKING CHANGES
feat(catalog): implement private YAML catalog with real-time scanning and auto-refresh

### ✨ Features
- implement private YAML catalog with real-time scanning and auto-refresh
- add automated versioning system with PowerShell scripts

### 📚 Documentation
- update README with versioning section and enhance copilot instructions


## [0.2.0] - 2026-02-03


# Changelog

All notable changes to PowerManager will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-02-03

### ✨ Features

- **Core**: Initial release of PowerManager
- **UI**: WinUI 3 NavigationView shell with MVVM pattern
- **Catalog**: Basic package browsing functionality
- **Queue**: Queue management system for package operations
- **WinGet**: WinGet service integration for package management
- **DI**: Dependency injection container setup
- **Logging**: Compile-time logging with `[LoggerMessage]` attributes

### 🏗️ Architecture

- .NET 10 (C# 13) with WinUI 3 (Windows App SDK 1.8)
- MVVM Toolkit 8.3.2 with source generators
- MSTest 4.0.1 for unit testing
- UI thread marshaling via `IUiDispatcher`
- Service-based architecture with interface abstraction

### 📦 Components

- **PowerManager.Core**: Business logic layer (services, models, enums)
- **PowerManager.UI**: WinUI 3 presentation layer (views, view models)
- **PowerManager.Tests**: MSTest test suite

[0.1.0]: https://github.com/shuntps/PowerManager_Software/releases/tag/v0.1.0
