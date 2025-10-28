# Test Coverage Gap Analysis Report

**Date:** October 28, 2025  
**Module:** Rnwood.Dataverse.Data.PowerShell  
**Current Test Status:** 271 tests passing, 4 skipped

## Executive Summary

This report systematically analyzes all documented features in the module and identifies gaps in test coverage. The analysis focuses on **high-value end-to-end tests** that can work within FakeXrmEasy mock limitations and metadata constraints (primarily the `contact` entity from `tests/contact.xml`).

### Overall Statistics

- **Total Documented Cmdlets (non-SDK):** 21
- **Total Documented Features:** ~129
- **Cmdlets with Test Coverage:** 8
- **Cmdlets without Any Tests:** 13
- **Existing Test Files:** 19
- **Current Test Count:** 271 passing tests

## Test Coverage by Cmdlet

### ✅ Well-Tested Cmdlets (Good Coverage)

#### 1. Get-DataverseRecord
**Test Files:** 5 files (Get-DataverseRecord.ps1, Get-DataverseRecord-Basic.ps1, Get-DataverseRecord-Links.ps1, Get-DataverseRecord-MatchOn.ps1, Get-DataverseRecord-Cancellation.ps1)  
**Estimated Tests:** ~60+ tests

**Features with Good Coverage:**
- ✅ Basic query all records
- ✅ Filter with equals (simple)
- ✅ Filter with explicit operators (Like, NotNull, etc.)
- ✅ Filter with AND/OR/NOT/XOR logic
- ✅ ExcludeFilterValues
- ✅ Top limit and paging
- ✅ OrderBy (ascending/descending)
- ✅ Columns selection
- ✅ Lookup type conversion
- ✅ OptionSet type conversion
- ✅ Links parameter (join related tables)
- ✅ MatchOn parameter
- ✅ Cancellation support (StopProcessing)

**Missing High-Value Tests:**
- ❌ **FetchXml queries** - High priority, works with mock
- ❌ **TotalRecordCount switch** - Medium priority
- ❌ **VerboseRecordCount switch** - Low priority
- ❌ **LookupValuesReturnName flag** - Medium priority
- ❌ **Column:Raw and Column:Display syntax** - Medium priority
- ❌ **IncludeSystemColumns flag** - Low priority
- ❌ **Id parameter (list of Ids)** - High priority
- ❌ **Name parameter (list of names)** - High priority
- ❌ **ExcludeId parameter** - Medium priority
- ❌ **Criteria parameter (FilterExpression SDK object)** - Medium priority

#### 2. Set-DataverseRecord
**Test Files:** 6 files (Set-DataverseRecord.ps1, Set-DataverseRecord-Parallel.ps1, etc.)  
**Estimated Tests:** ~70+ tests

**Features with Good Coverage:**
- ✅ Create single/multiple records
- ✅ Update by Id
- ✅ Update by MatchOn
- ✅ Upsert with MatchOn
- ✅ CreateOnly flag
- ✅ PassThru flag
- ✅ BatchSize
- ✅ MaxDegreeOfParallelism
- ✅ Retries and InitialRetryDelay
- ✅ Error handling in batch operations
- ✅ Lookup conversion (GUID)
- ✅ OptionSet conversion

**Missing High-Value Tests:**
- ❌ **Upsert with alternate keys (-Upsert flag)** - High priority, works with mock if alternate key defined
- ❌ **NoUpdate flag** - Medium priority
- ❌ **NoCreate flag** - Medium priority
- ❌ **NoUpdateColumns parameter** - High priority, verifies specific columns excluded
- ❌ **LookupColumns parameter** - High priority for lookup resolution control
- ❌ **UpdateAllColumns parameter** - Medium priority
- ❌ **IgnoreProperties parameter** - Medium priority
- ❌ **Lookup conversion by name (string)** - High priority, requires name lookup
- ❌ **OptionSet conversion by label (string)** - High priority
- ❌ **MultiSelectPicklist handling** - Medium priority
- ❌ **DateTime conversion and timezone handling** - Medium priority
- ❌ **Money type conversion** - Medium priority
- ❌ **PartyList type handling** - Low priority (complex entity)
- ❌ **ownerid assignment** - High priority
- ❌ **statuscode/statecode changes** - High priority
- ❌ **BypassBusinessLogicExecutionStepIds** - Low priority
- ❌ **CallerId (delegation)** - Medium priority

