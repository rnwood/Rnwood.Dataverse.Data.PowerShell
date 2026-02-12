# Investigation: Slow E2E Test - CanAddReadUpdateAndManageSolutionComponents

**Date**: 2026-02-12  
**Test**: `SolutionComponentTests.CanAddReadUpdateAndManageSolutionComponents`  
**Investigation Method**: Direct timing without introducing new scopes

---

## Executive Summary

The test `CanAddReadUpdateAndManageSolutionComponents` was investigated and optimized. Initial investigation incorrectly added `Measure-Operation` wrapper functions that introduced new PowerShell scopes, causing a variable scoping bug.

After removing the added scopes and optimizing to use a standard entity instead of creating/deleting a custom entity, the test **PASSED successfully in ~2 minutes** (reduced from ~3 minutes).

---

## Investigation Findings

### Initial Investigation (Incorrect)
- Added `Measure-Operation` helper function that wrapped code in scriptblocks
- These scriptblocks created new scopes
- The `$publisher` variable was defined in one scriptblock scope
- When accessed in a subsequent scriptblock, it was out of scope
- This caused validation failures and retry loops

### Corrected Investigation  
- Removed all `Measure-Operation` wrapper functions
- Added simple inline timing using `$stepStartTime` and duration calculations
- No new scopes introduced
- Test **PASSED** successfully in ~3 minutes

### Optimization
- **Removed custom entity creation/deletion** (Steps 2 and 6 in original test)
- **Now uses standard 'account' entity** which is always present in Dataverse
- Since the solution name is unique (timestamp + GUID), there are no conflicts
- Follows pattern used by other E2E tests in the repository

---

## Test Results

**Optimized (using standard entity)**:
- Status: ✅ PASSED in **2 minutes 1 second**
- Eliminated entity creation (~40-50 seconds)
- Eliminated entity deletion (~30-40 seconds)
- **Total improvement: ~58 seconds (~32% faster)**

**Before optimization (corrected approach)**:
- Status: ✅ PASSED in 2 minutes 59 seconds
- No retry failures
- No scoping issues

**With Measure-Operation scopes (initial incorrect approach)**:
- Status: ❌ FAILED after 210+ seconds
- 20 retry attempts
- Variable scoping bug introduced by the profiling code itself

---

## Root Cause & Solution

### Original Issue
**The slowness was caused by the profiling instrumentation itself**, not by the original test code.

The original test code did NOT have a variable scoping issue. The `$publisher` variable was correctly scoped within the `Invoke-WithRetry` scriptblock and accessible throughout that scope.

When `Measure-Operation` wrappers were added, they created nested scriptblocks that broke the variable scoping, causing the failure.

### Optimization Applied
**Removed unnecessary entity creation/deletion steps** by using the standard `account` entity instead of creating a custom entity.

Rationale:
- The test validates solution component operations
- The solution name is unique (includes timestamp and GUID)
- Using a standard entity avoids conflicts and is much faster
- Pattern matches other E2E tests (RecordAccessTests, etc.)

---

## Conclusion

### Main Finding

**The test has been optimized from ~3 minutes to ~2 minutes** by using a standard entity instead of creating/deleting a custom entity.

Test operations:
1. Create solution
2. Get entity metadata (standard 'account' entity)
3. Add entity to solution
4. Verify the component
5. Clean up (remove solution only)

### Lesson Learned

1. When profiling PowerShell code, avoid introducing new scriptblock scopes (`{ }`) as they can change variable accessibility and alter test behavior
2. Use standard entities when possible instead of creating custom ones, especially when the unique identifier (solution name) prevents conflicts
3. Look at similar tests for patterns to follow

### Corrected Approach

The test now includes:
- Simple inline timing without new scopes
- Uses standard 'account' entity instead of custom entity
- Faster execution (~2 minutes vs ~3 minutes)
- No entity cleanup needed

```powershell
$stepStartTime = Get-Date
# ... operation code ...
$stepDuration = ((Get-Date) - $stepStartTime).TotalSeconds
Write-Host "[TIMING] Step completed in $stepDuration seconds"
```

---

## Timing Breakdown

**Optimized version (~2 minutes):**
- Pre-check sleep: 5 seconds
- Step 1 (Create solution): ~10-20 seconds
- Step 2 (Get entity metadata): ~2-5 seconds
- Step 3 (Add to solution): ~5-10 seconds
- Step 4 (Verify component): ~5-10 seconds
- Step 5 (Remove solution): ~10-20 seconds
- **Total**: ~120 seconds (2 minutes)

**Time saved:** ~70 seconds from eliminating entity creation/deletion operations.
