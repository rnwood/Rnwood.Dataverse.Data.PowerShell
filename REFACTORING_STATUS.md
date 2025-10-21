# SetDataverseRecordCmdlet Refactoring Status

## Objective
Apply PR #137 refactoring pattern to SetDataverseRecordCmdlet

## What PR #137 Did
- Reduced RemoveDataverseRecordCmdlet from ~470 to ~145 lines
- Moved logic into DeleteOperationContext
- Made cmdlet a thin orchestration layer
- Pattern: Context owns request creation and execution

## Progress Made Through Current PR
### Infrastructure (Previous Commits)
✅ Created ISetOperationParameters interface
✅ Created SetOperationContext class with delegates
✅ Created SetBatchProcessor class
✅ Wired up lifecycle (BeginProcessing, EndProcessing, StopProcessing)
✅ Added CancellationTokenSource support

### Initial Code Extraction (Previous Commits)
✅ Extracted 7 utility methods to context:
  - GetKeySummary, GetColumnSummary, Ellipsis, GetValueSummary
  - RemoveUnchangedColumns
  - ApplyBypassBusinessLogicExecution
  - SetIdProperty
✅ Extracted 1 constant: DontUpdateDirectlyColumnNames
✅ Replaced 5 duplicate code blocks

### Current PR Phase 1 Progress (Commits 1-3)
✅ Moved NeedsRetrieval to SetOperationContext (~18 lines)
✅ Moved GetExistingRecord to SetOperationContext (~102 lines)  
✅ Moved 9 completion/error handlers to SetOperationContext (~120 lines)
  - CreateCompletion, UpdateCompletion, UpsertCompletion
  - AssociateCompletion, AssociateUpsertCompletion, AssociateUpsertError
  - AssociateUpsertGetIdCompletion
  - SetStateCompletion, AssignRecordCompletion
✅ Established hybrid delegation pattern for incremental migration
✅ All 188 tests passing throughout

**Total migrated this PR: 11 methods, ~240 lines (16% of business logic)**

## Current State (After PR Commits)
**File Structure:**
- Lines 1-88: ISetOperationParameters interface
- Lines 90-809: SetOperationContext class (was 323, now 809 lines)
- Lines 811-1057: SetBatchProcessor class
- Lines 1059-2306: SetDataverseRecordCmdlet class (with delegation wrappers)