#### 3. Remove-DataverseRecord
**Test Files:** 2 files (Remove-DataverseRecord.ps1, Remove-DataverseRecord-Parallel-Retries.ps1)  
**Estimated Tests:** ~25+ tests

**Features with Good Coverage:**
- ✅ Delete by Id
- ✅ Delete from pipeline
- ✅ BatchSize
- ✅ MaxDegreeOfParallelism
- ✅ Retries with parallelism

**Missing High-Value Tests:**
- ❌ **IfExists flag** - High priority
- ❌ **WhatIf support** - High priority (ShouldProcess)
- ❌ **Confirm support** - High priority (ShouldProcess)
- ❌ **Error handling when record doesn't exist** - Medium priority

#### 4. Get-DataverseConnection
**Test Files:** 1 file (Get-DataverseConnection.ps1)  
**Estimated Tests:** ~15 tests

**Features with Good Coverage:**
- ✅ Mock connection
- ✅ Default connection
- ✅ Named connections (basic save/load)
- ✅ Certificate authentication (basic)

**Missing High-Value Tests:**
- ❌ **Interactive authentication** - Cannot test in CI (requires user interaction)
- ❌ **Device code authentication** - Cannot test in CI (requires user interaction)
- ❌ **Username/password authentication** - Cannot test without real environment
- ❌ **Client secret authentication** - Cannot test without real environment
- ❌ **Named connections: list** - Medium priority
- ❌ **Named connections: delete** - Medium priority
- ❌ **Named connections: clear all** - Medium priority
- ❌ **SaveCredentials flag** - Low priority (security-sensitive)

#### 5. Set-DataverseConnectionAsDefault
**Test Files:** 1 file (Set-DataverseConnectionAsDefault.ps1)  
**Estimated Tests:** ~4 tests

**Features Tested:**
- ✅ Set default connection
- ✅ Get default connection
- ✅ Cmdlets use default when -Connection not provided
- ✅ Error when no default set

**Coverage:** Good ✅

#### 6. Invoke-DataverseRequest
**Test Files:** 1 file (Invoke-DataverseRequest.ps1)  
**Estimated Tests:** ~8 tests

**Features with Good Coverage:**
- ✅ Execute single request
- ✅ Execute multiple requests (batching)
- ✅ PassThru flag

**Missing High-Value Tests:**
- ❌ **BypassBusinessLogicExecutionStepIds** - Low priority
- ❌ **More complex SDK request types** - Medium priority

#### 7. Invoke-DataverseParallel
**Test Files:** 1 file (Invoke-DataverseParallel.ps1)  
**Estimated Tests:** ~6 tests

**Features Tested:**
- ✅ Parallel execution of ScriptBlock
- ✅ ChunkSize parameter
- ✅ MaxDegreeOfParallelism
- ✅ Connection cloning

**Missing Tests:**
- ❌ **Works on PS 5.1 and PS 7+ verification** - Low priority (CI tests both)

#### 8. Get-DataverseSolutionFile
**Test Files:** 1 file (Get-DataverseSolutionFile.ps1)  
**Estimated Tests:** ~4 tests

**Features Tested:**
- ✅ Parse solution ZIP file (from file path)
- ✅ Extract metadata without connection

**Missing Tests:**
- ❌ **Parse from bytes (pipeline input)** - Currently skipped in tests

---

### ❌ Untested Cmdlets (No Coverage)

#### 9. Invoke-DataverseSql
**Test Files:** None  
**Priority:** 🔴 HIGH

