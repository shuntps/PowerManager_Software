<#
.SYNOPSIS
    Creates a self-signed code signing certificate for PowerManager MSIX packaging.

.DESCRIPTION
    Generates a self-signed X.509 certificate suitable for code signing MSIX packages.
    The certificate is stored in the current user's certificate store and exported
    as a .pfx file for use in CI/CD pipelines.

.PARAMETER CertificateName
    Common Name (CN) for the certificate. Must match Package.appxmanifest Publisher.
    Default: "PowerManager Development"

.PARAMETER Password
    Password to protect the exported .pfx file.
    If not specified, prompts securely.

.PARAMETER ExportPath
    Directory where the .pfx file will be exported.
    Default: src/PowerManager.UI/

.PARAMETER ValidYears
    Certificate validity period in years.
    Default: 2 years

.EXAMPLE
    .\Create-Certificate.ps1
    Creates certificate with default settings and prompts for password

.EXAMPLE
    .\Create-Certificate.ps1 -CertificateName "CN=MyCompany" -ValidYears 5
    Creates certificate valid for 5 years with custom CN

.EXAMPLE
    $pwd = ConvertTo-SecureString "MyPassword123" -AsPlainText -Force
    .\Create-Certificate.ps1 -Password $pwd
    Creates certificate with programmatic password (for CI/CD)
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$CertificateName = "PowerManager Development",

    [Parameter()]
    [SecureString]$Password,

    [Parameter()]
    [string]$ExportPath = "$PSScriptRoot\..\src\PowerManager.UI",

    [Parameter()]
    [int]$ValidYears = 2
)

$ErrorActionPreference = "Stop"

Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║    PowerManager Certificate Generator v0.4.0          ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Ensure CN= prefix
if (-not $CertificateName.StartsWith("CN=")) {
    $CertificateName = "CN=$CertificateName"
}

Write-Host "📜 Certificate Configuration:" -ForegroundColor Yellow
Write-Host "   Subject: $CertificateName" -ForegroundColor Gray
Write-Host "   Valid for: $ValidYears years" -ForegroundColor Gray
Write-Host "   Export path: $ExportPath" -ForegroundColor Gray
Write-Host ""

# Request password if not provided
if (-not $Password) {
    Write-Host "🔐 Enter password to protect the certificate (min 8 chars):" -ForegroundColor Cyan
    $Password = Read-Host -AsSecureString
    Write-Host "🔐 Confirm password:" -ForegroundColor Cyan
    $PasswordConfirm = Read-Host -AsSecureString

    # Compare SecureStrings
    $pwd1 = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password))
    $pwd2 = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($PasswordConfirm))

    if ($pwd1 -ne $pwd2) {
        Write-Error "Passwords do not match"
        exit 1
    }

    if ($pwd1.Length -lt 8) {
        Write-Error "Password must be at least 8 characters"
        exit 1
    }
}

Write-Host "🔨 Creating self-signed certificate..." -ForegroundColor Cyan

try {
    # Create code signing certificate
    $cert = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject $CertificateName `
        -KeyUsage DigitalSignature `
        -FriendlyName "PowerManager Code Signing" `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}") `
        -NotAfter (Get-Date).AddYears($ValidYears)

    Write-Host "   ✓ Certificate created successfully" -ForegroundColor Green
    Write-Host "   ℹ️  Thumbprint: $($cert.Thumbprint)" -ForegroundColor Gray
    Write-Host "   ℹ️  Valid until: $($cert.NotAfter.ToString('yyyy-MM-dd'))" -ForegroundColor Gray

    # Export to .pfx file
    $pfxFileName = "PowerManager_TemporaryKey.pfx"
    $pfxPath = Join-Path $ExportPath $pfxFileName

    # Ensure export directory exists
    if (-not (Test-Path $ExportPath)) {
        New-Item -ItemType Directory -Path $ExportPath -Force | Out-Null
    }

    Write-Host "`n📤 Exporting certificate to .pfx file..." -ForegroundColor Cyan

    Export-PfxCertificate `
        -Cert $cert `
        -FilePath $pfxPath `
        -Password $Password | Out-Null

    Write-Host "   ✓ Certificate exported to: $pfxPath" -ForegroundColor Green
    Write-Host "   ℹ️  File size: $([math]::Round((Get-Item $pfxPath).Length/1KB, 2)) KB" -ForegroundColor Gray

    # Display next steps
    Write-Host "`n✅ Certificate created successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Update Package.appxmanifest if CN changed:" -ForegroundColor White
    Write-Host "     Publisher=`"$CertificateName`"" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  2. Build MSIX package with this certificate:" -ForegroundColor White
    Write-Host "     dotnet publish -p:PackageCertificateKeyFile=`"$pfxFileName`"" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  3. For GitHub Actions, add certificate password to secrets:" -ForegroundColor White
    Write-Host "     Settings > Secrets > CERT_PASSWORD" -ForegroundColor Gray
    Write-Host ""
    Write-Host "⚠️  Security Notes:" -ForegroundColor Yellow
    Write-Host "  • Add *.pfx to .gitignore (NEVER commit certificates)" -ForegroundColor Red
    Write-Host "  • This is a SELF-SIGNED certificate (not trusted by default)" -ForegroundColor Yellow
    Write-Host "  • Users must install certificate manually OR use Install-PowerManager.ps1" -ForegroundColor Yellow
    Write-Host "  • For production, purchase commercial code signing certificate (~\$300/year)" -ForegroundColor Yellow
    Write-Host ""

    # Check if .gitignore excludes .pfx files
    $gitignorePath = Join-Path (Split-Path $PSScriptRoot) ".gitignore"
    if (Test-Path $gitignorePath) {
        $gitignoreContent = Get-Content $gitignorePath -Raw
        if ($gitignoreContent -notmatch '\*\.pfx') {
            Write-Host "⚠️  WARNING: .pfx files are NOT excluded in .gitignore!" -ForegroundColor Red
            Write-Host "   Run: echo '*.pfx' >> .gitignore" -ForegroundColor Yellow
        }
    }
}
catch {
    Write-Error "Failed to create certificate: $_"
    exit 1
}
