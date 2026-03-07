# Scripts

This directory contains utility scripts for the project.

## Version Management

### Get-NextVersion.ps1

Determines the next version number based on conventional commits, with support for existing prerelease versions to avoid double-incrementing.

**Usage:**
```powershell
# Basic usage - calculate version from commits
./scripts/Get-NextVersion.ps1 -BaseVersion "1.4.0" -CommitMessages @("feat: add new feature", "fix: bug fix")
# Returns: 1.5.0 (minor bump due to feat:)

# With existing prereleases - prevents double-incrementing on main branch
./scripts/Get-NextVersion.ps1 -BaseVersion "1.4.0" -CommitMessages @("feat: add new feature") -ExistingPrereleases @("1.5.0-ci20241103001")
# Returns: 1.5.0 (uses existing prerelease version, doesn't bump to 1.6.0)
```

**Conventional Commit Rules:**
- `feat:` → minor bump (1.4.0 → 1.5.0)
- `fix:` → patch bump (1.4.0 → 1.4.1)
- `feat!:` or `BREAKING CHANGE:` → major bump (1.4.0 → 2.0.0)
- Other types → patch bump

**Prerelease Handling:**
When `-ExistingPrereleases` is provided, the script:
1. Calculates what the new version would be based on commits
2. Finds the highest existing prerelease version
3. Returns whichever is higher (prevents double-incrementing)

**Workflow Usage:**
- **PR builds**: Calculate version from PR title only (no prerelease comparison)
  - Multiple PRs may calculate the same version - this is OK
- **Main branch**: Analyze ALL commits since stable and use prerelease comparison
  - Prevents double-bumping when multiple PRs with same change type merge sequentially

**Example Scenario:**
- Stable: 1.0.0
- PR1 with `feat!:` calculates 2.0.0-ci001 (no prerelease logic)
- PR2 with `feat!:` calculates 2.0.0-ci002 (no prerelease logic) ← versions clash, OK!
- After both merge, main branch sees TWO `feat!:` commits
- Main uses prerelease logic: sees 2.0.0-ci002 already exists, stays at 2.0.0-ci003 (not 3.0.0!)

See [CONTRIBUTING.md](../CONTRIBUTING.md) for full details.

### Get-ReleaseNotes.ps1

Generates release notes from conventional commits between two git references.

**Usage:**
```powershell
# Markdown format for GitHub releases
./scripts/Get-ReleaseNotes.ps1 -FromRef "v1.4.0" -ToRef "HEAD" -Format markdown

# Text format for PowerShell Gallery
./scripts/Get-ReleaseNotes.ps1 -FromRef "v1.4.0" -ToRef "HEAD" -Format text
```

**Features:**
- Groups changes by type (Features, Bug Fixes, Breaking Changes, Other)
- Supports emoji icons for better readability in GitHub releases
- Generates both markdown and text formats
- Automatically used by CI/CD workflow for releases

**Change Categories:**
- ⚠️ **BREAKING CHANGES**: Commits with `!` or `BREAKING CHANGE:` footer
- ✨ **Features**: Commits starting with `feat:`
- 🐛 **Bug Fixes**: Commits starting with `fix:`
- 📝 **Other Changes**: Documentation, chore, refactor, etc.

### Test-VersionLogic.ps1

Tests the version calculation logic with various scenarios, including prerelease version handling.

**Usage:**
```powershell
./scripts/Test-VersionLogic.ps1
```

**Test Coverage:**
- Basic version bumps (major, minor, patch)
- Multiple commits (ensures highest bump level wins)
- Default to patch when no conventional commits found
- Prerelease version handling (ensures no double-incrementing)

### Test-WorkflowVersionCalculation.ps1

Tests the complete workflow version calculation logic including prerelease handling with real-world scenarios.

**Usage:**
```powershell
./scripts/Test-WorkflowVersionCalculation.ps1
```

**Test Scenarios:**
- First PR with breaking change after stable release
- Second PR with breaking change (should not double-increment)
- Features and fixes with existing prereleases
- Multiple prereleases with various bump types

### Test-WorkflowSimulation.ps1

Simulates the workflow Build step to validate the basic version calculation logic.

**Usage:**
```powershell
./scripts/Test-WorkflowSimulation.ps1
```

**Note:** This is a simplified test that validates basic workflow functionality.
For comprehensive prerelease version handling tests, see Test-WorkflowVersionCalculation.ps1.

### Test-ConventionalCommits.ps1

Validates that a PR description contains at least one valid conventional commit message.

**Usage:**
```powershell
# Validate a PR description
./scripts/Test-ConventionalCommits.ps1 -PRDescription "feat: add new feature"

# Returns $true if valid, $false if invalid
```

**Features:**
- Checks for at least one conventional commit in the description
- Provides detailed error messages when validation fails
- Lists all valid commit types and examples
- Used by CI/CD workflow to enforce PR requirements

**Validation Rules:**
- PR description must not be empty
- Must contain at least one line matching conventional commit format
- Format: `<type>(<scope>): <description>` where type is one of: feat, fix, docs, style, refactor, perf, test, build, ci, chore

### Test-PRValidation.ps1

Tests the PR validation logic to ensure it correctly identifies valid and invalid PR descriptions.

**Usage:**
```powershell
./scripts/Test-PRValidation.ps1
```
