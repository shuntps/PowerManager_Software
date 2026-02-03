# PowerManager MSIX Assets

This folder contains the required image assets for MSIX packaging.

## Required Assets (Placeholder - TO BE REPLACED)

Currently using placeholder images. For production release, replace with proper branding:

- **Square44x44Logo.png** (44x44px) - App list icon
- **Square150x150Logo.png** (150x150px) - Start menu tile
- **Wide310x150Logo.png** (310x150px) - Wide start menu tile
- **SplashScreen.png** (620x300px) - Splash screen
- **StoreLogo.png** (50x50px) - Microsoft Store listing

## Design Guidelines

- Use **transparent backgrounds** for logos
- Follow **Windows 11 design language** (rounded corners)
- Use **PowerManager brand colors** (blue/white theme)
- Export at **@1x, @1.25x, @1.5x, @2x** for different DPI scales
- File formats: **PNG** with transparency

## Temporary Solution

Current placeholders are generated programmatically during build.
For v0.4.0 release, proper icons should be designed using:

- Adobe Illustrator / Figma (vector)
- Export to PNG at required sizes
- Optimize with ImageOptim/TinyPNG

## Future Enhancement (v0.5.0+)

- Animated splash screen
- Contrast-aware icons (light/dark theme variants)
- High-resolution 4K displays support
