# SetDataverseRecordCmdlet Refactoring - Detailed Action Plan

## Executive Summary

This document provides a step-by-step action plan for completing the refactoring of SetDataverseRecordCmdlet following the PR #137 pattern. The refactoring is ~18% complete with critical architectural insights documented.

**Current State:** 11 methods migrated (~240 lines, 16% of business logic)  
**Remaining Work:** ~1280 lines across ~16 methods in 4 phases  
**Estimated Time:** 20-27 hours

## Phase 0: Completed ✅

**What was done:**
- Moved 11 utility and completion handler methods to SetOperationContext
- Established hybrid delegation pattern
- All 188 tests passing

**Commits:**
- b1403a2: Moved NeedsRetrieval and GetExistingRecord
- f33151c: Moved 9 completion handlers
- 5198dbf: Updated REFACTORING_STATUS with phase analysis

---

## Phase 3: Batch Infrastructure Migration (NEXT)

**Priority:** CRITICAL - This phase must be completed before Phase 1 & 2  
**Estimated Time:** 4-6 hours  
**Complexity:** High - Affects 9 call sites, requires coordinated migration

### Why This Phase is Critical

The current architecture has TWO batch processing systems:
1. **SetBatchProcessor** (new, ready but unused) - Uses SetOperationContext
2. **BatchItem system** (old, currently in use) - Embedded in cmdlet

Operation methods (UpsertRecord, CreateNewRecord, etc.) call QueueBatchItem which uses the old system. They must be migrated to use SetBatchProcessor before they can be moved to the context.

### Current Architecture Problem

```
CreateNewRecord() in cmdlet
  ↓ builds CreateRequest
  ↓ calls
QueueBatchItem(new BatchItem(..., completion lambda, ...))
  ↓ adds to
_nextBatchItems (List<BatchItem> in cmdlet)
  ↓ processed by  
ProcessBatch() in cmdlet
```

### Target Architecture

```
CreateNewRecord() in context
  ↓ builds CreateRequest
  ↓ stores in
context.Requests
  ↓ queued via
_setBatchProcessor.QueueOperation(context)
  ↓ processed by
SetBatchProcessor.ExecuteBatch()
```

### Step-by-Step Migration Plan

#### Step 1: Understand Current BatchItem Usage (30 min)

**Analyze:**
- BatchItem class (lines 962-1010)
- QueueBatchItem method (lines 1030-1044)
- ProcessBatch method (lines 1048-1164)
- 9 call sites where new BatchItem() is created

**Key insight:** BatchItem holds:
- PSObject InputObject
- OrganizationRequest Request
- Action<OrganizationResponse> ResponseCompletion (lambda)
- Func<OrganizationServiceFault, bool> ResponseExceptionCompletion (lambda)

**Problem:** SetOperationContext doesn't currently have these lambda callbacks.

#### Step 2: Add Callback Support to SetOperationContext (1-2 hours)

**File:** `SetDataverseRecordCmdlet.cs` - SetOperationContext class

**Add to SetOperationContext:**
```csharp
public Action<OrganizationResponse> ResponseCompletion { get; set; }
public Func<OrganizationServiceFault, bool> ResponseExceptionCompletion { get; set; }
```

**Update constructor to accept callbacks:**
```csharp
public SetOperationContext(
    // ... existing parameters ...
    Action<OrganizationResponse> responseCompletion = null,
    Func<OrganizationServiceFault, bool> responseExceptionCompletion = null)
{
    // ... existing code ...
    ResponseCompletion = responseCompletion;
    ResponseExceptionCompletion = responseExceptionCompletion;
}
```

**Test:** Build and run tests to ensure no regressions.

#### Step 3: Update SetBatchProcessor.ExecuteBatch to Use Callbacks (1 hour)

**File:** `SetDataverseRecordCmdlet.cs` - SetBatchProcessor class

**Current ExecuteBatch** processes SetOperationContext but doesn't call callbacks.

