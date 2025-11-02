# Scripts

This directory contains utility scripts for the project.

## Version Management

### Get-NextVersion.ps1

Determines the next version number based on conventional commits.

**Usage:**
```powershell
./scripts/Get-NextVersion.ps1 -BaseVersion "1.4.0" -CommitMessages @("feat: add new feature", "fix: bug fix")
# Returns: 1.5.0 (minor bump due to feat:)
```

**Conventional Commit Rules:**
- `feat:` → minor bump (1.4.0 → 1.5.0)
- `fix:` → patch bump (1.4.0 → 1.4.1)
- `feat!:` or `BREAKING CHANGE:` → major bump (1.4.0 → 2.0.0)
- Other types → patch bump

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

Tests the version calculation logic with various scenarios.

**Usage:**
```powershell
./scripts/Test-VersionLogic.ps1
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
