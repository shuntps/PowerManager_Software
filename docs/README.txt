==============================================================================
  PowerManager Software - Portable ZIP Distribution
==============================================================================

Thank you for downloading PowerManager!

WHAT IS THIS?
-------------
This ZIP archive contains a portable, standalone version of PowerManager
that does not require installation. Extract and run directly from any folder.

SYSTEM REQUIREMENTS
-------------------
- Windows 10 (version 1809 or later) / Windows 11
- x64 (64-bit) processor
- No administrative privileges required after extraction

INSTALLATION INSTRUCTIONS
--------------------------
1. Extract all files from this ZIP archive to a folder of your choice
   (e.g., C:\Apps\PowerManager or %LocalAppData%\PowerManager)

2. Locate PowerManager.UI.exe in the extracted folder

3. Double-click PowerManager.UI.exe to launch the application

WINDOWS SMARTSCREEN WARNING
----------------------------
On first launch, Windows Defender SmartScreen may display:
  "Windows protected your PC - Microsoft Defender SmartScreen prevented
   an unrecognized app from starting"

This is NORMAL for unsigned applications. PowerManager is safe but does not
have a commercial code signing certificate (planned for v1.0.0+).

To bypass this warning:
1. Click "More info"
2. Click "Run anyway"

This warning appears only once. Subsequent launches will work normally.

WHY PORTABLE VS. MSIX INSTALLER?
---------------------------------
Choose the PORTABLE ZIP if you:
  - Want to run PowerManager without installation
  - Need to use PowerManager on systems where you don't have admin rights
  - Prefer manual control over file locations
  - Want to test PowerManager before installing

Choose the MSIX INSTALLER (PowerManager-vX.X.X-win-multiarch.msixbundle) if you:
  - Want automatic updates (future feature)
  - Prefer proper Windows integration (Start Menu, App list, uninstall)
  - Need ARM64 support (experimental)
  - Want a cleaner installation experience

For MSIX installation instructions, see:
https://github.com/shuntps/PowerManager_Software#installation

USAGE
-----
PowerManager provides a modern, intuitive interface to manage Windows packages
using WinGet. Features include:

- Browse and search 40+ curated applications
- Install, update, and uninstall packages via queue system
- Real-time package status detection (installed/outdated/available)
- Categorized package catalog (Development, Productivity, Media, etc.)
- Lightweight and fast (no background services)

For full documentation, visit:
https://github.com/shuntps/PowerManager_Software

TROUBLESHOOTING
---------------
Q: PowerManager won't launch / crashes on startup
A: Ensure you have Windows 10 version 1809 or later. Run winver.exe to check.

Q: "This app can't run on your PC" error
A: This ZIP is for 64-bit (x64) Windows only. Check system type in Settings.

Q: Packages won't install
A: PowerManager requires WinGet to be installed. Download from:
   https://github.com/microsoft/winget-cli/releases

Q: "Access denied" when installing packages
A: Some packages require administrator privileges. Right-click
   PowerManager.UI.exe and select "Run as administrator"

UPDATES
-------
Portable versions do NOT auto-update. Check GitHub releases regularly:
https://github.com/shuntps/PowerManager_Software/releases

To update:
1. Download the latest ZIP
2. Extract to a new folder (or replace existing files)
3. Launch PowerManager.UI.exe

Your package catalog customizations are stored separately in:
%LocalAppData%\PowerManager Software\catalog_custom.yaml

UNINSTALL
---------
Simply delete the extracted folder. No registry entries or system changes
are made (except for installed packages via WinGet).

LICENSE
-------
PowerManager is open-source software licensed under the MIT License.
See LICENSE file in the repository for details.

SUPPORT
-------
- GitHub Issues: https://github.com/shuntps/PowerManager_Software/issues
- Documentation: https://github.com/shuntps/PowerManager_Software#readme

==============================================================================
PowerManager v0.4.0 | https://github.com/shuntps/PowerManager_Software
==============================================================================
