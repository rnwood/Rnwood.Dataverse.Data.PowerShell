# Testing Coverage Locally

## Quick Test
To run coverage analysis locally without waiting for CI:

```powershell
# 1. Build the module
dotnet build -c Release ./Rnwood.Dataverse.Data.PowerShell/Rnwood.Dataverse.Data.PowerShell.csproj

# 2. Copy to out directory
if (Test-Path "out/Rnwood.Dataverse.Data.PowerShell") {
    Remove-Item -Force -Recurse "out/Rnwood.Dataverse.Data.PowerShell"
}
Copy-Item -Recurse "Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0" "out/Rnwood.Dataverse.Data.PowerShell"

# 3. Set module path
$env:TESTMODULEPATH = (Resolve-Path "out/Rnwood.Dataverse.Data.PowerShell")

# 4. Run coverage analysis
pwsh -File scripts/Generate-TestCoverageReport.ps1
```

## View Results

After running, check:
- `coverage/test-coverage-report.md` - Human-readable markdown report
- `coverage/test-coverage.json` - Machine-readable data

## Fast Discovery Test

To quickly verify cmdlet discovery without running tests:

```powershell
$CmdletsPath = "Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands"
$cmdletFiles = Get-ChildItem -Path $CmdletsPath -Filter "*Cmdlet.cs" -Recurse

$count = 0
foreach ($file in $cmdletFiles) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match '\[Cmdlet\("(\w+)",\s*"([^"]+)"' -or 
        $content -match 'Cmdlet\(Verbs\w+\.(\w+),\s*"([^"]+)"\)') {
        $count++
    }
}

Write-Host "Found $count cmdlets"
```

Expected: ~383 cmdlets

## Expected Runtime

- **Cmdlet Discovery**: < 1 second
- **Test Execution**: 2-5 minutes (275 tests)
- **Report Generation**: < 1 second
- **Total**: ~2-5 minutes

## Troubleshooting

### Issue: "Module not found"
**Solution**: Make sure `$env:TESTMODULEPATH` is set correctly:
```powershell
$env:TESTMODULEPATH = (Resolve-Path "out/Rnwood.Dataverse.Data.PowerShell")
```

### Issue: Tests fail
**Solution**: This is fine - the script still generates coverage. Failed tests are marked in the report.

### Issue: "Pester not installed"
**Solution**: The script installs Pester automatically, but you can install manually:
```powershell
Install-Module -Force -Scope CurrentUser Pester -MinimumVersion 5.0.0
```

### Issue: Script takes too long
**Solution**: The tests take 2-5 minutes. This is normal. You can cancel with Ctrl+C if needed.
