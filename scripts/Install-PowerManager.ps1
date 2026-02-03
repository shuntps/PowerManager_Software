<#
.SYNOPSIS
    Installs PowerManager MSIX package with automatic certificate handling.

.DESCRIPTION
    This script automates the installation of PowerManager by:
    1. Extracting the embedded self-signed certificate from the .msix
    2. Installing the certificate to Trusted Root Certification Authorities
    3. Installing the .msix package
    4. Optionally launching the application

    Requires Administrator privileges for certificate installation.

.PARAMETER MsixPath
    Path to the PowerManager .msix or .msixbundle file.
    If not specified, searches current directory.

.PARAMETER SkipLaunch
    Don't launch PowerManager after installation.

.EXAMPLE
    .\Install-PowerManager.ps1
    Installs from .msix in current directory with admin prompt

.EXAMPLE
    .\Install-PowerManager.ps1 -MsixPath "C:\Downloads\PowerManager-v0.4.0.msixbundle"
    Installs from specific file

.EXAMPLE
    .\Install-PowerManager.ps1 -SkipLaunch
    Installs without launching the app
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$MsixPath,

    [Parameter()]
    [switch]$SkipLaunch
)

$ErrorActionPreference = "Stop"

# ============================================================================
# Helper Functions
# ============================================================================

function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Request-AdminPrivileges {
    if (-not (Test-Administrator)) {
        Write-Host "⚠️  Administrator privileges required for certificate installation" -ForegroundColor Yellow
        Write-Host "   Relaunching with elevated privileges..." -ForegroundColor Gray

        $arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`""
        if ($MsixPath) { $arguments += " -MsixPath `"$MsixPath`"" }
        if ($SkipLaunch) { $arguments += " -SkipLaunch" }

        Start-Process powershell.exe -Verb RunAs -ArgumentList $arguments
        exit
    }
}

function Find-MsixFile {
    if ($MsixPath -and (Test-Path $MsixPath)) {
        return $MsixPath
    }

    # Search current directory for .msix or .msixbundle
    $msixFiles = Get-ChildItem -Path $PSScriptRoot -Filter "*.msix*" | Where-Object { $_.Name -match '\.(msix|msixbundle)$' }

    if ($msixFiles.Count -eq 0) {
        throw "No .msix or .msixbundle file found in current directory. Use -MsixPath parameter."
    }

    if ($msixFiles.Count -eq 1) {
        return $msixFiles[0].FullName
    }

    # Multiple files found - prompt user
    Write-Host "`nMultiple MSIX packages found:" -ForegroundColor Cyan
    for ($i = 0; $i -lt $msixFiles.Count; $i++) {
        Write-Host "  [$($i + 1)] $($msixFiles[$i].Name) ($([math]::Round($msixFiles[$i].Length/1MB, 2)) MB)"
    }

    $selection = Read-Host "`nSelect package to install (1-$($msixFiles.Count))"
    $index = [int]$selection - 1

    if ($index -lt 0 -or $index -ge $msixFiles.Count) {
        throw "Invalid selection"
    }

    return $msixFiles[$index].FullName
}

function Extract-CertificateFromMsix {
    param([string]$MsixFile)

    Write-Host "`n📜 Extracting certificate from MSIX package..." -ForegroundColor Cyan

    try {
        # Get digital signature
        $signature = Get-AuthenticodeSignature -FilePath $MsixFile

        if ($signature.Status -ne "Valid" -and $signature.Status -ne "UnknownError") {
            throw "MSIX package is not signed or signature is invalid: $($signature.Status)"
        }

        $cert = $signature.SignerCertificate

        if (-not $cert) {
            throw "Could not extract certificate from MSIX package"
        }

        Write-Host "   ✓ Certificate subject: $($cert.Subject)" -ForegroundColor Green
        Write-Host "   ✓ Certificate issuer: $($cert.Issuer)" -ForegroundColor Green
        Write-Host "   ✓ Valid until: $($cert.NotAfter.ToString('yyyy-MM-dd'))" -ForegroundColor Green

        return $cert
    }
    catch {
        Write-Error "Failed to extract certificate: $_"
        throw
    }
}

