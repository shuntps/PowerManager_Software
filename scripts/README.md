# PowerManager Release Scripts

PowerShell scripts for automated versioning and release management.

## 📁 Scripts Overview

| Script                 | Purpose                             | Usage                                     |
| ---------------------- | ----------------------------------- | ----------------------------------------- |
| `Get-NextVersion.ps1`  | Calculate next version from commits | `.\Get-NextVersion.ps1`                   |
| `Update-Changelog.ps1` | Generate CHANGELOG entry            | `.\Update-Changelog.ps1 -Version "0.2.0"` |
| `New-Release.ps1`      | Create GitHub Release               | `.\New-Release.ps1 -Version "0.2.0"`      |

## 🚀 Quick Start

### Automated (GitHub Actions)

Push commits to `master` - everything happens automatically:

```bash
git commit -m "feat: add amazing feature"
git push origin master
# GitHub Actions handles the rest
```

### Manual Release

```powershell
# 1. Calculate next version
$version = .\scripts\Get-NextVersion.ps1

# 2. Update VERSION.txt
$version | Out-File VERSION.txt -NoNewline

# 3. Update CHANGELOG
.\scripts\Update-Changelog.ps1 -Version $version

# 4. Commit changes
git add VERSION.txt CHANGELOG.md
git commit -m "chore(release): $version [skip ci]"
git push

# 5. Build project
dotnet publish src/PowerManager.UI/PowerManager.UI.csproj -c Release -o publish/

# 6. Create release
.\scripts\New-Release.ps1 -Version $version
```

## 📖 Script Details

### Get-NextVersion.ps1

Analyzes commits using Conventional Commits to determine version bump:

- `feat:` → MINOR bump (0.1.0 → 0.2.0)
- `fix:` → PATCH bump (0.1.0 → 0.1.1)
- `feat!:` or `BREAKING CHANGE:` → MAJOR bump (0.1.0 → 1.0.0)
- `docs:`, `chore:`, etc. → No bump

**Output**: Next version string (e.g., "0.2.0")

### Update-Changelog.ps1

Generates formatted CHANGELOG.md entry from commits:

**Parameters**:

- `-Version` (required): Version number
- `-Date` (optional): Release date (defaults to today)

**Categories**:

- ✨ Features (`feat:`)
- 🐛 Bug Fixes (`fix:`)
- ⚡ Performance Improvements (`perf:`)
- ♻️ Code Refactoring (`refactor:`)
- 📚 Documentation (`docs:`)
- ⚠️ BREAKING CHANGES

### New-Release.ps1

Creates Git tag and GitHub Release with artifacts:

**Parameters**:

- `-Version` (required): Version to release
- `-PublishPath` (optional): Path to artifacts (default: `publish/`)
- `-DryRun` (optional): Preview without creating release

**Requirements**:

- GitHub CLI (`gh`) installed and authenticated

**Example**:

```powershell
# Preview release
.\scripts\New-Release.ps1 -Version "0.2.0" -DryRun

# Create release
.\scripts\New-Release.ps1 -Version "0.2.0"
```

## 🔧 Requirements

- **PowerShell 5.1+** (included with Windows)
- **Git** for version control
- **GitHub CLI** (`gh`) for creating releases
  - Install: `winget install GitHub.cli`
  - Authenticate: `gh auth login`

## 📝 Conventional Commits Reference

```bash
# Version bumps
feat: add new feature        # → MINOR (0.1.0 → 0.2.0)
fix: resolve bug             # → PATCH (0.1.0 → 0.1.1)
feat!: breaking change       # → MAJOR (0.1.0 → 1.0.0)

# No release
docs: update README          # → No bump
chore: update dependencies   # → No bump
style: format code           # → No bump
refactor: restructure        # → No bump
test: add tests              # → No bump
ci: update workflows         # → No bump
```

## 🎯 Best Practices

1. **Write descriptive commits**: `feat(catalog): add search filter`
2. **Use scopes**: `fix(queue): prevent duplicate items`
3. **Document breaking changes**:

   ```
   feat!: redesign API

   BREAKING CHANGE: Method signature changed
   ```

4. **Keep commits atomic**: One logical change per commit
5. **Let automation work**: Don't manually edit VERSION.txt or create tags

## 🐛 Troubleshooting

### "GitHub CLI not found"

```powershell
winget install GitHub.cli
gh auth login
```

### "Failed to push tag"

Ensure you have write access to the repository and are authenticated.

### "No version-affecting commits"

Only `feat`, `fix`, `perf`, and `BREAKING CHANGE` trigger releases.
Commits like `docs:`, `chore:`, etc. don't create releases.

## 📚 Additional Resources

- [Conventional Commits](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/)
- [GitHub CLI Documentation](https://cli.github.com/manual/)
