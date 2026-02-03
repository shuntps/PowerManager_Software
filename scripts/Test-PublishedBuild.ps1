<#
.SYNOPSIS
    Tests the published PowerManager build before creating a release.

.DESCRIPTION
    Publishes the application, verifies files, and optionally launches for manual testing.

.PARAMETER SkipLaunch
    Skip launching the application (useful for CI/CD)

.EXAMPLE
    .\Test-PublishedBuild.ps1

.EXAMPLE
    .\Test-PublishedBuild.ps1 -SkipLaunch
#>

param(
    [switch]$SkipLaunch
)

$ErrorActionPreference = "Stop"
$PublishPath = "$PSScriptRoot\..\publish-test\win-x64"

Write-Host "=== PowerManager - Test de Build Publié ===" -ForegroundColor Cyan
Write-Host ""

# 1. Nettoyer le dossier précédent
if (Test-Path "$PSScriptRoot\..\publish-test") {
    Write-Host "Nettoyage du dossier publish-test..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force "$PSScriptRoot\..\publish-test"
}

# 2. Publier l'application
Write-Host "`nÉtape 1/4: Publication de l'application..." -ForegroundColor Cyan
dotnet publish "$PSScriptRoot\..\src\PowerManager.UI\PowerManager.UI.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=true `
    -o $PublishPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Échec de la publication"
    exit 1
}

Write-Host "✅ Publication réussie" -ForegroundColor Green

# 3. Vérifier les fichiers critiques
Write-Host "`nÉtape 2/4: Vérification des fichiers..." -ForegroundColor Cyan

$requiredFiles = @(
    "PowerManager.UI.exe",
    "PowerManager.Core.dll"
)

$allFilesPresent = $true
foreach ($file in $requiredFiles) {
    $path = Join-Path $PublishPath $file
    if (Test-Path $path) {
        $size = [math]::Round((Get-Item $path).Length / 1MB, 2)
        Write-Host "  ✓ $file ($size MB)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ $file (MANQUANT)" -ForegroundColor Red
        $allFilesPresent = $false
    }
}

if (-not $allFilesPresent) {
    Write-Error "❌ Fichiers manquants"
    exit 1
}

# 4. Calculer la taille totale
Write-Host "`nÉtape 3/4: Analyse de la taille..." -ForegroundColor Cyan
$totalSize = (Get-ChildItem $PublishPath -Recurse -File | Measure-Object -Property Length -Sum).Sum
$totalSizeMB = [math]::Round($totalSize / 1MB, 2)
Write-Host "  Taille totale: $totalSizeMB MB" -ForegroundColor Cyan

$fileCount = (Get-ChildItem $PublishPath -Recurse -File).Count
Write-Host "  Nombre de fichiers: $fileCount" -ForegroundColor Cyan

# 5. Créer l'archive ZIP (comme GitHub Actions)
Write-Host "`nÉtape 4/4: Création de l'archive ZIP..." -ForegroundColor Cyan
$zipPath = "$PSScriptRoot\..\publish-test\PowerManager-test-win-x64.zip"
Compress-Archive -Path "$PublishPath\*" -DestinationPath $zipPath -Force

$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
Write-Host "  ✓ Archive créée: $zipSize MB" -ForegroundColor Green

# 6. Lancer l'application (optionnel)
if (-not $SkipLaunch) {
    Write-Host "`n=== Test Manuel ===" -ForegroundColor Yellow
    Write-Host "Lancement de PowerManager.UI.exe..." -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Vérifiez que:" -ForegroundColor Yellow
    Write-Host "  1. L'application se lance sans erreur" -ForegroundColor Gray
    Write-Host "  2. La page Catalogue s'affiche avec les 5 packages" -ForegroundColor Gray
    Write-Host "  3. La recherche fonctionne" -ForegroundColor Gray
    Write-Host "  4. Les statuts des packages sont corrects" -ForegroundColor Gray
    Write-Host ""

    Start-Sleep -Seconds 2
    $exePath = Join-Path $PublishPath "PowerManager.UI.exe"
    Start-Process -FilePath $exePath -WorkingDirectory $PublishPath

    Write-Host "Application lancée. Fermez l'application après les tests." -ForegroundColor Green
} else {
    Write-Host "`n⚠️  Lancement manuel ignoré (utilisez sans -SkipLaunch pour tester)" -ForegroundColor Yellow
}

Write-Host "`n=== Résumé ===" -ForegroundColor Cyan
Write-Host "✅ Build publié avec succès" -ForegroundColor Green
Write-Host "📁 Chemin: $PublishPath" -ForegroundColor Gray
Write-Host "📦 Archive: $zipPath ($zipSize MB)" -ForegroundColor Gray
Write-Host "📊 Taille totale: $totalSizeMB MB" -ForegroundColor Gray
Write-Host ""
Write-Host "Prêt pour la release sur GitHub!" -ForegroundColor Green
