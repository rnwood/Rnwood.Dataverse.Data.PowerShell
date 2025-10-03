# Complex Type Conversion Implementation Status

## Overview

This document tracks the implementation status of PowerShell-friendly parameter sets for all cmdlets that use complex SDK types.

## Summary Statistics

- **Total Cmdlets with Complex Types:** 43
- **Completed:** 2 (5%)
- **In Progress:** 41 (95%)
- **Target:** 100% completion

## Implementation Pattern

### For QueryBase/QueryExpression Parameters

```csharp
// Add three parameter sets
[Parameter(ParameterSetName = "QueryObject", Mandatory = false)]
public QueryBase Query { get; set; }

[Parameter(ParameterSetName = "FetchXml", Mandatory = true)]
public string FetchXml { get; set; }

[Parameter(ParameterSetName = "Filter", Mandatory = true)]
public Hashtable Filter { get; set; }

[Parameter(ParameterSetName = "Filter", Mandatory = true)]
[Parameter(ParameterSetName = "FetchXml", Mandatory = false)]
public string TableName { get; set; }

// In ProcessRecord()
if (ParameterSetName == "FetchXml" || ParameterSetName == "Filter")
{
    request.Query = DataverseComplexTypeConverter.ToQueryBase(FetchXml, Filter, TableName);
}
else
{
    request.Query = Query;
}
```

### For ColumnSet Parameters

```csharp
// Add three parameter sets
[Parameter(ParameterSetName = "ColumnSetObject", Mandatory = false)]
public ColumnSet ColumnSet { get; set; }

[Parameter(ParameterSetName = "Columns", Mandatory = true)]
public string[] Columns { get; set; }

[Parameter(ParameterSetName = "AllColumns", Mandatory = true)]
public SwitchParameter AllColumns { get; set; }

// In ProcessRecord()
ColumnSet columnSet;
if (ParameterSetName == "Columns")
{
    columnSet = DataverseComplexTypeConverter.ToColumnSet(Columns, false);
}
else if (ParameterSetName == "AllColumns")
{
    columnSet = DataverseComplexTypeConverter.ToColumnSet(null, true);
}
else
{
    columnSet = ColumnSet;
}
```

### For PagingInfo Parameters

```csharp
// Add two parameter sets
[Parameter(ParameterSetName = "PagingObject", Mandatory = false)]
public PagingInfo PagingInfo { get; set; }

[Parameter(ParameterSetName = "SimplePaging", Mandatory = false)]
public int PageNumber { get; set; } = 1;

[Parameter(ParameterSetName = "SimplePaging", Mandatory = false)]
public int PageSize { get; set; } = 5000;

// In ProcessRecord()
PagingInfo pagingInfo;
if (ParameterSetName == "SimplePaging")
{
    pagingInfo = DataverseComplexTypeConverter.ToPagingInfo(PageNumber, PageSize);
}
else
{
    pagingInfo = PagingInfo;
}
```

## Priority 1: QueryBase/QueryExpression Parameters (17 cmdlets)

### Completed (2/17)
- ✅ **BulkDataverseDetectDuplicatesCmdlet** - Group-DataverseDetectDuplicates
- ✅ **CopyDataverseByExpressionCmdlet** - Copy-DataverseByExpression

### Remaining (15/17)
- ⏳ **FindDataverseByBodyKbArticleCmdlet** - Find-DataverseByBodyKbArticle
- ⏳ **FindDataverseByKeywordsKbArticleCmdlet** - Find-DataverseByKeywordsKbArticle
- ⏳ **FindDataverseByTitleKbArticleCmdlet** - Find-DataverseByTitleKbArticle
- ⏳ **FindDataverseExpressionToFetchXmlCmdlet** - Find-DataverseExpressionToFetchXml
- ⏳ **GetDataverseByGroupResourceCmdlet** - Get-DataverseByGroupResource
- ⏳ **GetDataverseByResourceResourceGroupCmdlet** - Get-DataverseByResourceResourceGroup
- ⏳ **GetDataverseByResourcesServiceCmdlet** - Get-DataverseByResourcesService
- ⏳ **GetDataverseMembersBulkOperationCmdlet** - Get-DataverseMembersBulkOperation
- ⏳ **GetDataverseParentGroupsResourceGroupCmdlet** - Get-DataverseParentGroupsResourceGroup
- ⏳ **GetDataverseSubGroupsResourceGroupCmdlet** - Get-DataverseSubGroupsResourceGroup
- ⏳ **InvokeDataverseBackgroundSendEmailCmdlet** - Invoke-DataverseBackgroundSendEmail
- ⏳ **InvokeDataverseFetchXmlToQueryExpressionCmdlet** - Invoke-DataverseFetchXmlToQueryExpression
- ⏳ **MeasureDataverseRollupCmdlet** - Measure-DataverseRollup
- ⏳ **SendDataverseBulkMailCmdlet** - Send-DataverseBulkMail
- ⏳ **SearchDataverseTextSearchKnowledgeArticleCmdlet** - Search-DataverseTextSearchKnowledgeArticle

## Priority 2: ColumnSet Parameters (16 cmdlets)

