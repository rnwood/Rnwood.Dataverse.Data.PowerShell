# Coverage Scripts

## Run-TestsWithCoverage.ps1

Runs tests with code coverage instrumentation using coverlet. This provides line-level and branch-level coverage for C# cmdlet code.

### How It Works

1. **Instrumentation**: Uses coverlet.console to instrument the cmdlets DLL
2. **Test Execution**: Runs Pester tests with the instrumented assembly
3. **Coverage Collection**: Coverlet tracks which lines and branches are executed
4. **Report Generation**: Parses Cobertura XML to create per-cmdlet coverage report

### Usage

```powershell
# Basic usage
./scripts/Run-TestsWithCoverage.ps1

# With custom output directory
./scripts/Run-TestsWithCoverage.ps1 -OutputDir "my-coverage"

# With baseline comparison
./scripts/Run-TestsWithCoverage.ps1 -BaselineCoverageFile "baseline.json"
```

### Requirements

- .NET SDK (for coverlet)
- PowerShell 7+
- Pester 5.0+

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
1. Build and test the PR with instrumentation
2. Get baseline coverage from base branch
3. Compare and show delta
4. Post report as PR comment

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