**Update:** Lines 753-795 in ExecuteBatch to:
```csharp
foreach (var context in _nextBatchItems)
{
    if (itemResponse.Fault != null)
    {
        // Call context.ResponseExceptionCompletion if set
        bool handled = false;
        if (context.ResponseExceptionCompletion != null)
        {
            handled = context.ResponseExceptionCompletion(itemResponse.Fault);
        }
        
        if (!handled && context.RetriesRemaining > 0)
        {
            context.ScheduleRetry(e);
            _pendingRetries.Add(context);
        }
        else if (!handled)
        {
            context.ReportError(e);
        }
    }
    else
    {
        // Call context.ResponseCompletion if set
        if (context.ResponseCompletion != null)
        {
            context.ResponseCompletion(itemResponse.Response);
        }
        else
        {
            context.Complete(); // Default completion
        }
    }
}
```

**Test:** Build and run tests.

#### Step 4: Migrate One Operation Method as Proof of Concept (2-3 hours)

**Target:** CreateNewRecord method (simplest of the three)

**Create new method in SetOperationContext:**
```csharp
public void CreateNewRecord(EntityMetadata entityMetadata)
{
    if (NoCreate)
    {
        _writeVerbose($"Skipped creating new record {TableName}:{Id} - NoCreate enabled");
        return;
    }

    if (entityMetadata.IsIntersect.GetValueOrDefault())
    {
        // Handle M:M association creation
        // Move lines 1609-1667 here
    }
    else
    {
        // Handle regular entity creation
        Entity targetCreate = new Entity(Target.LogicalName) { Id = Target.Id };
        targetCreate.Attributes.AddRange(Target.Attributes.Where(a => 
            !DontUpdateDirectlyColumnNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)));

        string columnSummary = GetColumnSummary(targetCreate, EntityConverter);

        CreateRequest request = new CreateRequest() { Target = targetCreate };
        ApplyBypassBusinessLogicExecution(request);
        
        // Store request and callback
        Requests.Add(request);
        ResponseCompletion = (response) => {
            CreateCompletion(Target, targetCreate, columnSummary, (CreateResponse)response);
        };

        _writeVerbose($"Added created of new record {TableName}:{targetCreate.Id} to batch - columns:\n{columnSummary}");
    }
}
```

**Update cmdlet wrapper:**
```csharp
private void CreateNewRecord(PSObject inputObject, string tableName, Guid? callerId, EntityMetadata entityMetadata, Entity target)
{
    var context = new SetOperationContext(inputObject, tableName, callerId, this, ...);
    context.Target = target;
    context.EntityMetadata = entityMetadata;
    context.CreateNewRecord(entityMetadata);
    
    // If batching is enabled, queue the context
    if (_setBatchProcessor != null)
    {
        _setBatchProcessor.QueueOperation(context);
    }
    else
    {
        // Non-batch mode: execute immediately
        if (ShouldProcess($"Create new record {tableName}"))
        {
            try
            {
                if (callerId.HasValue)
                {
                    Connection.CallerId = callerId.Value;
                }
                var response = Connection.Execute(context.Requests[0]);
                if (context.ResponseCompletion != null)
                {
                    context.ResponseCompletion(response);
                }
                if (callerId.HasValue)
                {
                    Connection.CallerId = Guid.Empty;
                }
            }
            catch (Exception e)
            {
                if (callerId.HasValue)
                {
                    Connection.CallerId = Guid.Empty;
                }
                WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, inputObject));
            }
        }
    }
}
```

**Remove:** Old BatchItem call at line 1685 (now handled by SetBatchProcessor)

**Test:** Run full test suite, especially Set-DataverseRecord tests.

#### Step 5: Migrate UpdateExistingRecord (1-2 hours)

**Follow same pattern as Step 4** for UpdateExistingRecord method.

#### Step 6: Migrate UpsertRecord (1-2 hours)

**Follow same pattern as Step 4** for UpsertRecord method.

#### Step 7: Remove Old Batch Infrastructure (30 min)