**All Features Untested:**
- ❌ SELECT queries
- ❌ INSERT statements
- ❌ UPDATE statements
- ❌ DELETE statements
- ❌ Parameterized queries
- ❌ Pipeline parameterization
- ❌ ReturnEntityReferenceAsGuid
- ❌ BypassCustomPluginExecution
- ❌ UseBulkDelete
- ❌ UseLocalTimezone
- ❌ BatchSize
- ❌ MaxDegreeOfParallelism
- ❌ Timeout
- ❌ WhatIf/Confirm support

**Recommended High-Value Tests:**
1. **SELECT with simple WHERE clause** - Returns PSObjects with correct properties
2. **SELECT with JOIN** - Joins work correctly with mock data
3. **INSERT with OUTPUT** - Returns created record ID
4. **UPDATE with parameterization** - Updates correct records
5. **DELETE with WhatIf** - Shows what would be deleted without deleting
6. **Parameterized query from pipeline** - Executes once per pipeline object
7. **ReturnEntityReferenceAsGuid flag** - Returns GUIDs instead of EntityReference objects

#### 10. Get-DataverseWhoAmI
**Test Files:** None  
**Priority:** 🟡 MEDIUM

**All Features Untested:**
- ❌ Returns current user info (UserId, BusinessUnitId, OrganizationId)

**Recommended Test:**
1. **Returns WhoAmI response** - Validates response structure with mock

#### 11. Get-DataverseRecordsFolder / Set-DataverseRecordsFolder
**Test Files:** None  
**Priority:** 🟡 MEDIUM

**All Features Untested:**
- ❌ Read records from JSON files in folder
- ❌ Write records to JSON files in folder

**Recommended High-Value Tests:**
1. **Read from folder with multiple JSON files** - Correctly deserializes all records
2. **Write to folder creates one file per record** - Proper JSON serialization
3. **Round-trip test** - Write then read produces same data

#### 12. Export-DataverseSolution
**Test Files:** None  
**Priority:** 🔴 HIGH (but requires real Dataverse)

**All Features Untested:**
- ❌ Export to file
- ❌ Export managed
- ❌ PassThru flag
- ❌ Include settings

**Mock Limitation:** Requires real Dataverse connection. FakeXrmEasy may not fully support solution export.

**Recommended Tests (if mock supports):**
1. **Export unmanaged solution** - Creates valid ZIP file
2. **Export with PassThru** - Returns byte array
3. **Export managed vs unmanaged** - Different content/structure

#### 13. Import-DataverseSolution
**Test Files:** None  
**Priority:** 🔴 HIGH (but requires real Dataverse)

**All Features Untested:**
- ❌ Auto mode (intelligent import)
- ❌ NoUpgrade mode
- ❌ StageAndUpgrade mode
- ❌ HoldingSolution mode
- ❌ UseUpdateIfAdditive (experimental)
- ❌ ConnectionReferences
- ❌ EnvironmentVariables
- ❌ SkipConnectionReferenceValidation
- ❌ SkipEnvironmentVariableValidation

**Mock Limitation:** Requires real Dataverse. FakeXrmEasy may not support solution import.

**Recommended Tests (if mock supports):**
1. **Import new solution** - Solution appears in environment
2. **Import with ConnectionReferences** - Validates and sets connection references
3. **Import with EnvironmentVariables** - Sets variables correctly
4. **Import validation errors** - Missing connection refs throw expected errors

#### 14. Get-DataverseSolution
**Test Files:** None  
**Priority:** 🟡 MEDIUM

**All Features Untested:**
- ❌ List all solutions
- ❌ Filter by managed status
- ❌ Filter by name
- ❌ Exclude system solutions

**Mock Limitation:** Requires solution metadata in mock. May need to add solution entity to contact.xml.

**Recommended High-Value Tests:**
1. **List all solutions** - Returns solution records with correct properties
2. **Filter by managed/unmanaged** - Filters correctly
3. **Get by UniqueName** - Returns specific solution

#### 15. Set-DataverseSolution
**Test Files:** None  
**Priority:** 🟡 MEDIUM

**All Features Untested:**
- ❌ Create new solution
- ❌ Update existing solution
- ❌ Set version, description, publisher

