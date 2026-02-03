<#
.SYNOPSIS
    Creates a new GitHub release with compiled artifacts.

.DESCRIPTION
    Creates Git tag, pushes to remote, and creates GitHub Release using GitHub CLI.
    Uploads compiled binaries as release assets.

.PARAMETER Version
    The version to release (e.g., "0.2.0")

.PARAMETER PublishPath
    Path to published artifacts (defaults to "publish/")

.PARAMETER DryRun
    Preview release without creating it

.EXAMPLE
    .\New-Release.ps1 -Version "0.2.0"

.EXAMPLE
    .\New-Release.ps1 -Version "0.2.0" -DryRun
#>

param(
    [Parameter(Mandatory)]
    [string]$Version,

    [string]$PublishPath = "$PSScriptRoot\..\publish",

    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

Write-Host "Creating release for version $Version..." -ForegroundColor Cyan

# Validate version format
if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    Write-Error "Invalid version format. Expected: MAJOR.MINOR.PATCH"
    exit 1
}

$tag = "v$Version"

# Extract release notes from CHANGELOG.md
$changelogPath = "$PSScriptRoot\..\CHANGELOG.md"
$releaseNotesPath = "$PSScriptRoot\..\RELEASE_NOTES.md"

if (Test-Path $changelogPath) {
    $changelog = Get-Content $changelogPath -Raw

    # Extract section for this version - improved regex to handle emoji sections
    # Match: ## [0.3.1] - 2026-02-03 ... until next ## [ or end of file
    $pattern = "## \[$([regex]::Escape($Version))\].*?\n([\s\S]*?)(?=\n## \[|\z)"

    if ($changelog -match $pattern) {
        $releaseNotes = $matches[1].Trim()

        # Remove empty lines at start/end
        $releaseNotes = $releaseNotes -replace '^\s+|\s+$', ''

        if ([string]::IsNullOrWhiteSpace($releaseNotes)) {
            $releaseNotes = "Release $Version`n`nNo detailed changelog available for this version."
            Write-Host "Warning: Empty changelog section for v$Version, using default message" -ForegroundColor Yellow
        } else {
            Write-Host "Release notes extracted from CHANGELOG.md ($($releaseNotes.Length) chars)" -ForegroundColor Green
        }

        Set-Content -Path $releaseNotesPath -Value $releaseNotes -NoNewline -Encoding UTF8
    } else {
        $releaseNotes = "Release $Version`n`nSee [CHANGELOG.md](CHANGELOG.md) for details."
        Set-Content -Path $releaseNotesPath -Value $releaseNotes -NoNewline -Encoding UTF8
        Write-Host "Warning: Could not find section [v$Version] in CHANGELOG" -ForegroundColor Yellow
        Write-Host "Using default release notes" -ForegroundColor Yellow
    }
} else {
    Write-Error "CHANGELOG.md not found"
    exit 1
}

if ($DryRun) {
    Write-Host "`n=== DRY RUN MODE ===" -ForegroundColor Yellow
    Write-Host "Would create tag: $tag" -ForegroundColor Cyan
    Write-Host "`nRelease notes:" -ForegroundColor Cyan
    Write-Host $releaseNotes
    Write-Host "`nArtifacts to upload:" -ForegroundColor Cyan
    if (Test-Path $PublishPath) {
        Get-ChildItem $PublishPath -Recurse -File | ForEach-Object {
            Write-Host "  - $($_.FullName)" -ForegroundColor Gray
        }
    } else {
        Write-Host "  (Publish path not found: $PublishPath)" -ForegroundColor Yellow
    }
    exit 0
}

# Check if gh CLI is available
$ghExists = Get-Command gh -ErrorAction SilentlyContinue
if (-not $ghExists) {
    Write-Error "GitHub CLI (gh) not found. Install from: https://cli.github.com/"
    exit 1
}

# Create and push tag
Write-Host "`nCreating Git tag: $tag..." -ForegroundColor Cyan
git tag -a $tag -m "Release $Version"

Write-Host "Pushing tag to remote..." -ForegroundColor Cyan
git push origin $tag

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to push tag"
    exit 1
}

# Create GitHub Release
Write-Host "`nCreating GitHub Release..." -ForegroundColor Cyan

$releaseArgs = @(
    "release", "create", $tag,
    "--title", "v$Version",
    "--notes-file", $releaseNotesPath
)

# Add artifacts if publish path exists
# Only upload distribution packages (.zip, .msixbundle), NOT individual .exe or .msix files
if (Test-Path $PublishPath) {
    $artifacts = Get-ChildItem $PublishPath -Recurse -File -Include *.zip, *.msixbundle
    if ($artifacts.Count -gt 0) {
        Write-Host "Uploading $($artifacts.Count) distribution package(s)..." -ForegroundColor Cyan
        $artifacts | ForEach-Object {
            $releaseArgs += $_.FullName
            Write-Host "  - $($_.Name) ($([math]::Round($_.Length/1MB, 2)) MB)" -ForegroundColor Gray
        }
    } else {
        Write-Host "Warning: No distribution packages found in $PublishPath" -ForegroundColor Yellow
        Write-Host "Release will be created without binary attachments" -ForegroundColor Yellow
    }
} else {
    Write-Host "Warning: Publish path not found: $PublishPath" -ForegroundColor Yellow
    Write-Host "Release will be created without binary attachments" -ForegroundColor Yellow
}

& gh @releaseArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create GitHub Release"
    exit 1
}

# Cleanup temporary release notes
Remove-Item $releaseNotesPath -ErrorAction SilentlyContinue

Write-Host "`n✅ Release $Version created successfully!" -ForegroundColor Green
Write-Host "View at: https://github.com/$env:GITHUB_REPOSITORY/releases/tag/$tag" -ForegroundColor Cyan
