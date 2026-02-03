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

    # Extract section for this version
    if ($changelog -match "## \[$Version\].*?\n(.*?)(?=\n## \[|$)") {
        $releaseNotes = $matches[1].Trim()
        Set-Content -Path $releaseNotesPath -Value $releaseNotes -NoNewline
        Write-Host "Release notes extracted from CHANGELOG.md" -ForegroundColor Green
    } else {
        $releaseNotes = "Release $Version"
        Set-Content -Path $releaseNotesPath -Value $releaseNotes -NoNewline
        Write-Host "Warning: Could not extract release notes from CHANGELOG" -ForegroundColor Yellow
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
if (Test-Path $PublishPath) {
    $artifacts = Get-ChildItem $PublishPath -Recurse -File
    if ($artifacts.Count -gt 0) {
        Write-Host "Uploading $($artifacts.Count) artifact(s)..." -ForegroundColor Cyan
        $artifacts | ForEach-Object {
            $releaseArgs += $_.FullName
            Write-Host "  - $($_.Name)" -ForegroundColor Gray
        }
    }
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