**Mock Limitation:** May require publisher and solution metadata.

**Recommended High-Value Tests:**
1. **Create new solution** - Solution record created with correct properties
2. **Update solution properties** - Description/version updated
3. **Error when publisher not found** - Proper error handling

#### 16. Remove-DataverseSolution
**Test Files:** None  
**Priority:** 🟢 LOW (destructive, complex async operation)

**All Features Untested:**
- ❌ Uninstall solution
- ❌ Progress monitoring
- ❌ Custom timeout

**Mock Limitation:** Async operation, may not be fully supported by FakeXrmEasy.

#### 17. Publish-DataverseCustomizations
**Test Files:** None  
**Priority:** 🟢 LOW

**All Features Untested:**
- ❌ Publish all customizations
- ❌ Publish specific entity

**Mock Limitation:** Publishing is a platform operation that may not work with mock.

#### 18. Get-DataverseSolutionComponent
**Test Files:** None  
**Priority:** 🟢 LOW (experimental, incomplete)

**All Features Untested:**
- ❌ List components from environment
- ❌ IncludeSubcomponents

#### 19. Get-DataverseSolutionFileComponent
**Test Files:** None  
**Priority:** 🟢 LOW (experimental, incomplete)

**All Features Untested:**
- ❌ Extract components from file

#### 20. Compare-DataverseSolutionComponents
**Test Files:** None  
**Priority:** 🟢 LOW (experimental, incomplete)

**All Features Untested:**
- ❌ Compare file with environment
- ❌ Compare two files
- ❌ ReverseComparison

---

## High-Priority Test Recommendations

### Priority 1: Critical Gaps in Core Cmdlets

These tests cover frequently-used features that are currently untested:

1. **Get-DataverseRecord: FetchXml Queries** ⭐⭐⭐
   - **Test:** Execute FetchXml query with filter, ordering, and linked entities
   - **Validates:** FetchXml parsing and execution works correctly
   - **Mock compatible:** Yes
   - **Estimated effort:** 2-3 tests

2. **Get-DataverseRecord: Id/Name/ExcludeId Parameters** ⭐⭐⭐
   - **Test:** Retrieve specific records by list of Ids
   - **Test:** Retrieve records by list of names (primary attribute)
   - **Test:** Exclude specific Ids from results
   - **Validates:** Alternative query methods work correctly
   - **Mock compatible:** Yes
   - **Estimated effort:** 3 tests

3. **Set-DataverseRecord: NoUpdateColumns** ⭐⭐⭐
   - **Test:** Update record but exclude specific columns from update
   - **Validates:** Selective column updates work, excluded columns unchanged
   - **Mock compatible:** Yes
   - **Estimated effort:** 2 tests

4. **Set-DataverseRecord: Lookup Resolution by Name** ⭐⭐⭐
   - **Test:** Set lookup field using name string (unique lookup)
   - **Test:** Set lookup with LookupColumns to control resolution
   - **Validates:** Name-based lookup resolution and LookupColumns parameter
   - **Mock compatible:** Yes (needs related records in mock)
   - **Estimated effort:** 3-4 tests

5. **Set-DataverseRecord: OptionSet by Label** ⭐⭐⭐
   - **Test:** Set choice field using label string instead of numeric value
   - **Validates:** Metadata-based label-to-value conversion
   - **Mock compatible:** Yes (uses metadata from contact.xml)
   - **Estimated effort:** 2 tests

6. **Set-DataverseRecord: ownerid Assignment** ⭐⭐⭐
   - **Test:** Create record with ownerid, verify AssignRequest issued
   - **Test:** Update ownerid, verify assignment executed
   - **Validates:** Ownership assignment after create/update
   - **Mock compatible:** Yes (may need systemuser in metadata)
   - **Estimated effort:** 2-3 tests

7. **Set-DataverseRecord: statuscode/statecode Changes** ⭐⭐⭐
   - **Test:** Set statuscode on record, verify SetStateRequest issued
   - **Test:** Set both statecode and statuscode
   - **Validates:** State transitions handled correctly
   - **Mock compatible:** Yes (uses metadata from contact.xml)
   - **Estimated effort:** 2-3 tests

