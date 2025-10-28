# Test Coverage Script

## Generate-TestCoverageReport.ps1

This script analyzes which cmdlets in the repository have test coverage by:

1. **Discovering cmdlets**: Scans all `*Cmdlet.cs` files in the `Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands` directory
2. **Running tests**: Executes the Pester test suite
3. **Matching tests to cmdlets**: Determines which cmdlets are covered by checking if test names/paths contain the cmdlet name
4. **Generating reports**: Creates markdown and JSON reports showing coverage statistics

### Usage

```powershell
# Basic usage (runs tests and generates reports)
./scripts/Generate-TestCoverageReport.ps1

# With custom paths
./scripts/Generate-TestCoverageReport.ps1 `
    -TestPath "tests" `
    -CmdletsPath "Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands" `
    -OutputMarkdown "coverage/report.md" `
    -OutputJson "coverage/coverage.json"

# With baseline comparison
./scripts/Generate-TestCoverageReport.ps1 `
    -BaselineJson "coverage/baseline-coverage.json"
```

### Output

The script generates:

1. **Markdown Report** (`coverage/test-coverage-report.md`):
   - Summary statistics (cmdlet coverage %, test counts)
   - Table of tested cmdlets with test counts and status
   - Table of untested cmdlets
   - Delta comparison with baseline (if provided)

2. **JSON Data** (`coverage/test-coverage.json`):
   - Machine-readable coverage data
   - Can be used as baseline for future comparisons

### CI Integration

The script is integrated into the GitHub Actions workflow (`.github/workflows/publish.yml`) and runs automatically on pull requests:

1. Builds and tests the PR branch
2. Generates baseline coverage from the base branch (if possible)
3. Compares PR coverage with baseline
4. Posts results as a PR comment with delta indicators:
   - 📈 Coverage increased
   - 📉 Coverage decreased
   - ➡️ Coverage unchanged

### Coverage Calculation

**Cmdlet coverage** is calculated as:
```
Coverage % = (Cmdlets with tests / Total cmdlets) × 100
```

A cmdlet is considered "tested" if any test name or test file path contains the cmdlet name (e.g., tests for `Get-DataverseRecord` would be in `Get-DataverseRecord.ps1` or have test names containing "Get-DataverseRecord").

### Notes

- This measures **cmdlet-level coverage**, not line-level code coverage
- All 383+ cmdlets are analyzed, including auto-generated SDK cmdlets
- Tests may cover multiple cmdlets if they use several cmdlets in combination
- The script does not require code instrumentation or additional tools beyond Pester