**Cmdlet Status:**
- Infrastructure: ✅ Complete (matches PR #137)
- Business Logic: 🟡 16% migrated, 84% remaining in cmdlet

## Remaining Work: Complete the Refactoring
To match PR #137 pattern, need to move **~1280 lines** more from cmdlet to context:

### Phase 1 REMAINING: Retrieval Infrastructure (~460 lines, 6 methods)
**Status:** 🔴 Blocked by architectural dependencies
- ProcessQueuedRecords (~130 lines) - depends on _retrievalBatchQueue state
- RetrieveRecordsBatchById (~70 lines) - depends on cmdlet Id property
- RetrieveRecordsBatchByMatchOn (~135 lines) - depends on cmdlet MatchOn property  
- RetrieveRecordsBatchIntersect (~75 lines)
- ScheduleRecordRetry (~30 lines) - depends on _pendingRetries state
- RecordRetryDone (~3 lines) - depends on _pendingRetries state
- ProcessRetries (~45 lines) - complex retry orchestration

**Blocker:** These methods require moving cmdlet state (_retrievalBatchQueue, _pendingRetries) and the RecordProcessingItem class. This is a coordinated migration requiring architectural changes.

### Phase 2: Core Operation Methods (~600 lines, 5 methods) 
**Status:** 🔴 Blocked by batch infrastructure dependencies
- ProcessSingleRecord (~100 lines) - orchestrates record processing flow
- UpsertRecord (~145 lines) - depends on _nextBatchItems, QueueBatchItem
- CreateNewRecord (~120 lines) - depends on _nextBatchItems, QueueBatchItem
- UpdateExistingRecord (~135 lines) - depends on _nextBatchItems, QueueBatchItem
- ProcessRecordWithExistingRecord (~115 lines) - calls above methods

**Blocker:** These methods depend heavily on cmdlet batch state (_nextBatchItems, _nextBatchCallerId) and QueueBatchItem/ProcessBatch infrastructure.

### Phase 3: Batch Infrastructure Migration (~150 lines, 4 methods)
**Status:** 🟡 Key architectural blocker for Phase 1 & 2
- Move _nextBatchItems state to SetBatchProcessor
- Move _retrievalBatchQueue to context or separate processor
- Move _pendingRetries to SetBatchProcessor  
- QueueBatchItem (~15 lines) → processor method
- ProcessBatch (~80 lines) → processor method
- AppendFaultDetails (~10 lines) → utility

**This phase must be completed BEFORE Phase 1 & 2 can proceed.**

### Phase 4: Cmdlet Simplification (~70 lines cleanup)
**Status:** ⏳ Final step after Phase 1-3 complete
- Remove delegation wrappers (replace with direct context calls)
- Simplify ProcessRecord to ~10 lines
- Simplify BeginProcessing/EndProcessing
- Remove duplicate business logic from cmdlet
- Target: Cmdlet reduced to ~400-500 lines

## Complexity Factors
1. **Scale:** 3.5x larger than RemoveDataverseRecord (2306 lines vs 470 lines pre-refactoring)
2. **Operations:** 8 operation types vs 1 (Create/Update/Upsert/Associate/Disassociate/Assign/SetState/Retrieve vs Delete)
3. **State:** Complex state management across 3 different queues:
   - _nextBatchItems: Pending operations for batch execution
   - _retrievalBatchQueue: Records needing retrieval before processing
   - _pendingRetries: Failed operations awaiting retry
4. **Callbacks:** 8 callback invocation points to replace in lambdas
5. **Testing:** Must validate all 8 operation paths (188 tests must pass)

## Critical Architectural Dependency
**THE BLOCKER:** Phase 1 and Phase 2 cannot proceed until Phase 3 (Batch Infrastructure Migration) is completed first.

The current architecture has:
- Operation methods (UpsertRecord, CreateNewRecord, etc.) that call QueueBatchItem
- QueueBatchItem accesses _nextBatchItems state in cmdlet
- ProcessQueuedRecords accesses _retrievalBatchQueue state in cmdlet
- Both need these states moved to processors FIRST

**Correct Order:**
1. ✅ **Phase 0** (Current): Move utility methods and establish pattern (~240 lines done)
2. 🔴 **Phase 3** (NEXT): Migrate batch infrastructure (~150 lines) - UNBLOCKS Phase 1 & 2
3. 🟡 **Phase 1**: Migrate retrieval infrastructure (~460 lines)
4. 🟡 **Phase 2**: Migrate core operations (~600 lines)
5. ⏳ **Phase 4**: Final cleanup (~70 lines)

## Updated Recommendations

### Immediate Next Steps (Phase 3 - Batch Infrastructure)
**Estimated: 4-6 hours**

1. Move BatchItem class to SetOperationContext or SetBatchProcessor
2. Move _nextBatchItems and _nextBatchCallerId to SetBatchProcessor
3. Move QueueBatchItem method to processor
4. Move ProcessBatch method to processor
5. Update all callers to use processor methods
6. Validate all 188 tests pass

### After Phase 3 Complete (Phase 1 - Retrieval)
**Estimated: 6-8 hours**

1. Move RecordProcessingItem class to SetOperationContext
2. Move _retrievalBatchQueue to context or separate retrieval processor
3. Move ProcessQueuedRecords and retrieval batch methods to context
4. Move _pendingRetries and retry methods to processor
5. Validate all 188 tests pass

### After Phase 1 Complete (Phase 2 - Operations)
**Estimated: 8-10 hours**

1. Move UpsertRecord, CreateNewRecord, UpdateExistingRecord to context
2. Move ProcessSingleRecord to context
3. Move ProcessRecordWithExistingRecord to context
4. Update cmdlet ProcessRecord to create context and delegate
5. Validate all 188 tests pass

### Final (Phase 4 - Cleanup)
**Estimated: 2-3 hours**

1. Remove all delegation wrappers
2. Thread context through entire call chain
3. Simplify cmdlet lifecycle methods
4. Final validation

**Total Remaining: 20-27 hours across 4 focused sessions**