**Once all 9 call sites are migrated:**
1. Remove BatchItem class (lines 962-1010)
2. Remove _nextBatchItems List<BatchItem> (line 1012)
3. Remove _nextBatchCallerId (line 1013)
4. Remove QueueBatchItem method (lines 1030-1044)
5. Remove ProcessBatch method (lines 1048-1164)

**Test:** Full test suite must pass.

#### Step 8: Clean Up Cmdlet State (30 min)

**Result:** Cmdlet should now use only _setBatchProcessor for batching.

---

## Phase 1: Retrieval Infrastructure Migration

**Priority:** After Phase 3 complete  
**Estimated Time:** 6-8 hours  
**Complexity:** High - Complex state management

### Prerequisites
- Phase 3 complete
- SetBatchProcessor fully operational
- All tests passing

### Step-by-Step Plan

#### Step 1: Move RecordProcessingItem Class (30 min)

**Current location:** Lines 1018-1026 in cmdlet

**Move to:** SetOperationContext (nested class or separate)

```csharp
internal class RecordProcessingItem
{
    public PSObject InputObject { get; set; }
    public Entity Target { get; set; }
    public EntityMetadata EntityMetadata { get; set; }
    public Entity ExistingRecord { get; set; }
    public string TableName { get; set; }
    public Guid? CallerId { get; set; }
}
```

#### Step 2: Create RetrievalBatchProcessor or Add to Context (2-3 hours)

**Option A:** Create separate RetrievalBatchProcessor class  
**Option B:** Add retrieval methods to SetOperationContext

**Recommendation:** Option B (add to context) for simplicity.

**Move these methods to SetOperationContext:**
- ProcessQueuedRecords (~130 lines)
- RetrieveRecordsBatchById (~70 lines)
- RetrieveRecordsBatchByMatchOn (~135 lines)
- RetrieveRecordsBatchIntersect (~75 lines)

**Add to context:**
```csharp
private List<RecordProcessingItem> _retrievalBatchQueue;

public void QueueForRetrieval(RecordProcessingItem item)
{
    _retrievalBatchQueue.Add(item);
}

public void ProcessQueuedRecords(Action<PSObject, string, Guid?, Entity, EntityMetadata, Entity> processRecordWithExistingRecord)
{
    // Move logic from cmdlet lines 1839-1967
}

private void RetrieveRecordsBatchById(List<RecordProcessingItem> records)
{
    // Move logic from cmdlet lines 1974-2015
}

// ... etc
```

**Update cmdlet to delegate:**
```csharp
// In ProcessSingleRecord:
if (context.NeedsRetrieval(entityMetadata, target))
{
    context.QueueForRetrieval(new RecordProcessingItem { ... });
    
    if (context.RetrievalQueueCount >= RetrievalBatchSize)
    {
        context.ProcessQueuedRecords(ProcessRecordWithExistingRecord);
    }
}
```

#### Step 3: Move Retry Management (2-3 hours)

**Move RetryRecord class and methods:**
- RetryRecord class (lines 949-958)
- _pendingRetries list
- ScheduleRecordRetry method
- RecordRetryDone method
- ProcessRetries method

**Move to:** SetBatchProcessor (retry is part of batch processing)

---

## Phase 2: Core Operation Methods Migration

**Priority:** After Phases 3 & 1 complete  
**Estimated Time:** 8-10 hours  
**Complexity:** Very High - Core business logic

### Prerequisites
- Phase 3 & 1 complete
- Batch and retrieval infrastructure migrated
- All tests passing

### Methods to Migrate

1. **ProcessSingleRecord** (~100 lines)
   - Entry point for record processing
   - Move to context as ProcessRecord()
   
2. **ProcessRecordWithExistingRecord** (~115 lines)
   - Routes to create/update/upsert
   - Move to context
   
3. **UpsertRecord** (~145 lines) - If not done in Phase 3
4. **CreateNewRecord** (~120 lines) - If not done in Phase 3
5. **UpdateExistingRecord** (~135 lines) - If not done in Phase 3

### General Pattern

