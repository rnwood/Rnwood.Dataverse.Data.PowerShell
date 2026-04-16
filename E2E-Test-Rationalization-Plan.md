# E2E Test Rationalization Plan

## Scope

Goal: reduce the E2E suite so it covers only behavior that meaningfully requires a real Dataverse environment, while moving pure cmdlet logic, module smoke checks, and duplicate coverage into the standard test project or deleting it when equivalent standard coverage already exists.

No code changes have been made yet. This file is the exploration and execution plan requested before proceeding.

## How I categorized tests

I used four action categories:

- `KEEP_E2E`: keep in E2E because the value is real Dataverse behavior, real module loading/runtime behavior, or a live customization/publish/plugin execution workflow.
- `MOVE_TO_STANDARD`: move the whole test file into `Rnwood.Dataverse.Data.PowerShell.Tests` because the behavior is mockable and does not need live Dataverse.
- `SPLIT`: keep the genuinely live parts in E2E and move the rest into standard tests.
- `DELETE_DUPLICATE`: remove the E2E file because equivalent standard coverage already exists or the remaining assertions are low-value smoke checks.

## Inventory And Recommended Action

| E2E file | Area | Action | Why |
| --- | --- | --- | --- |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Module/ModuleTfmRoutingTests.cs` | Runtime-specific assembly routing | `KEEP_E2E` | Validates actual PowerShell process + runtime/TFM routing behavior that mocks will not prove. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Module/AzureFunctionsCompatibilityTests.cs` | Azure Functions compatibility | `KEEP_E2E` | Real assembly preload/version-conflict behavior is the point of the test. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Module/ModuleBasicTests.cs` | Module help/module packaging smoke | `SPLIT` | Keep live module import/help availability checks; move help-structure assertions that only inspect generated help artifacts. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Module/ConnectionTests.cs` | Connection creation and affinity-cookie warnings | `DELETE_DUPLICATE` | Current assertions are mostly property/warning checks already suited to standard tests and do not exercise complex Dataverse behavior. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/RecordOperations/SetDataverseRecordBasicTests.cs` | Basic `Set-DataverseRecord` `-Values` usage | `DELETE_DUPLICATE` | Standard tests already cover create/update/pass-through and input conversion patterns. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Sql/InvokeDataverseSqlTests.cs` | SQL cmdlet basics and additional connections | `DELETE_DUPLICATE` | Standard tests now cover basic SQL, named primary datasource, named additional datasource, cross-datasource queries, and invalid additional-connection values using the fixed mock harness. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Request/InvokeDataverseRequestTests.cs` | Generic request invocation | `DELETE_DUPLICATE` | Request construction/dispatch is already covered in standard tests and does not depend on live Dataverse semantics here. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Parallel/InvokeDataverseParallelTests.cs` | Parallel chunking and runspace behavior | `DELETE_DUPLICATE` | Standard tests already cover chunking and runspace connection mechanics. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Plugin/PluginManagementTests.cs` | Plugin assembly/type/step queries and enum smoke | `MOVE_TO_STANDARD` | These are metadata/query and enum assertions, not live plugin execution. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/AppModule/AppModuleTests.cs` | App module CRUD/component association | `DELETE_DUPLICATE` | Standard tests already cover app module cmdlet behavior broadly. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/RecordAccess/RecordAccessTests.cs` | Record access queries/sharing semantics | `DELETE_DUPLICATE` | Existing standard tests already cover record access cmdlet behavior; these E2E tests are not focused on complex security edge cases. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Views/ViewManipulationTests.cs` | View CRUD and publish behavior | `SPLIT` | Move basic CRUD/query-shape assertions; keep publish/customization lifecycle checks in E2E. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Forms/FormManipulationTests.cs` | Form CRUD and publish behavior | `SPLIT` | Move pure XML/manipulation assertions; keep publish/unpublished/live-environment behavior in E2E. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/WebResource/WebResourceTests.cs` | Web resource CRUD/content flow | `SPLIT` | Keep any live upload/download/content encoding verification; move command-level CRUD logic. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Solution/SolutionComponentTests.cs` | Solution component operations | `SPLIT` | Keep real solution/publish persistence checks; move purely command-shape logic. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/ErrorHandling/ErrorMessageTests.cs` | Error messaging for unreadable columns | `SPLIT` | Keep only the part that truly depends on real Dataverse error text/wrapping; move metadata-driven validation logic. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Metadata/EntityMetadataTests.cs` | Entity schema CRUD | `KEEP_E2E` | Real schema persistence and publish timing are the core behavior. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Metadata/AttributeMetadataTests.cs` | Attribute schema CRUD | `KEEP_E2E` | Real metadata persistence and type-specific server behavior are required. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Metadata/OptionSetMetadataTests.cs` | Option set metadata behavior | `KEEP_E2E` | Real metadata persistence and label/value behavior justify E2E coverage. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Metadata/RelationshipMetadataTests.cs` | Relationship schema behavior | `KEEP_E2E` | Real relationship creation/publish behavior is the value. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Metadata/EntityKeyMetadataTests.cs` | Alternate key metadata | `KEEP_E2E` | Server-side key persistence is the real-world behavior being tested. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Forms/FormLibraryAndEventHandlerTests.cs` | Form libraries/event handlers | `KEEP_E2E` | Touches shared form metadata and publish/customization behavior. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Plugin/DynamicPluginAssemblyTests.cs` | Dynamic plugin registration/execution | `KEEP_E2E` | Requires real plugin registration and execution lifecycle. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/Sitemap/SitemapTests.cs` | Sitemap customization/publish | `KEEP_E2E` | Real customization/publish workflow is the core behavior. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/EnvironmentVariable/EnvironmentVariableTests.cs` | Environment variable definition/value lifecycle | `KEEP_E2E` | Real Dataverse metadata/value persistence matters here. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/OrganizationSettings/OrganizationSettingsTests.cs` | Organization settings and OrgDbOrgSettings | `KEEP_E2E` | Shared singleton behavior and live persistence justify E2E coverage. |
| `Rnwood.Dataverse.Data.PowerShell.E2ETests/FileData/FileDataTests.cs` | File-data scenario placeholder | `KEEP_E2E` | Keep for now unless the file is clearly dead; it represents genuinely live file-storage behavior when fleshed out. |

