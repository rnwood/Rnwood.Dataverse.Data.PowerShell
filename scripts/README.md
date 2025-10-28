# Coverage Scripts

## Run-TestsWithCoverage.ps1

Runs tests with code coverage by building with coverlet.msbuild instrumentation. This ensures the instrumented DLL is what gets loaded during tests.

### How It Works

1. **Build with Instrumentation**: Uses coverlet.msbuild to build the cmdlets project with coverage instrumentation baked into the DLL
2. **Test Execution**: Runs Pester tests which load and execute the instrumented DLL
3. **Coverage Collection**: Coverlet automatically tracks which lines and branches are executed
4. **Report Generation**: Parses Cobertura XML to create per-cmdlet coverage report

This approach solves the issue where tests copy the module to a temporary directory - since the DLL itself is instrumented, coverage is tracked regardless of where it's copied.

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

- .NET SDK (for coverlet.msbuild)
- PowerShell 7+
- Pester 5.0+

### Output Files

- `coverage/coverage.cobertura.xml` - Cobertura format (standard for coverage tools)
- `coverage/coverage-report.md` - Markdown report with per-cmdlet breakdown
- `coverage/coverage-data.json` - Simplified JSON for baseline comparison

### Coverage Metrics

- **Line Coverage**: Percentage of executable lines that were executed
- **Branch Coverage**: Percentage of decision points (if/else, switch, etc.) that were executed  
- **Per-Cmdlet**: Coverage broken down by each cmdlet class

### CI Integration

The script is integrated into `.github/workflows/publish.yml` and runs automatically on pull requests to:
1. Build with instrumentation using coverlet.msbuild
2. Run tests (coverage tracked automatically)
3. Get baseline coverage from base branch
4. Compare and show delta
5. Post report as PR comment

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