function Install-Certificate {
    param([System.Security.Cryptography.X509Certificates.X509Certificate2]$Certificate)

    Write-Host "`n🔐 Installing certificate to Trusted Root..." -ForegroundColor Cyan

    try {
        $store = New-Object System.Security.Cryptography.X509Certificates.X509Store(
            [System.Security.Cryptography.X509Certificates.StoreName]::Root,
            [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine
        )

        $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)

        # Check if certificate already exists
        $existing = $store.Certificates | Where-Object { $_.Thumbprint -eq $Certificate.Thumbprint }

        if ($existing) {
            Write-Host "   ℹ️  Certificate already installed (Thumbprint: $($Certificate.Thumbprint))" -ForegroundColor Yellow
        }
        else {
            $store.Add($Certificate)
            Write-Host "   ✓ Certificate installed successfully" -ForegroundColor Green
            Write-Host "   ℹ️  Thumbprint: $($Certificate.Thumbprint)" -ForegroundColor Gray
        }

        $store.Close()
    }
    catch {
        Write-Error "Failed to install certificate: $_"
        throw
    }
}

function Install-MsixPackage {
    param([string]$MsixFile)

    Write-Host "`n📦 Installing PowerManager MSIX package..." -ForegroundColor Cyan

    try {
        Add-AppxPackage -Path $MsixFile -ErrorAction Stop
        Write-Host "   ✓ PowerManager installed successfully!" -ForegroundColor Green
    }
    catch {
        if ($_.Exception.Message -match "already installed") {
            Write-Host "   ℹ️  PowerManager is already installed" -ForegroundColor Yellow
            Write-Host "   💡 Use Windows Settings > Apps to uninstall first, or use -Force" -ForegroundColor Gray
        }
        else {
            Write-Error "Failed to install MSIX package: $_"
            throw
        }
    }
}

function Start-PowerManager {
    Write-Host "`n🚀 Launching PowerManager..." -ForegroundColor Cyan

    try {
        # Get the installed app
        $app = Get-AppxPackage -Name "PowerManagerSoftware" -ErrorAction SilentlyContinue

        if (-not $app) {
            Write-Warning "Could not find installed PowerManager app"
            return
        }

        # Launch using shell:AppsFolder
        $appId = $app.PackageFamilyName + "!PowerManager"
        Start-Process "shell:AppsFolder\$appId"

        Write-Host "   ✓ PowerManager started" -ForegroundColor Green
    }
    catch {
        Write-Warning "Could not launch PowerManager automatically: $_"
        Write-Host "   💡 Launch manually from Start Menu" -ForegroundColor Gray
    }
}

# ============================================================================
# Main Installation Flow
# ============================================================================

Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Blue
Write-Host "║          PowerManager MSIX Installer v0.4.0           ║" -ForegroundColor Blue
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Blue
Write-Host ""

# Step 1: Check admin privileges
Request-AdminPrivileges

# Step 2: Find MSIX file
$msixFile = Find-MsixFile
Write-Host "📁 Package: $msixFile" -ForegroundColor Gray
Write-Host "   Size: $([math]::Round((Get-Item $msixFile).Length/1MB, 2)) MB" -ForegroundColor Gray

# Step 3: Extract and install certificate
$cert = Extract-CertificateFromMsix -MsixFile $msixFile
Install-Certificate -Certificate $cert

# Step 4: Install MSIX package
Install-MsixPackage -MsixFile $msixFile

# Step 5: Launch app (unless -SkipLaunch)
if (-not $SkipLaunch) {
    Start-PowerManager
}

Write-Host "`n✅ Installation complete!" -ForegroundColor Green
Write-Host ""
Write-Host "PowerManager is now installed on your system." -ForegroundColor White
Write-Host "Find it in your Start Menu or launch via:" -ForegroundColor Gray
Write-Host "  Start Menu > PowerManager" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠️  Note: This version uses a self-signed certificate." -ForegroundColor Yellow
Write-Host "   Future updates may require reinstalling the certificate." -ForegroundColor Yellow
Write-Host ""
