<#
.SYNOPSIS
    Generates placeholder MSIX assets for PowerManager packaging.

.DESCRIPTION
    Creates temporary PNG image assets required for MSIX packaging.
    These are basic placeholders - replace with proper branding for production.

.EXAMPLE
    .\Generate-PlaceholderAssets.ps1
#>

$ErrorActionPreference = "Stop"

$assetsPath = "$PSScriptRoot\..\src\PowerManager.UI\Assets"

Write-Host "Generating placeholder MSIX assets..." -ForegroundColor Cyan

# Asset specifications
$assets = @(
    @{ Name = "Square44x44Logo.png"; Width = 44; Height = 44 }
    @{ Name = "Square150x150Logo.png"; Width = 150; Height = 150 }
    @{ Name = "Wide310x150Logo.png"; Width = 310; Height = 150 }
    @{ Name = "SplashScreen.png"; Width = 620; Height = 300 }
    @{ Name = "StoreLogo.png"; Width = 50; Height = 50 }
)

# Ensure Assets directory exists
if (-not (Test-Path $assetsPath)) {
    New-Item -ItemType Directory -Path $assetsPath -Force | Out-Null
}

# Generate each asset using .NET Drawing
Add-Type -AssemblyName System.Drawing

foreach ($asset in $assets) {
    $filePath = Join-Path $assetsPath $asset.Name

    # Create bitmap
    $bitmap = New-Object System.Drawing.Bitmap $asset.Width, $asset.Height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)

    # Fill background (blue gradient)
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.Point 0, 0),
        (New-Object System.Drawing.Point $asset.Width, $asset.Height),
        [System.Drawing.Color]::FromArgb(0, 120, 215),  # Windows blue
        [System.Drawing.Color]::FromArgb(0, 90, 158)    # Darker blue
    )
    $graphics.FillRectangle($brush, 0, 0, $asset.Width, $asset.Height)

    # Draw "PM" text in center
    $font = New-Object System.Drawing.Font("Segoe UI", [Math]::Max(12, $asset.Height / 5), [System.Drawing.FontStyle]::Bold)
    $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = [System.Drawing.StringAlignment]::Center
    $format.LineAlignment = [System.Drawing.StringAlignment]::Center

    $rect = New-Object System.Drawing.RectangleF 0, 0, $asset.Width, $asset.Height
    $graphics.DrawString("PM", $font, $textBrush, $rect, $format)

    # Save as PNG
    $bitmap.Save($filePath, [System.Drawing.Imaging.ImageFormat]::Png)

    # Cleanup
    $graphics.Dispose()
    $bitmap.Dispose()
    $font.Dispose()
    $textBrush.Dispose()

    Write-Host "  ✓ Created $($asset.Name) ($($asset.Width)x$($asset.Height))" -ForegroundColor Green
}

Write-Host "`n✅ Placeholder assets generated successfully!" -ForegroundColor Green
Write-Host "⚠️  Replace with proper branding before production release" -ForegroundColor Yellow
Write-Host "   Assets location: $assetsPath" -ForegroundColor Gray
