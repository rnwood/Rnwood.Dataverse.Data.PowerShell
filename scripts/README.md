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

## Coverage Scripts

## Run-TestsWithCoverage.ps1

Runs tests with code coverage using coverlet.console to instrument the cmdlets DLL. This provides line-level and branch-level coverage for C# cmdlet code.

### How It Works

1. **Build**: Builds the project without instrumentation
2. **Copy**: Copies the built module to the `out` directory
3. **Instrument**: Uses coverlet.console to instrument the cmdlets DLL in the `out` directory
4. **Test Execution**: Runs Pester tests which load the instrumented DLL
5. **Coverage Collection**: Coverlet tracks which lines and branches are executed
6. **Report Generation**: Parses Cobertura XML to create per-cmdlet coverage report

The key insight is that tests copy the module to a temp directory, so we instrument the DLL in the `out` directory before tests run. When tests copy from `out` to temp, they copy the instrumented DLL.

### Usage

```powershell
# Basic usage (handles build and test)
./scripts/Run-TestsWithCoverage.ps1

# With custom output directory
./scripts/Run-TestsWithCoverage.ps1 -OutputDir "my-coverage"

# With baseline comparison
./scripts/Run-TestsWithCoverage.ps1 -BaselineCoverageFile "baseline.json"
```

### Requirements

- .NET SDK
- PowerShell 7+
- Pester 5.0+
- coverlet.console (auto-installed)

### Output Files

- `coverage/coverage.cobertura.xml` - Cobertura format (standard for coverage tools)
- `coverage/coverage.json` - JSON format
- `coverage/coverage-report.md` - Markdown report with per-cmdlet breakdown
- `coverage/coverage-data.json` - Simplified JSON for baseline comparison

### Coverage Metrics

- **Line Coverage**: Percentage of executable lines that were executed
- **Branch Coverage**: Percentage of decision points (if/else, switch, etc.) that were executed  
- **Per-Cmdlet**: Coverage broken down by each cmdlet class

### CI Integration

The script is integrated into `.github/workflows/publish.yml` and runs automatically on pull requests to:
1. Build the project
2. Instrument the cmdlets DLL with coverlet.console
3. Run tests (coverage tracked automatically)
4. Get baseline coverage from base branch
5. Compare and show delta
6. Post report as PR comment

### Exclusions

The following are excluded from coverage:
- `[GeneratedCode]` attributed code
- `[ExcludeFromCodeCoverage]` attributed code
- FakeXrmEasy test framework code
- Test assemblies

### Report Format

```markdown
# 📊 Code Coverage Report

## Overall Coverage
- **Line Coverage**: 45.2% (📈 +2.1% from base)
- **Branch Coverage**: 38.5%
- **Lines Covered**: 1234 / 2731

## Coverage by Cmdlet
| Cmdlet | Line Coverage | Branch Coverage | Lines |
|--------|---------------|-----------------|-------|
| ⚠️ `Get-DataverseRecord` | 65.3% | 52.1% | 234 / 358 |
| ✅ `Set-DataverseRecord` | 82.4% | 71.2% | 412 / 500 |
...
```

Indicators:
- ⚠️ Low coverage (<30%)
- ⚡ Medium coverage (30-60%)
- ✅ Good coverage (>60%)
