<#
.SYNOPSIS
    Calculates the next semantic version based on conventional commits.

.DESCRIPTION
    Analyzes Git commits since the last version to determine the next version bump.
    Follows Conventional Commits specification and Semantic Versioning.

.PARAMETER CurrentVersion
    The current version from VERSION.txt (optional, will read from file if not provided)

.OUTPUTS
    String - The next version number (e.g., "0.2.0")

.EXAMPLE
    .\Get-NextVersion.ps1
    # Returns: 0.2.0
#>

param(
    [string]$CurrentVersion
)

# Read current version from VERSION.txt if not provided
if (-not $CurrentVersion) {
    $CurrentVersion = (Get-Content "$PSScriptRoot\..\VERSION.txt" -Raw).Trim()
}

Write-Host "Current version: $CurrentVersion" -ForegroundColor Cyan

# Parse current version
if ($CurrentVersion -notmatch '^(\d+)\.(\d+)\.(\d+)$') {
    Write-Error "Invalid version format in VERSION.txt. Expected: MAJOR.MINOR.PATCH"
    exit 1
}

$major = [int]$matches[1]
$minor = [int]$matches[2]
$patch = [int]$matches[3]

# Get commits since last tag
$lastTag = "v$CurrentVersion"
$tagExists = git tag -l $lastTag

if ($tagExists) {
    $commits = git log "$lastTag..HEAD" --pretty=format:"%s" 2>$null
} else {
    Write-Host "No tag found for $lastTag, analyzing all commits" -ForegroundColor Yellow
    $commits = git log --pretty=format:"%s" 2>$null
}

if (-not $commits) {
    Write-Host "No new commits found. Version remains: $CurrentVersion" -ForegroundColor Yellow
    Write-Output $CurrentVersion
    exit 0
}

Write-Host "`nAnalyzing commits..." -ForegroundColor Cyan
$commits | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }

# Determine bump type
$bumpType = "none"

foreach ($commit in $commits) {
    # Check for BREAKING CHANGE (MAJOR bump)
    if ($commit -match 'BREAKING CHANGE|^[a-z]+(\(.+\))?!:') {
        $bumpType = "major"
        Write-Host "`nBREAKING CHANGE detected" -ForegroundColor Red
        break
    }

    # Check for feat (MINOR bump)
    if ($commit -match '^feat(\(.+\))?:') {
        if ($bumpType -ne "major") {
            $bumpType = "minor"
        }
    }

    # Check for fix or perf (PATCH bump)
    if ($commit -match '^(fix|perf)(\(.+\))?:') {
        if ($bumpType -eq "none") {
            $bumpType = "patch"
        }
    }
}

# Calculate new version
switch ($bumpType) {
    "major" {
        $major++
        $minor = 0
        $patch = 0
        Write-Host "Bump type: MAJOR" -ForegroundColor Red
    }
    "minor" {
        $minor++
        $patch = 0
        Write-Host "Bump type: MINOR" -ForegroundColor Yellow
    }
    "patch" {
        $patch++
        Write-Host "Bump type: PATCH" -ForegroundColor Green
    }
    default {
        Write-Host "No version-affecting commits found (docs/chore/style/refactor/test/ci)" -ForegroundColor Yellow
        Write-Output $CurrentVersion
        exit 0
    }
}

$newVersion = "$major.$minor.$patch"
Write-Host "`nNext version: $newVersion" -ForegroundColor Green
Write-Output $newVersion
