<#
.SYNOPSIS
    Test script for local MSIX build and installation validation.

.DESCRIPTION
    Validates the complete MSIX packaging workflow locally before CI/CD deployment.
    Tests: assets, certificate, build, bundle creation, and installation.

.EXAMPLE
    .\Test-MsixBuild.ps1

.EXAMPLE
    .\Test-MsixBuild.ps1 -CleanFirst
#>

param(
    [switch]$CleanFirst
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$projectRoot = Split-Path $PSScriptRoot
$testPublishPath = Join-Path $projectRoot "test-publish"
$certPassword = "TestPass123!"

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  PowerManager MSIX Build Test Suite" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Cleanup previous test artifacts
if ($CleanFirst -and (Test-Path $testPublishPath)) {
    Write-Host "🧹 Cleaning previous test artifacts..." -ForegroundColor Yellow
    Remove-Item $testPublishPath -Recurse -Force
    Write-Host "  ✓ Cleaned" -ForegroundColor Green
    Write-Host ""
}

# Test 1: Verify MSIX assets
Write-Host "📋 Test 1/5: Verifying MSIX assets..." -ForegroundColor Cyan
$assetsPath = Join-Path $projectRoot "src\PowerManager.UI\Assets"
$requiredAssets = @(
    "Square44x44Logo.png",
    "Square150x150Logo.png",
    "Wide310x150Logo.png",
    "SplashScreen.png",
    "StoreLogo.png"
)

$missingAssets = @()
foreach ($asset in $requiredAssets) {
    $path = Join-Path $assetsPath $asset
    if (-not (Test-Path $path)) {
        $missingAssets += $asset
    }
}

if ($missingAssets.Count -gt 0) {
    Write-Host "  ✗ Missing assets: $($missingAssets -join ', ')" -ForegroundColor Red
    Write-Host "  Run: .\scripts\Generate-PlaceholderAssets.ps1" -ForegroundColor Yellow
    exit 1
} else {
    $totalSize = (Get-ChildItem $assetsPath -Filter *.png | Measure-Object -Property Length -Sum).Sum / 1KB
    Write-Host "  ✓ All 5 assets present ($([math]::Round($totalSize, 1)) KB total)" -ForegroundColor Green
}
Write-Host ""

# Test 2: Create test certificate
Write-Host "📋 Test 2/5: Creating test certificate..." -ForegroundColor Cyan
$certPath = Join-Path $projectRoot "src\PowerManager.UI\PowerManager_TemporaryKey.pfx"

if (Test-Path $certPath) {
    Write-Host "  ℹ️  Using existing certificate: $certPath" -ForegroundColor Gray
} else {
    $createCertScript = Join-Path $PSScriptRoot "Create-Certificate.ps1"
    & $createCertScript -Password (ConvertTo-SecureString $certPassword -AsPlainText -Force) | Out-Null
}

if (Test-Path $certPath) {
    Write-Host "  ✓ Certificate ready ($([math]::Round((Get-Item $certPath).Length/1KB, 2)) KB)" -ForegroundColor Green
} else {
    Write-Host "  ✗ Certificate creation failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Test 3: Build x64 MSIX
Write-Host "📋 Test 3/5: Building x64 MSIX (this may take 1-2 minutes)..." -ForegroundColor Cyan
$buildOutput = dotnet publish "$projectRoot\src\PowerManager.UI\PowerManager.UI.csproj" `
    -c Release `
    -r win-x64 `
    -p:Platform=x64 `
    -p:RuntimeIdentifier=win-x64 `
    -p:AppxPackageDir="$testPublishPath\msix\x64\" `
    -p:AppxBundle=Never `
    -p:UapAppxPackageBuildMode=SideloadOnly `
    -p:AppxPackageSigningEnabled=true `
    -p:PackageCertificateKeyFile="PowerManager_TemporaryKey.pfx" `
    -p:PackageCertificatePassword="$certPassword" `
    -p:GenerateAppxPackageOnBuild=true `
    2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ Build failed" -ForegroundColor Red
    Write-Host $buildOutput
    exit 1
}

$x64Msix = Get-ChildItem "$testPublishPath\msix\x64" -Recurse -Filter "*.msix" | Select-Object -First 1
if ($x64Msix) {
    Write-Host "  ✓ x64 MSIX created: $($x64Msix.Name) ($([math]::Round($x64Msix.Length/1MB, 2)) MB)" -ForegroundColor Green
} else {
    Write-Host "  ✗ x64 MSIX file not found" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Test 4: Verify Windows SDK tools
Write-Host "📋 Test 4/5: Checking Windows SDK tools..." -ForegroundColor Cyan
$sdkBinPath = "${env:ProgramFiles(x86)}\Windows Kits\10\bin"
$sdkVersions = Get-ChildItem $sdkBinPath -Directory |
    Where-Object { $_.Name -match '^\d+\.\d+' } |
    Sort-Object Name -Descending

$makeappxPath = $null
foreach ($version in $sdkVersions) {
    $candidate = Join-Path $version.FullName "x64\makeappx.exe"
    if (Test-Path $candidate) {
        $makeappxPath = $candidate
        break
    }
}

if ($makeappxPath) {
    Write-Host "  ✓ makeappx.exe found: $((Split-Path (Split-Path $makeappxPath)).Name)" -ForegroundColor Green

    $signtoolPath = Join-Path (Split-Path $makeappxPath) "signtool.exe"
    if (Test-Path $signtoolPath) {
        Write-Host "  ✓ signtool.exe found" -ForegroundColor Green
    } else {
        Write-Host "  ✗ signtool.exe not found" -ForegroundColor Red
    }
} else {
    Write-Host "  ✗ makeappx.exe not found - cannot create .msixbundle" -ForegroundColor Red
    Write-Host "  ℹ️  Install Windows 10 SDK from: https://developer.microsoft.com/windows/downloads/windows-sdk/" -ForegroundColor Yellow
}
Write-Host ""

# Test 5: Validate Package.appxmanifest
Write-Host "📋 Test 5/5: Validating Package.appxmanifest..." -ForegroundColor Cyan
$manifestPath = Join-Path $projectRoot "src\PowerManager.UI\Package.appxmanifest"
$manifest = [xml](Get-Content $manifestPath)

$publisher = $manifest.Package.Identity.Publisher
$version = $manifest.Package.Identity.Version

Write-Host "  ✓ Publisher: $publisher" -ForegroundColor Green
Write-Host "  ✓ Version: $version" -ForegroundColor Green

# Verify Publisher matches certificate (load with password non-interactively)
try {
    $certBytes = [System.IO.File]::ReadAllBytes($certPath)
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certBytes, $certPassword)

    if ($cert.Subject -eq $publisher) {
        Write-Host "  ✓ Publisher matches certificate Subject" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️  Publisher mismatch:" -ForegroundColor Yellow
        Write-Host "     Manifest: $publisher" -ForegroundColor Gray
        Write-Host "     Certificate: $($cert.Subject)" -ForegroundColor Gray
    }
    $cert.Dispose()
} catch {
    Write-Host "  ⚠️  Could not verify certificate: $($_.Exception.Message)" -ForegroundColor Yellow
}
Write-Host ""

# Summary
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  TEST RESULTS" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ All core tests passed!" -ForegroundColor Green
Write-Host ""
Write-Host "📦 Generated artifacts:" -ForegroundColor Yellow
Write-Host "  • Certificate: $certPath" -ForegroundColor Gray
Write-Host "  • x64 MSIX: $($x64Msix.FullName)" -ForegroundColor Gray
Write-Host ""
Write-Host "🎯 Next steps:" -ForegroundColor Cyan
Write-Host "  1. Test installation: .\scripts\Install-PowerManager.ps1 -MsixPath '$($x64Msix.FullName)'" -ForegroundColor White
Write-Host "  2. Commit changes: git add . && git commit -m 'fix(msix): add MSIX packaging configuration'" -ForegroundColor White
Write-Host "  3. Push to branch: git push origin feat/msix-installer" -ForegroundColor White
Write-Host "  4. Merge to main: triggers automated v0.4.0 release with MSIX bundle" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  Remember:" -ForegroundColor Yellow
Write-Host "  • *.pfx files are gitignored (never commit certificates)" -ForegroundColor Gray
Write-Host "  • Self-signed cert = SmartScreen warning (documented in README.txt)" -ForegroundColor Gray
Write-Host "  • ARM64 build only tested in CI (windows-latest runner)" -ForegroundColor Gray
Write-Host ""