### All Remaining (16/16)
- ⏳ **ConvertDataverseQuoteToSalesOrderCmdlet** - Convert-DataverseQuoteToSalesOrder
- ⏳ **ConvertDataverseSalesOrderToInvoiceCmdlet** - Convert-DataverseSalesOrderToInvoice
- ⏳ **GetDataverseAadUserRolesCmdlet** - Get-DataverseAadUserRoles
- ⏳ **GetDataverseAllChildUsersSystemUserCmdlet** - Get-DataverseAllChildUsersSystemUser
- ⏳ **GetDataverseBusinessHierarchyBusinessUnitCmdlet** - Get-DataverseBusinessHierarchyBusinessUnit
- ⏳ **GetDataverseDefaultPriceLevelCmdlet** - Get-DataverseDefaultPriceLevel
- ⏳ **GetDataverseMembersTeamCmdlet** - Get-DataverseMembersTeam
- ⏳ **GetDataverseSubsidiaryTeamsBusinessUnitCmdlet** - Get-DataverseSubsidiaryTeamsBusinessUnit
- ⏳ **GetDataverseSubsidiaryUsersBusinessUnitCmdlet** - Get-DataverseSubsidiaryUsersBusinessUnit
- ⏳ **GetDataverseTeamsSystemUserCmdlet** - Get-DataverseTeamsSystemUser
- ⏳ **GetDataverseUnpublishedCmdlet** - Get-DataverseUnpublished
- ⏳ **GetDataverseUnpublishedMultipleCmdlet** - Get-DataverseUnpublishedMultiple
- ⏳ **InvokeDataverseBackgroundSendEmailCmdlet** - Invoke-DataverseBackgroundSendEmail (also has QueryBase)
- ⏳ **InvokeDataverseCalculatePriceCmdlet** - Invoke-DataverseCalculatePrice
- ⏳ **MeasureDataverseRollupCmdlet** - Measure-DataverseRollup (also has QueryBase)
- ⏳ **SendDataverseBulkMailCmdlet** - Send-DataverseBulkMail (also has QueryBase)

## Priority 3: PagingInfo Parameters (4 cmdlets)

### All Remaining (4/4)
- ⏳ **GetDataverseAttributeChangeHistoryCmdlet** - Get-DataverseAttributeChangeHistory
- ⏳ **GetDataverseDuplicatesCmdlet** - Get-DataverseDuplicates
- ⏳ **GetDataverseParsedDataImportFileCmdlet** - Get-DataverseParsedDataImportFile
- ⏳ **GetDataverseRecordChangeHistoryCmdlet** - Get-DataverseRecordChangeHistory

## Priority 4: RolePrivilege[] Parameters (2 cmdlets)

### All Remaining (2/2)
- ⏳ **AddDataversePrivilegesRoleCmdlet** - Add-DataversePrivilegesRole
- ⏳ **UpdateDataversePrivilegesRoleCmdlet** - Update-DataversePrivilegesRole

## Priority 5: TimeCode[] Parameters (2 cmdlets)

### All Remaining (2/2)
- ⏳ **FindDataverseMultipleSchedulesCmdlet** - Find-DataverseMultipleSchedules
- ⏳ **FindDataverseScheduleCmdlet** - Find-DataverseSchedule

## Priority 6: LocalizedLabel[] Parameters (1 cmdlet)

### Remaining (1/1)
- ⏳ **SetDataverseLocLabelsCmdlet** - Set-DataverseLocLabels

## Priority 7: SolutionParameters (4 cmdlets)

These cmdlets need individual boolean parameters instead of complex object.

### All Remaining (4/4)
- ⏳ **ExportDataverseSolutionCmdlet** - Export-DataverseSolution
- ⏳ **ExportDataverseSolutionAsyncCmdlet** - Export-DataverseSolutionAsync
- ⏳ **ImportDataverseSolutionCmdlet** - Import-DataverseSolution
- ⏳ **ImportDataverseSolutionAsyncCmdlet** - Import-DataverseSolutionAsync

## Test Updates Required

### Tests to Unskip (with new metadata available)
- Set-DataverseRecordState (contact metadata available)
- Grant-DataverseAccess (may work with contact records)
- Revoke-DataverseAccess (may work with contact records)
- Publish-DataverseCustomization (may work with minimal metadata)

### New Tests to Add
For each updated cmdlet, add tests for:
- FetchXml parameter set
- Filter parameter set
- Columns parameter set
- AllColumns parameter set
- Simple paging parameters

## Documentation Updates Required

For each updated cmdlet:
1. Add "PARAMETER SETS" section in docs explaining the three approaches
2. Add examples for FetchXml usage
3. Add examples for Filter/Hashtable usage
4. Add examples for simplified parameters (Columns, PageNumber, etc.)
5. Update SYNOPSIS and DESCRIPTION to mention PowerShell-friendly parameters

## Timeline Estimate

- **Priority 1 (QueryBase):** 15 cmdlets × 10 min = 2.5 hours
- **Priority 2 (ColumnSet):** 16 cmdlets × 8 min = 2.1 hours
- **Priority 3 (PagingInfo):** 4 cmdlets × 8 min = 0.5 hours
- **Priority 4-6 (Other):** 5 cmdlets × 10 min = 0.8 hours
- **Priority 7 (SolutionParams):** 4 cmdlets × 15 min = 1.0 hour
- **Testing:** 43 tests × 3 min = 2.1 hours
- **Documentation:** 43 docs × 5 min = 3.6 hours

**Total Estimated Time:** 12.6 hours for 100% completion

## Next Actions

1. Complete Priority 1 (QueryBase parameters) - 15 cmdlets
2. Complete Priority 2 (ColumnSet parameters) - 16 cmdlets
3. Complete Priorities 3-7 - 10 cmdlets
4. Update all tests
5. Update all documentation
6. Final validation and testing

## Notes

- Pattern is established and validated
- DataverseComplexTypeConverter is complete and tested
- Build succeeds with current implementation
- No breaking changes - all original parameter sets still work
- 100% backward compatibility maintained