For each method:
1. Move logic to SetOperationContext
2. Replace cmdlet property references with context properties
3. Replace cmdlet method calls with delegate calls
4. Create thin wrapper in cmdlet that creates context and delegates
5. Test after each method

---

## Phase 4: Final Cleanup

**Priority:** After all phases complete  
**Estimated Time:** 2-3 hours  
**Complexity:** Medium - Cleanup and validation

### Tasks

1. **Remove Delegation Wrappers** (1 hour)
   - Remove cmdlet wrapper methods
   - Thread context through call chain
   - Direct calls to context methods

2. **Simplify Cmdlet Lifecycle** (1 hour)
   - ProcessRecord becomes ~10 lines
   - BeginProcessing initializes processor only
   - EndProcessing calls processor.Flush() and ProcessRetries()

3. **Final Validation** (1 hour)
   - All 188 tests pass
   - No performance regression
   - Code review for cleanup opportunities

---

## Testing Strategy

### After Each Step
1. Build solution: `dotnet build -c Release`
2. Run full test suite: `Invoke-Pester -Path tests`
3. Verify 188/188 tests pass
4. Check test execution time (~101-104s baseline)

### Specific Test Focus
- **Set-DataverseRecord.Tests.ps1** - Main cmdlet tests (46KB)
- **Set-DataverseRecord-BatchedRetrieval.Tests.ps1** - Batching tests (9KB)
- **Examples.Tests.ps1** - Integration examples (51KB)

### Regression Indicators
- Any test failures
- Test execution time increase > 10%
- Build errors or warnings (beyond existing baseline)

---

## Success Criteria

### Per Phase
- [ ] All code migrated as planned
- [ ] All 188 tests passing
- [ ] Build successful with no new errors
- [ ] No performance regression

### Overall Project
- [ ] Cmdlet class reduced to ~400-500 lines
- [ ] ProcessRecord method ~10-15 lines
- [ ] All business logic in SetOperationContext and SetBatchProcessor
- [ ] Code maintainability significantly improved
- [ ] Pattern matches RemoveDataverseRecordCmdlet refactoring

---

## Risk Mitigation

### High-Risk Areas
1. **Callback Migration** (Phase 3) - Lambda closures may behave differently
2. **State Management** (Phase 1) - Queue timing and synchronization
3. **Batch Execution** (Phase 3) - Request ordering and responses

### Mitigation Strategies
1. **Incremental Commits** - Commit after each method migration
2. **Continuous Testing** - Run tests after every change
3. **Rollback Plan** - Keep git history clean for easy revert
4. **Code Review** - Review each phase before proceeding to next

---

## Estimated Timeline

### Full-Time Focused Work
- **Week 1:** Phase 3 (2 days) + Phase 1 (3 days)
- **Week 2:** Phase 2 (4 days) + Phase 4 (1 day)

### Part-Time Work (4 hours/day)
- **Weeks 1-2:** Phase 3 (3 days)
- **Weeks 2-3:** Phase 1 (4 days)
- **Weeks 3-5:** Phase 2 (5 days)
- **Week 5:** Phase 4 (1 day)

### Total: 3-5 weeks depending on availability

---

## Additional Notes

### Code Review Checkpoints
- After Phase 3: Batch infrastructure migration complete
- After Phase 1: Retrieval infrastructure migrated
- After Phase 2: All operation methods migrated
- After Phase 4: Final cleanup complete

### Documentation Updates
- Update REFACTORING_STATUS.md after each phase
- Update PR description with progress
- Document any architectural decisions or deviations from plan

### Questions to Resolve
1. Should RecordProcessingItem be in context or separate?
2. Should retrieval batching be in separate processor or context?
3. How to handle non-batch mode execution after migration?
4. Should we maintain hybrid delegation or go all-in?

---

## Conclusion

This refactoring is substantial but well-structured. The critical insight is that Phase 3 must be completed first to unblock the other phases. With careful, incremental execution following this plan, the refactoring can be completed successfully while maintaining test coverage and code quality throughout.