## Existing Standard-Test Coverage Already Present

These existing files are the main reason the duplicate E2E candidates are good first moves:

- `Rnwood.Dataverse.Data.PowerShell.Tests/Cmdlets/GetDataverseConnectionTests.cs`
- `Rnwood.Dataverse.Data.PowerShell.Tests/Cmdlets/SetDataverseRecord_BasicTests.cs`
- `Rnwood.Dataverse.Data.PowerShell.Tests/Cmdlets/InvokeDataverseRequestTests.cs`
- `Rnwood.Dataverse.Data.PowerShell.Tests/Cmdlets/InvokeDataverseParallelTests.cs`
- `Rnwood.Dataverse.Data.PowerShell.Tests/Cmdlets/InvokeDataverseSqlTests.cs`
- `Rnwood.Dataverse.Data.PowerShell.Tests/Cmdlets/AppModulesTests.cs`
- `Rnwood.Dataverse.Data.PowerShell.Tests/Cmdlets/TestDataverseRecordAccessTests.cs`
- `Rnwood.Dataverse.Data.PowerShell.Tests/Cmdlets/ViewsTests.cs`
- `Rnwood.Dataverse.Data.PowerShell.Tests/Cmdlets/FormsTests.cs`
- `Rnwood.Dataverse.Data.PowerShell.Tests/Cmdlets/SolutionsTests.cs`
- `Rnwood.Dataverse.Data.PowerShell.Tests/Cmdlets/ModuleLoadingTests.cs`

## Recommended Execution Order

The safest sequence is to remove obvious duplicates first, then handle `MOVE_TO_STANDARD`, then tackle `SPLIT` files one by one.

### Phase 1: obvious duplicate removals

1. `Module/ConnectionTests.cs`
2. `RecordOperations/SetDataverseRecordBasicTests.cs`
3. `Request/InvokeDataverseRequestTests.cs`
4. `Parallel/InvokeDataverseParallelTests.cs`
5. `Sql/InvokeDataverseSqlTests.cs`
6. `AppModule/AppModuleTests.cs`
7. `RecordAccess/RecordAccessTests.cs`

Reason: these are the lowest-risk cuts because the standard project already has mature coverage for the same command paths.

### Phase 2: move fully mockable E2E files into standard tests

8. `Plugin/PluginManagementTests.cs`

Reason: this one should become ordinary standard coverage rather than remain in E2E.

### Phase 3: split mixed-value E2E files

9. `Module/ModuleBasicTests.cs`
10. `Views/ViewManipulationTests.cs`
11. `Forms/FormManipulationTests.cs`
12. `WebResource/WebResourceTests.cs`
13. `Solution/SolutionComponentTests.cs`
14. `ErrorHandling/ErrorMessageTests.cs`

Reason: these need method-by-method judgement rather than file-level deletion.

### Phase 4: leave real Dataverse behavior in E2E

Keep the metadata/customization/plugin-execution/runtime-assembly files as E2E unless a specific test method is later shown to be only command plumbing.

## Validation Strategy For Each Move

Each migration step should be done in isolation and verified before the next one.

### Preconditions

Before any step:

1. Build the solution.
2. Set `TESTMODULEPATH` to `Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0`.
3. If the step still involves E2E verification, set `E2ETESTS_URL`, `E2ETESTS_CLIENTID`, and `E2ETESTS_CLIENTSECRET` from the Dataverse dev environment.

### Per-step checks

For every file moved or deleted:

1. Run the targeted standard test file that now owns the coverage.
2. Run the targeted E2E class that was edited or the neighboring E2E class if the file was split.
3. Run a quick solution build.
4. Only then move to the next file.