8. **Remove-DataverseRecord: IfExists Flag** ⭐⭐⭐
   - **Test:** Delete non-existent record with IfExists, no error
   - **Test:** Delete without IfExists on non-existent record, error thrown
   - **Validates:** Graceful handling of missing records
   - **Mock compatible:** Yes
   - **Estimated effort:** 2 tests

9. **Remove-DataverseRecord: WhatIf/Confirm Support** ⭐⭐⭐
   - **Test:** Delete with -WhatIf, verify no records deleted
   - **Test:** Delete with -Confirm:$false, records deleted
   - **Validates:** ShouldProcess implementation
   - **Mock compatible:** Yes
   - **Estimated effort:** 2 tests

10. **Invoke-DataverseSql: Core SQL Operations** ⭐⭐⭐
    - **Test:** SELECT query returns correct PSObjects
    - **Test:** INSERT statement creates record
    - **Test:** UPDATE with parameterization
    - **Test:** DELETE with WhatIf
    - **Test:** Parameterized query from pipeline
    - **Validates:** SQL4Cds integration works correctly
    - **Mock compatible:** Depends on MarkMpn.Sql4Cds.Engine mock support
    - **Estimated effort:** 5-7 tests

### Priority 2: Important Feature Gaps

11. **Get-DataverseRecord: LookupValuesReturnName** ⭐⭐
    - **Test:** Query with flag, lookups return names instead of objects
    - **Validates:** Lookup output format control
    - **Mock compatible:** Yes
    - **Estimated effort:** 2 tests

12. **Get-DataverseRecord: Column:Raw and :Display** ⭐⭐
    - **Test:** Request column with :Raw suffix, get raw value
    - **Test:** Request column with :Display suffix, get formatted value
    - **Validates:** Per-column output format control
    - **Mock compatible:** Yes
    - **Estimated effort:** 2 tests

13. **Set-DataverseRecord: Upsert with Alternate Keys** ⭐⭐
    - **Test:** Use -Upsert flag with alternate key defined on table
    - **Validates:** Platform UpsertRequest usage
    - **Mock compatible:** If FakeXrmEasy supports alternate keys
    - **Estimated effort:** 2-3 tests

14. **Set-DataverseRecord: NoUpdate/NoCreate Flags** ⭐⭐
    - **Test:** Use -NoUpdate, only creates not updates
    - **Test:** Use -NoCreate, only updates not creates
    - **Validates:** Operation mode control
    - **Mock compatible:** Yes
    - **Estimated effort:** 2 tests

15. **Get-DataverseWhoAmI** ⭐⭐
    - **Test:** Returns WhoAmI response with UserId, BusinessUnitId, OrganizationId
    - **Validates:** Identity information retrieval
    - **Mock compatible:** Yes
    - **Estimated effort:** 1 test

16. **Get/Set-DataverseRecordsFolder** ⭐⭐
    - **Test:** Write records to folder, read back, verify round-trip
    - **Validates:** JSON serialization/deserialization
    - **Mock compatible:** Yes (filesystem operation)
    - **Estimated effort:** 3-4 tests

### Priority 3: Advanced Feature Gaps

17. **Set-DataverseRecord: Type Conversions** ⭐
    - **Test:** MultiSelectPicklist (array of labels/values)
    - **Test:** DateTime with timezone handling
    - **Test:** Money type
    - **Validates:** Complex type conversions work correctly
    - **Mock compatible:** Yes
    - **Estimated effort:** 3-4 tests

18. **Set-DataverseRecord: Advanced Parameters** ⭐
    - **Test:** UpdateAllColumns skips retrieve step
    - **Test:** IgnoreProperties excludes specified properties
    - **Test:** CallerId delegation
    - **Validates:** Advanced control parameters
    - **Mock compatible:** Partial (some may require specific mock setup)
    - **Estimated effort:** 3-4 tests

