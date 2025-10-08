# Code Coverage Scripts

This directory contains scripts for generating and posting code coverage reports for the Dataverse PowerShell module.

## Overview

The coverage reporting system provides cmdlet-level test coverage analysis by examining test files and tracking which cmdlets are being tested. This is particularly useful for PowerShell modules where traditional code coverage tools may not integrate well.

## Scripts

### Generate-CoverageReport.ps1

Analyzes test files to determine which cmdlets have test coverage and generates a detailed markdown report.

**Features:**
- Scans all cmdlet files in the Commands directory (excluding generated sdk/ cmdlets)
- Analyzes test files to count references to each cmdlet
- Determines coverage status: Good (✓), Partial (⚠), or None (❌)
- Generates both console output and a markdown file

**Usage:**
```powershell
# Generate coverage report (will build and run tests)
./scripts/Generate-CoverageReport.ps1

# Generate report with custom test path
./scripts/Generate-CoverageReport.ps1 -TestPath ./my-tests
```

**Output:**
- Console: Displays the full coverage report
- File: `coverage/coverage-report.md`

**Coverage Criteria:**
- **Good (✓)**: Cmdlet has a dedicated test file OR is referenced 4+ times in tests
- **Partial (⚠)**: Cmdlet is referenced 1-3 times in tests
- **None (❌)**: No test references found

### Post-CoverageComment.ps1

Posts or updates a coverage report comment on a GitHub Pull Request.

**Features:**
- Reads the generated coverage report markdown
- Posts it as a comment to a GitHub PR
- Updates existing coverage comments instead of creating duplicates
- Automatically extracts PR and repository information from GitHub Actions context

**Usage:**
```powershell
# In GitHub Actions (automatic detection)
./scripts/Post-CoverageComment.ps1 -GitHubToken $env:GITHUB_TOKEN

# Manual usage
./scripts/Post-CoverageComment.ps1 `
    -GitHubToken "ghp_xxxxxxxxxxxx" `
    -Repository "owner/repo" `
    -PullRequestNumber 123
```

**Requirements:**
- `GITHUB_TOKEN` with `write:discussion` permission (automatically available in GitHub Actions)
- Coverage report file must exist at `coverage/coverage-report.md`

## GitHub Actions Integration

The scripts are integrated into the CI/CD pipeline in `.github/workflows/publish.yml`:

1. Tests are run as normal using Pester
2. After tests complete, `Generate-CoverageReport.ps1` analyzes the test files
3. `Post-CoverageComment.ps1` posts the report to the PR
4. Coverage reports are only generated for PRs on ubuntu-latest with PowerShell latest (to avoid duplicate comments)

**Workflow excerpt:**
```yaml
- name: Generate Coverage Report
  shell: pwsh
  run: ./scripts/Generate-CoverageReport.ps1

- name: Post Coverage Comment
  shell: pwsh
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  run: ./scripts/Post-CoverageComment.ps1 -GitHubToken $env:GITHUB_TOKEN
```

## How It Works

### Coverage Detection

The system uses static analysis to determine coverage:

1. **Cmdlet Discovery**: Scans `Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands/*.cs` for cmdlet classes
2. **Command Name Conversion**: Converts class names to PowerShell command names
   - Example: `GetDataverseConnectionCmdlet` → `Get-DataverseConnection`
3. **Test Analysis**: Searches all `*.Tests.ps1` files for references to each command
4. **Reference Counting**: Uses regex to count how many times each cmdlet appears in tests
5. **Status Determination**: Assigns Good/Partial/None based on reference count

### Limitations

- **Static Analysis**: Does not measure actual code execution or line coverage
- **False Positives**: May count comments or strings that mention cmdlet names
- **False Negatives**: May miss tests that use dynamic invocation or aliases
- **Generated Cmdlets**: Cmdlets in `sdk/` folder are automatically excluded (they have `[ExcludeFromCodeCoverage]` attribute)

### Benefits

- **No Instrumentation**: Works without modifying or instrumenting the module code
- **Fast**: Runs in seconds, doesn't require special test execution
- **Clear**: Provides actionable cmdlet-level insights
- **CI-Friendly**: Integrates easily into existing GitHub Actions workflows

## Local Testing

To test the scripts locally:

```powershell
# 1. Build the module
dotnet build -c Release ./Rnwood.Dataverse.Data.PowerShell/Rnwood.Dataverse.Data.PowerShell.csproj

# 2. Set the test module path
$env:TESTMODULEPATH = (Resolve-Path "./Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0")

# 3. Generate coverage report
./scripts/Generate-CoverageReport.ps1

# 4. View the report
Get-Content ./coverage/coverage-report.md
```

## Future Enhancements

Potential improvements:

- **True Line Coverage**: Integrate with Coverlet or dotnet test for actual C# code coverage
- **Trend Tracking**: Store coverage history and show trends over time
- **Coverage Goals**: Set minimum coverage thresholds and fail builds if not met
- **Per-Method Analysis**: Extend analysis to show method-level coverage within cmdlets
- **Integration Tests**: Separate unit test coverage from integration test coverage

## Troubleshooting

**No coverage detected:**
- Ensure tests are actually calling the cmdlets
- Check that cmdlet names in tests match the expected format (Verb-Noun)
- Verify test files are in the correct location (`tests/*.Tests.ps1`)

**GitHub comment not posting:**
- Verify `GITHUB_TOKEN` has sufficient permissions
- Check that the workflow is running in a PR context
- Look for API errors in the workflow logs

**Build failures:**
- Ensure the module builds successfully before running coverage
- Check that all dependencies are restored
- Verify PowerShell version is compatible (5.1+ or 7+)
