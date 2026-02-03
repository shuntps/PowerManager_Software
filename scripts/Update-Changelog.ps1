<#
.SYNOPSIS
    Updates CHANGELOG.md with new release information.

.DESCRIPTION
    Parses conventional commits and generates formatted changelog entries.
    Prepends new release section to existing CHANGELOG.md.

.PARAMETER Version
    The new version number (e.g., "0.2.0")

.PARAMETER Date
    Release date (defaults to today)

.EXAMPLE
    .\Update-Changelog.ps1 -Version "0.2.0"
#>

param(
    [Parameter(Mandatory)]
    [string]$Version,

    [string]$Date = (Get-Date -Format "yyyy-MM-dd")
)

$changelogPath = "$PSScriptRoot\..\CHANGELOG.md"
$currentVersion = (Get-Content "$PSScriptRoot\..\VERSION.txt" -Raw).Trim()

Write-Host "Generating changelog for version $Version..." -ForegroundColor Cyan

# Get commits since last tag
$lastTag = "v$currentVersion"
$tagExists = git tag -l $lastTag

if ($tagExists) {
    $commits = git log "$lastTag..HEAD" --pretty=format:"%s|||%b" 2>$null
} else {
    $commits = git log --pretty=format:"%s|||%b" 2>$null
}

if (-not $commits) {
    Write-Host "No commits to process" -ForegroundColor Yellow
    exit 0
}

# Categorize commits
$features = @()
$fixes = @()
$performance = @()
$refactors = @()
$docs = @()
$breaking = @()

foreach ($commit in $commits) {
    $parts = $commit -split '\|\|\|'
    $subject = $parts[0]
    $body = if ($parts.Length -gt 1) { $parts[1] } else { "" }

    # Check for breaking changes
    if ($subject -match '!:' -or $body -match 'BREAKING CHANGE') {
        $breaking += $subject
    }

    # Categorize by type
    if ($subject -match '^feat(\(.+\))?:\s*(.+)') {
        $features += "- $($matches[2])"
    }
    elseif ($subject -match '^fix(\(.+\))?:\s*(.+)') {
        $fixes += "- $($matches[2])"
    }
    elseif ($subject -match '^perf(\(.+\))?:\s*(.+)') {
        $performance += "- $($matches[2])"
    }
    elseif ($subject -match '^refactor(\(.+\))?:\s*(.+)') {
        $refactors += "- $($matches[2])"
    }
    elseif ($subject -match '^docs(\(.+\))?:\s*(.+)') {
        $docs += "- $($matches[2])"
    }
}

# Build new changelog entry
$newEntry = @"
## [$Version] - $Date

"@

if ($breaking.Count -gt 0) {
    $newEntry += @"

### ⚠️ BREAKING CHANGES

"@
    $breaking | ForEach-Object { $newEntry += "$_`n" }
}

if ($features.Count -gt 0) {
    $newEntry += @"

### ✨ Features

"@
    $features | ForEach-Object { $newEntry += "$_`n" }
}

if ($fixes.Count -gt 0) {
    $newEntry += @"

### 🐛 Bug Fixes

"@
    $fixes | ForEach-Object { $newEntry += "$_`n" }
}

if ($performance.Count -gt 0) {
    $newEntry += @"

### ⚡ Performance Improvements

"@
    $performance | ForEach-Object { $newEntry += "$_`n" }
}

if ($refactors.Count -gt 0) {
    $newEntry += @"

### ♻️ Code Refactoring

"@
    $refactors | ForEach-Object { $newEntry += "$_`n" }
}

if ($docs.Count -gt 0) {
    $newEntry += @"

### 📚 Documentation

"@
    $docs | ForEach-Object { $newEntry += "$_`n" }
}

# Read existing changelog or create header
if (Test-Path $changelogPath) {
    $existingContent = Get-Content $changelogPath -Raw

    # Insert new entry after header
    if ($existingContent -match '(# Changelog.*?\n\n)(.*)') {
        $header = $matches[1]
        $rest = $matches[2]
        $updatedContent = $header + $newEntry + "`n" + $rest
    } else {
        # No proper header, prepend
        $updatedContent = $newEntry + "`n`n" + $existingContent
    }
} else {
    # Create new changelog
    $updatedContent = @"
# Changelog

All notable changes to PowerManager will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

$newEntry

"@
}

# Write updated changelog
Set-Content -Path $changelogPath -Value $updatedContent -NoNewline

Write-Host "`nCHANGELOG.md updated successfully!" -ForegroundColor Green
Write-Host "`nNew entry preview:" -ForegroundColor Cyan
Write-Host $newEntry
