# Test Coverage - Quick Reference

**See [TEST_COVERAGE_GAP_ANALYSIS.md](TEST_COVERAGE_GAP_ANALYSIS.md) for the full detailed report.**

## At a Glance

| Metric | Value |
|--------|-------|
| Total Cmdlets (non-SDK) | 21 |
| Documented Features | ~129 |
| Current Tests | 271 passing |
| Cmdlets with Tests | 8 |
| **Cmdlets with NO Tests** | **13** ❌ |
| Recommended New Tests | 45-60 |

## Test Coverage Status

### ✅ Well Tested (Good Coverage)
1. **Get-DataverseRecord** - 60+ tests across 5 files
2. **Set-DataverseRecord** - 70+ tests across 6 files
3. **Remove-DataverseRecord** - 25+ tests across 2 files
4. **Get-DataverseConnection** - 15+ tests
5. **Set-DataverseConnectionAsDefault** - 4 tests ✅
6. **Invoke-DataverseRequest** - 8 tests
7. **Invoke-DataverseParallel** - 6 tests
8. **Get-DataverseSolutionFile** - 4 tests

### ❌ Completely Untested (No Tests)
1. **Invoke-DataverseSql** 🔴 HIGH PRIORITY
2. **Get-DataverseWhoAmI** 🟡 MEDIUM PRIORITY
3. **Get-DataverseRecordsFolder** 🟡 MEDIUM PRIORITY
4. **Set-DataverseRecordsFolder** 🟡 MEDIUM PRIORITY
5. **Export-DataverseSolution** 🔴 (requires real Dataverse)
6. **Import-DataverseSolution** 🔴 (requires real Dataverse)
7. **Get-DataverseSolution** 🟡
8. **Set-DataverseSolution** 🟡
9. **Remove-DataverseSolution** 🟢 LOW
10. **Publish-DataverseCustomizations** 🟢 LOW
11. **Get-DataverseSolutionComponent** 🟢 LOW (experimental)
12. **Get-DataverseSolutionFileComponent** 🟢 LOW (experimental)
13. **Compare-DataverseSolutionComponents** 🟢 LOW (experimental)

## Top 10 Missing Tests (High Impact)

| Rank | Cmdlet | Feature | Impact | Effort | Mock Works? |
|------|--------|---------|--------|--------|-------------|
| 1 | Invoke-DataverseSql | SELECT/INSERT/UPDATE/DELETE | ⭐⭐⭐ | 5-7 tests | ✅ |
| 2 | Get-DataverseRecord | FetchXml queries | ⭐⭐⭐ | 2-3 tests | ✅ |
| 3 | Get-DataverseRecord | Id/Name/ExcludeId params | ⭐⭐⭐ | 3 tests | ✅ |
| 4 | Set-DataverseRecord | NoUpdateColumns | ⭐⭐⭐ | 2 tests | ✅ |
| 5 | Set-DataverseRecord | Lookup by name + LookupColumns | ⭐⭐⭐ | 3-4 tests | ✅ |
| 6 | Set-DataverseRecord | OptionSet by label | ⭐⭐⭐ | 2 tests | ✅ |
| 7 | Set-DataverseRecord | ownerid assignment | ⭐⭐⭐ | 2-3 tests | ✅ |
| 8 | Set-DataverseRecord | statuscode/statecode | ⭐⭐⭐ | 2-3 tests | ✅ |
| 9 | Remove-DataverseRecord | IfExists flag | ⭐⭐⭐ | 2 tests | ✅ |
| 10 | Remove-DataverseRecord | WhatIf/Confirm | ⭐⭐⭐ | 2 tests | ✅ |

**Total Phase 1 Tests:** 20-25 tests | **Estimated Effort:** 4-6 hours

## Implementation Phases

### Phase 1: Core CRUD Gaps (Critical) 🔴
**Tests:** 20-25 | **Effort:** 4-6 hours | **Priority:** HIGH

Focus: Tests #1-10 above - Core functionality used daily by users

### Phase 2: Important Features 🟡
**Tests:** 15-20 | **Effort:** 4-5 hours | **Priority:** MEDIUM

- Invoke-DataverseSql parameters (BatchSize, MaxDOP, etc.)
- Get-DataverseWhoAmI
- Get/Set-DataverseRecordsFolder
- Get-DataverseRecord formatting options (LookupValuesReturnName, :Raw/:Display)
- Set-DataverseRecord NoUpdate/NoCreate flags

### Phase 3: Advanced Features 🟢
**Tests:** 10-15 | **Effort:** 3-4 hours | **Priority:** LOW

- Complex type conversions (MultiSelectPicklist, Money, DateTime)
- Advanced parameters (UpdateAllColumns, IgnoreProperties, CallerId)
- TotalRecordCount switch
- Upsert with alternate keys

### Phase 4: Solution Management
**Note:** Most require real Dataverse (E2E tests), not unit tests

## Quick Test Template

```powershell
Describe 'CmdletName - FeatureName' {
    It "Should [expected behavior]" {
        # Arrange
        $connection = getMockConnection
        # Create test data if needed
        
        # Act
        $result = Invoke-Cmdlet -Connection $connection -Param1 $value1
        
        # Assert
        $result | Should -Not -BeNullOrEmpty
        $result.Property | Should -Be $expectedValue
        
        # Verify no side effects
        $allRecords = Get-DataverseRecord -Connection $connection -TableName table
        $allRecords | Should -HaveCount $expectedCount
    }
}
```

## Key Testing Principles

1. ✅ **Full E2E validation** - Create, query, verify exact results
2. ✅ **Assert no side effects** - Check unrelated records unchanged
3. ✅ **Test edge cases** - Empty results, multiple matches, non-existent records
4. ✅ **Realistic scenarios** - Use examples from documentation
5. ✅ **Clear test names** - Describe expected outcome

## Mock Limitations

### Works Well
- ✅ CRUD operations (contact entity)
- ✅ Filters, joins, ordering
- ✅ Type conversions with metadata
- ✅ Batch operations
- ✅ Error handling

### Limited/May Not Work
- ❌ Solution import/export (async)
- ❌ Complex metadata operations
- ❌ Alternate keys (may be limited)
- ❌ Some specialized SDK requests

**Workaround:** Use E2E tests in `e2e-tests/` directory for features requiring real Dataverse.

## Quick Links

- **Full Report:** [TEST_COVERAGE_GAP_ANALYSIS.md](TEST_COVERAGE_GAP_ANALYSIS.md)
- **Test Directory:** `tests/`
- **Mock Metadata:** `tests/contact.xml`
- **E2E Tests:** `e2e-tests/`

## How to Run Tests

```bash
# Build first
dotnet build

# Set module path
export TESTMODULEPATH=$(pwd)/Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0

# Run all tests (PowerShell 7+)
# IMPORTANT: Must use All.Tests.ps1 as entry point
pwsh -Command "
  \$config = New-PesterConfiguration
  \$config.Run.Path = 'tests/All.Tests.ps1'
  \$config.Output.Verbosity = 'Normal'
  Invoke-Pester -Configuration \$config
"

# Note: Individual test files cannot be run directly
# They depend on setup from All.Tests.ps1 (getMockConnection, etc.)
```

---

**Created:** October 28, 2025  
**Last Updated:** October 28, 2025  
**Status:** Analysis Complete ✅