### Standard validation command pattern

```powershell
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0")

dotnet test ./Rnwood.Dataverse.Data.PowerShell.Tests/Rnwood.Dataverse.Data.PowerShell.Tests.csproj `
  -f net8.0 `
  --filter "FullyQualifiedName~NameOfTargetStandardTestClass" `
  --logger "console;verbosity=normal"
```

### E2E validation command pattern

```powershell
$env:E2ETESTS_URL = $env:DATAVERSE_DEV_URL
$env:E2ETESTS_CLIENTID = $env:DATAVERSE_DEV_CLIENTID
$env:E2ETESTS_CLIENTSECRET = $env:DATAVERSE_DEV_CLIENTSECRET
$env:TESTMODULEPATH = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0")

dotnet test ./Rnwood.Dataverse.Data.PowerShell.E2ETests/Rnwood.Dataverse.Data.PowerShell.E2ETests.csproj `
  -f net8.0 `
  --filter "FullyQualifiedName~NameOfEditedOrRemainingE2EClass" `
  --logger "console;verbosity=normal"
```

## Step-by-step mapping for the first pass

| Step | E2E file | Standard destination or owning file | Planned result |
| --- | --- | --- | --- |
| 1 | `Module/ConnectionTests.cs` | `Tests/Cmdlets/GetDataverseConnectionTests.cs` | Delete E2E file after confirming warning/property coverage exists or is added in standard tests. |
| 2 | `RecordOperations/SetDataverseRecordBasicTests.cs` | `Tests/Cmdlets/SetDataverseRecord_BasicTests.cs` | Delete E2E file after confirming `-Values` alias/loop scenario is covered in standard tests. |
| 3 | `Request/InvokeDataverseRequestTests.cs` | `Tests/Cmdlets/InvokeDataverseRequestTests.cs` | Delete E2E file after confirming request parameter-set coverage in standard tests. |
| 4 | `Parallel/InvokeDataverseParallelTests.cs` | `Tests/Cmdlets/InvokeDataverseParallelTests.cs` | Delete E2E file after confirming chunking/runspace behavior remains covered. |
| 5 | `Sql/InvokeDataverseSqlTests.cs` | `Tests/Cmdlets/InvokeDataverseSqlTests.cs` | Delete E2E file after confirming SQL scenarios are covered in standard tests. |
| 6 | `AppModule/AppModuleTests.cs` | `Tests/Cmdlets/AppModulesTests.cs` | Delete E2E file if no uniquely live assertions remain. |
| 7 | `RecordAccess/RecordAccessTests.cs` | `Tests/Cmdlets/TestDataverseRecordAccessTests.cs` | Delete E2E file if no genuinely complex security behavior is present. |
| 8 | `Plugin/PluginManagementTests.cs` | New or existing plugin-focused file in `Tests/Cmdlets` | Move all tests to standard tests and remove the E2E file. |
| 9 | `Module/ModuleBasicTests.cs` | `Tests/Cmdlets/ModuleLoadingTests.cs` or new help-focused standard file | Keep only the live module/help packaging assertions in E2E. |
| 10 | `Views/ViewManipulationTests.cs` | `Tests/Cmdlets/ViewsTests.cs` | Keep only publish/persistence assertions in E2E. |
| 11 | `Forms/FormManipulationTests.cs` | `Tests/Cmdlets/FormsTests.cs` | Keep only publish/live-form behavior in E2E. |
| 12 | `WebResource/WebResourceTests.cs` | New or existing web-resource standard test file | Keep only live content/encoding behavior in E2E. |
| 13 | `Solution/SolutionComponentTests.cs` | `Tests/Cmdlets/SolutionsTests.cs` | Keep only live component persistence/publish behavior in E2E. |
| 14 | `ErrorHandling/ErrorMessageTests.cs` | Existing or new standard error-handling file | Keep only the real Dataverse exception-formatting assertion in E2E. |

## Expected Outcome

If the plan holds, the E2E suite should become smaller and more defensible:

- E2E focuses on schema changes, customization publish cycles, live plugin behavior, runtime loading issues, and true server-side behavior.
- Standard tests absorb mockable cmdlet logic and command-shape assertions.
- The migration can be executed safely one file at a time with targeted tests after each change.

## Risks To Watch While Executing

- Some files marked `DELETE_DUPLICATE` may still contain one or two assertions worth reintroducing in standard tests before deleting the E2E file.
- `SPLIT` files can easily drift into broad refactors; method-by-method extraction is safer than rewriting whole files.
- Some live tests rely on shared environment state or customization locks; these should remain isolated and should not be mixed into the standard suite.
- If a supposedly duplicate E2E test exposes behavior FakeXrmEasy cannot represent, stop and reclassify it instead of forcing the move.
- Investigation update: the standard SQL harness required seeding `ServiceClient` internal `ConnectionService` state so `ConnectedOrgUriActual` and org detail getters worked. With that mock fix in place, named/additional datasource SQL scenarios now pass in standard tests.