19. **Get-DataverseRecord: TotalRecordCount** ⭐
    - **Test:** Use -TotalRecordCount, returns count not records
    - **Validates:** Count-only query mode
    - **Mock compatible:** Yes
    - **Estimated effort:** 1 test

---

## Recommended Test Implementation Plan

### Phase 1: Core CRUD Gaps (High Impact)
**Estimated Effort:** 20-25 tests, 4-6 hours

- Get-DataverseRecord: FetchXml queries
- Get-DataverseRecord: Id/Name/ExcludeId parameters
- Set-DataverseRecord: NoUpdateColumns
- Set-DataverseRecord: Lookup by name and LookupColumns
- Set-DataverseRecord: OptionSet by label
- Set-DataverseRecord: ownerid assignment
- Set-DataverseRecord: statuscode/statecode
- Remove-DataverseRecord: IfExists flag
- Remove-DataverseRecord: WhatIf/Confirm

### Phase 2: SQL and Important Features
**Estimated Effort:** 15-20 tests, 4-5 hours

- Invoke-DataverseSql: All core SQL operations
- Get-DataverseRecord: LookupValuesReturnName and column format
- Set-DataverseRecord: NoUpdate/NoCreate flags
- Get-DataverseWhoAmI
- Get/Set-DataverseRecordsFolder

### Phase 3: Advanced Features
**Estimated Effort:** 10-15 tests, 3-4 hours

- Set-DataverseRecord: Type conversions (MultiSelectPicklist, DateTime, Money)
- Set-DataverseRecord: Advanced parameters
- Get-DataverseRecord: TotalRecordCount
- Set-DataverseRecord: Upsert with alternate keys (if supported)

### Phase 4: Solution Management (Lower Priority, Mock Limitations)
**Note:** Many solution cmdlets may not work well with FakeXrmEasy mock

- Get-DataverseSolution (if metadata can be added)
- Set-DataverseSolution (if supported)
- Export/Import (likely requires E2E tests with real environment)

---

## Mock Limitations and Workarounds

### Current Metadata
- **Primary Entity:** `contact` (fully defined in tests/contact.xml)
- **Related Entities:** Limited (may need to add account, systemuser, etc.)

### Adding New Entities for Tests
If tests require entities beyond `contact`:
1. Add entity metadata to `tests/contact.xml` (or create new metadata file)
2. Or create SDK Entity objects directly in tests (without full metadata)

### FakeXrmEasy Limitations
Some features may not work with FakeXrmEasy v3.x:
- Solution import/export (async operations)
- Complex metadata operations
- Some specialized SDK requests
- Alternate keys support (may be limited)

**Workaround:** Focus on E2E tests for these features (e2e-tests/ directory with real environment).

---

## Test Quality Guidelines

For all new tests, follow these principles:

1. **Full End-to-End Validation**
   - Create records, query them, verify exact results
   - Don't just check that cmdlet doesn't throw
   - Verify data correctness, not just data shape

2. **Assert No Side Effects**
   - Verify only expected records are created/updated/deleted
   - Check that unrelated records are not affected
   - Validate that excluded columns remain unchanged

3. **Test Edge Cases**
   - Empty results
   - Multiple matches
   - Non-existent records
   - Invalid input data

4. **Realistic Scenarios**
   - Use meaningful test data
   - Test common usage patterns from documentation
   - Include examples from docs as tests

5. **Clear Test Names**
   - Describe what's being tested, not how
   - Include expected outcome in test name
   - Example: "Updates record with NoUpdateColumns excludes specified columns"

---

## Conclusion

This analysis identifies **significant test gaps** in the module:
- **13 cmdlets have no tests** at all
- **Core cmdlets are missing tests** for many documented features
- **Most untested features can be tested** with the current mock setup

**Recommended Action:** Implement **Phase 1 tests first** (20-25 tests) to cover the most critical gaps in core CRUD operations that users rely on daily. These tests will:
- Improve confidence in frequently-used features
- Catch regressions in critical functionality  
- Work within current mock limitations
- Provide high value with reasonable effort

**Total Recommended Tests:** 45-60 new high-value tests across 3 phases.
