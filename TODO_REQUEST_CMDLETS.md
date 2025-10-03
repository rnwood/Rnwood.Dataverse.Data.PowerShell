# Dataverse SDK Request Cmdlets - Complete Implementation Plan

## Summary
Total Request Types: 337
Currently Implemented: 17
Remaining: 320

## Current Implementation Status

### ✅ Completed (17 cmdlets)
1. AssignRequest → `Set-DataverseRecordOwner`
2. SetStateRequest → `Set-DataverseRecordState`  
3. ExecuteWorkflowRequest → `Invoke-DataverseWorkflow`
4. AssociateRequest → `Add-DataverseAssociation` (M:M from Xrm.Sdk.Messages)
5. DisassociateRequest → `Remove-DataverseAssociation` (M:M from Xrm.Sdk.Messages)
6. GrantAccessRequest → `Grant-DataverseAccess`
7. RevokeAccessRequest → `Revoke-DataverseAccess`
8. AddMembersTeamRequest → `Add-DataverseTeamMembers`
9. RemoveMembersTeamRequest → `Remove-DataverseTeamMembers`
10. PublishXmlRequest → `Publish-DataverseCustomization`
11. SendEmailRequest → `Send-DataverseEmail`
12. LockSalesOrderPricingRequest → `Lock-DataverseSalesOrderPricing`
13. UnlockSalesOrderPricingRequest → `Unlock-DataverseSalesOrderPricing`
14. LockInvoicePricingRequest → `Lock-DataverseInvoicePricing`
15. UnlockInvoicePricingRequest → `Unlock-DataverseInvoicePricing`
16. MergeRequest → `Merge-DataverseRecord`
17. RouteToRequest → `Set-DataverseRecordRoute`

## Implementation Instructions

### Quick Reference for Each Cmdlet

**File Structure:**
- Cmdlet: `Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands/{VerbNoun}Cmdlet.cs`
- Documentation: `Rnwood.Dataverse.Data.PowerShell/docs/{Verb-Noun}.md`
- Tests: Add to `tests/Request-Cmdlets.Tests.ps1`

**Standard Pattern:**
```csharp
using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsXxx.Verb, "DataverseNoun", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(XxxResponse))]
    public class VerbDataverseNounCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true)]
        public override ServiceClient Connection { get; set; }

        // Add parameters matching request properties
        // Use DataverseTypeConverter.ToEntityReference() for EntityReference params
        // Use DataverseTypeConverter.ToOptionSetValue() for OptionSetValue params

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            
            var request = new XxxRequest
            {
                Property1 = Value1,
                Property2 = Value2
            };

            if (ShouldProcess("target", "action"))
            {
                var response = (XxxResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
```

**Test Pattern:**
```powershell
Describe 'Verb-DataverseNoun' {
    It "Does the operation" {
        $connection = getMockConnection
        # Test code
        $response = Verb-DataverseNoun -Connection $connection -Param1 $value1 -Confirm:$false
        $response | Should -Not -BeNullOrEmpty
    }
}
```

Use `-Skip:$true` if FakeXrmEasy doesn't support the request.

## Priority Tiers for Implementation

### 🔴 Priority 1: Core Operations (High Value, Common Use)
- [ ] WhoAmIRequest → `Get-DataverseWhoAmI` *(Already exists)*
- [ ] RetrieveRequest → `Get-DataverseRecordById` (alternative to Get-DataverseRecord)
- [ ] CreateRequest → Part of Set-DataverseRecord
- [ ] UpdateRequest → Part of Set-DataverseRecord
- [ ] DeleteRequest → Part of Remove-DataverseRecord
- [ ] RetrieveMultipleRequest → Part of Get-DataverseRecord
- [x] SendEmailRequest → `Send-DataverseEmail`
- [x] LockSalesOrderPricingRequest → `Lock-DataverseSalesOrderPricing`
- [x] UnlockSalesOrderPricingRequest → `Unlock-DataverseSalesOrderPricing`
- [x] LockInvoicePricingRequest → `Lock-DataverseInvoicePricing`
- [x] UnlockInvoicePricingRequest → `Unlock-DataverseInvoicePricing`
- [x] MergeRequest → `Merge-DataverseRecord`
- [x] RouteToRequest → `Set-DataverseRecordRoute`

### 🟠 Priority 2: Team & Security Management
- [ ] AddUserToRecordTeamRequest → `Add-DataverseRecordTeamMember`
- [ ] RemoveUserFromRecordTeamRequest → `Remove-DataverseRecordTeamMember`
- [ ] RetrieveTeamsSystemUserRequest → `Get-DataverseUserTeams`
- [ ] RetrieveMembersTeamRequest → `Get-DataverseTeamMembers`
- [ ] RetrievePrincipalAccessRequest → `Get-DataverseRecordAccess`
- [ ] RetrieveSharedPrincipalsAndAccessRequest → `Get-DataverseRecordSharing`
- [ ] ModifyAccessRequest → `Set-DataverseRecordAccess`
- [ ] AddPrivilegesRoleRequest → `Add-DataverseRolePrivileges`
- [ ] RemovePrivilegeRoleRequest → `Remove-DataverseRolePrivileges`
- [ ] RetrieveRolePrivilegesRoleRequest → `Get-DataverseRolePrivileges`

### 🟡 Priority 3: Solution Management
- [ ] ExportSolutionRequest → `Export-DataverseSolution`
- [ ] ImportSolutionRequest → `Import-DataverseSolution`
- [ ] ExportSolutionAsyncRequest → `Export-DataverseSolutionAsync`
- [ ] ImportSolutionAsyncRequest → `Import-DataverseSolutionAsync`
- [ ] PublishAllXmlRequest → `Publish-DataverseAllCustomizations`
- [ ] DeleteAndPromoteRequest → `Remove-DataverseActiveLayer`
- [ ] CloneAsSolutionRequest → `Copy-DataverseSolutionAsNew`
- [ ] CloneAsPatchRequest → `Copy-DataverseSolutionAsPatch`
- [ ] AddSolutionComponentRequest → `Add-DataverseSolutionComponent`
- [ ] RemoveSolutionComponentRequest → `Remove-DataverseSolutionComponent`

### 🟢 Priority 4: Duplicate Detection & Data Quality
- [ ] RetrieveDuplicatesRequest → `Get-DataverseDuplicates`
- [ ] BulkDetectDuplicatesRequest → `Find-DataverseDuplicatesBulk`
- [ ] PublishDuplicateRuleRequest → `Publish-DataverseDuplicateRule`
- [ ] UnpublishDuplicateRuleRequest → `Unpublish-DataverseDuplicateRule`

### 🔵 Priority 5: Business Process & Workflows
- [ ] SetProcessRequest → `Set-DataverseRecordProcess`
- [ ] RetrieveProcessInstancesRequest → `Get-DataverseProcessInstances`
- [ ] CreateWorkflowFromTemplateRequest → `New-DataverseWorkflowFromTemplate`

### 🟣 Priority 6: Bulk Operations
- [ ] BulkDeleteRequest → `Remove-DataverseRecordsBulk`
- [ ] BulkOperationRequest → `Invoke-DataverseBulkOperation`
- [ ] SendBulkMailRequest → `Send-DataverseEmailBulk`

### ⚪ Priority 7: Specialized Features (327 remaining)
All other request types grouped by functional area...

---

## Remaining Request Types (Alphabetical)

### A
- [ ] AddAppComponentsRequest
- [ ] AddChannelAccessProfilePrivilegesRequest
- [ ] AddItemCampaignActivityRequest
- [ ] AddItemCampaignRequest
- [ ] AddListMembersListRequest
- [ ] AddMemberListRequest
- [ ] AddPrincipalToQueueRequest
- [ ] AddPrivilegesRoleRequest
- [ ] AddProductToKitRequest
- [ ] AddRecurrenceRequest
- [ ] AddSolutionComponentRequest
- [ ] AddSubstituteProductRequest
- [ ] AddToQueueRequest
- [ ] AddUserToRecordTeamRequest
- [ ] ApplyRecordCreationAndUpdateRuleRequest
- [ ] ApplyRoutingRuleRequest
- [ ] AppointmentRequest
- [ ] AssociateEntitiesRequest
- [ ] AutoMapEntityRequest

### B
- [ ] BackgroundSendEmailRequest
- [ ] BookRequest
- [ ] BulkDeleteRequest
- [ ] BulkDetectDuplicatesRequest

### C
- [ ] CalculateActualValueOpportunityRequest
- [ ] CalculatePriceRequest
- [ ] CalculateRollupFieldRequest
- [ ] CalculateTotalTimeIncidentRequest
- [ ] CancelContractRequest
- [ ] CancelSalesOrderRequest
- [ ] CheckIncomingEmailRequest
- [ ] CheckPromoteEmailRequest
- [ ] CloneAsPatchRequest
- [ ] CloneAsSolutionRequest
- [ ] CloneContractRequest
- [ ] CloneMobileOfflineProfileRequest
- [ ] CloneProductRequest
- [ ] CloseIncidentRequest
- [ ] CloseQuoteRequest
- [ ] CommitAnnotationBlocksUploadRequest
- [ ] CommitAttachmentBlocksUploadRequest
- [ ] CommitFileBlocksUploadRequest
- [ ] CompoundCreateRequest
- [ ] CompoundUpdateDuplicateDetectionRuleRequest
- [ ] CompoundUpdateRequest
- [ ] ConvertKitToProductRequest
- [ ] ConvertOwnerTeamToAccessTeamRequest
- [ ] ConvertProductToKitRequest
- [ ] ConvertQuoteToSalesOrderRequest
- [ ] ConvertSalesOrderToInvoiceRequest
- [ ] CopyCampaignRequest
- [ ] CopyCampaignResponseRequest
- [ ] CopyDynamicListToStaticRequest
- [ ] CopyMembersListRequest
- [ ] CopySystemFormRequest
- [ ] CreateActivitiesListRequest
- [ ] CreateAsyncJobToRevokeInheritedAccessRequest
- [ ] CreateExceptionRequest
- [ ] CreateInstanceRequest
- [ ] CreateKnowledgeArticleTranslationRequest
- [ ] CreateKnowledgeArticleVersionRequest
- [ ] CreatePolymorphicLookupAttributeRequest
- [ ] CreateWorkflowFromTemplateRequest

### D
- [ ] DeleteAndPromoteAsyncRequest
- [ ] DeleteAndPromoteRequest
- [ ] DeleteAuditDataRequest
- [ ] DeleteFileRequest
- [ ] DeleteOpenInstancesRequest
- [ ] DeleteRecordChangeHistory1Request
- [ ] DeleteRecordChangeHistoryRequest
- [ ] DeliverImmediatePromoteEmailRequest
- [ ] DeliverIncomingEmailRequest
- [ ] DeliverPromoteEmailRequest
- [ ] DeprovisionLanguageRequest
- [ ] DisassociateEntitiesRequest
- [ ] DistributeCampaignActivityRequest
- [ ] DownloadBlockRequest
- [ ] DownloadReportDefinitionRequest
- [ ] DownloadSolutionExportDataRequest

### E
- [ ] ExecuteByIdSavedQueryRequest
- [ ] ExecuteByIdUserQueryRequest
- [ ] ExecuteFetchRequest
- [ ] ExpandCalendarRequest
- [ ] ExportFieldTranslationRequest
- [ ] ExportMappingsImportMapRequest
- [ ] ExportSolutionAsyncRequest
- [ ] ExportSolutionRequest
- [ ] ExportTranslationRequest

### F
- [ ] FetchXmlToQueryExpressionRequest
- [ ] FindParentResourceGroupRequest
- [ ] FormatAddressRequest
- [ ] FulfillSalesOrderRequest
- [ ] FullTextSearchKnowledgeArticleRequest

### G
- [ ] GenerateInvoiceFromOpportunityRequest
- [ ] GenerateQuoteFromOpportunityRequest
- [ ] GenerateSalesOrderFromOpportunityRequest
- [ ] GenerateSharedLinkRequest
- [ ] GenerateSocialProfileRequest
- [ ] GetAllTimeZonesWithDisplayNameRequest
- [ ] GetAutoNumberSeed1Request
- [ ] GetAutoNumberSeedRequest
- [ ] GetDecryptionKeyRequest
- [ ] GetDefaultPriceLevelRequest
- [ ] GetDistinctValuesImportFileRequest
- [ ] GetFileSasUrlRequest
- [ ] GetHeaderColumnsImportFileRequest
- [ ] GetInvoiceProductsFromOpportunityRequest
- [ ] GetNextAutoNumberValue1Request
- [ ] GetNextAutoNumberValueRequest
- [ ] GetPreferredSolutionRequest
- [ ] GetQuantityDecimalRequest
- [ ] GetQuoteProductsFromOpportunityRequest
- [ ] GetReportHistoryLimitRequest
- [ ] GetSalesOrderProductsFromOpportunityRequest
- [ ] GetTimeZoneCodeByLocalizedNameRequest
- [ ] GetTrackingTokenEmailRequest
- [ ] GrantAccessUsingSharedLinkRequest

### I
- [ ] ImmediateBookRequest
- [ ] ImportCardTypeSchemaRequest
- [ ] ImportFieldTranslationRequest
- [ ] ImportMappingsImportMapRequest
- [ ] ImportRecordsImportRequest
- [ ] ImportSolutionAsyncRequest
- [ ] ImportSolutionRequest
- [ ] ImportSolutionsRequest
- [ ] ImportTranslationAsyncRequest
- [ ] ImportTranslationRequest
- [ ] InitializeAnnotationBlocksDownloadRequest
- [ ] InitializeAnnotationBlocksUploadRequest
- [ ] InitializeAttachmentBlocksDownloadRequest
- [ ] InitializeAttachmentBlocksUploadRequest
- [ ] InitializeFileBlocksDownloadRequest
- [ ] InitializeFileBlocksUploadRequest
- [ ] InitializeFromRequest
- [ ] InstallSampleDataRequest
- [ ] InstantiateFiltersRequest
- [ ] InstantiateTemplateRequest
- [ ] IsBackOfficeInstalledRequest
- [ ] IsComponentCustomizableRequest
- [ ] IsDataEncryptionActiveRequest
- [ ] IsValidStateTransitionRequest

### L
- [ ] LocalTimeFromUtcTimeRequest
- [ ] LockInvoicePricingRequest
- [ ] LockSalesOrderPricingRequest
- [ ] LoseOpportunityRequest

### M
- [ ] MakeAvailableToOrganizationReportRequest
- [ ] MakeAvailableToOrganizationTemplateRequest
- [ ] MakeUnavailableToOrganizationReportRequest
- [ ] MakeUnavailableToOrganizationTemplateRequest
- [ ] MergeRequest
- [ ] ModifyAccessRequest

### O
- [ ] OrderOptionRequest
- [ ] OverridePriceListPriceListRequest

### P
- [ ] ParseImportRequest
- [ ] PickFromQueueRequest
- [ ] ProcessInboundEmailRequest
- [ ] ProcessOneMemberBulkOperationRequest
- [ ] PropagateByExpressionRequest
- [ ] ProvisionLanguageAsyncRequest
- [ ] ProvisionLanguageRequest
- [ ] PublishAllXmlRequest
- [ ] PublishDuplicateRuleRequest
- [ ] PublishProductHierarchyRequest
- [ ] PublishThemeRequest

### Q
- [ ] QualifyLeadRequest
- [ ] QualifyMemberListRequest
- [ ] QueryExpressionToFetchXmlRequest
- [ ] QueryMultipleSchedulesRequest
- [ ] QueryScheduleRequest

### R
- [ ] ReassignObjectsOwnerRequest
- [ ] ReassignObjectsSystemUserRequest
- [ ] RecalculateRequest
- [ ] ReleaseToQueueRequest
- [ ] RemoveAppComponentsRequest
- [ ] RemoveChannelAccessProfilePrivilegesRequest
- [ ] RemoveFromQueueRequest
- [ ] RemoveItemCampaignActivityRequest
- [ ] RemoveItemCampaignRequest
- [ ] RemoveMemberListRequest
- [ ] RemoveParentRequest
- [ ] RemovePrivilegeRoleRequest
- [ ] RemoveProductFromKitRequest
- [ ] RemoveSolutionComponentRequest
- [ ] RemoveSubstituteProductRequest
- [ ] RemoveUserFromRecordTeamRequest
- [ ] RenewContractRequest
- [ ] RenewEntitlementRequest
- [ ] ReplacePrivilegesRoleRequest
- [ ] RescheduleRequest
- [ ] ResetUserFiltersRequest
- [ ] RetrieveAadUserPrivilegesRequest
- [ ] RetrieveAadUserRolesRequest
- [ ] RetrieveAadUserSetOfPrivilegesByIdsRequest
- [ ] RetrieveAadUserSetOfPrivilegesByNamesRequest
- [ ] RetrieveAbsoluteAndSiteCollectionUrlRequest
- [ ] RetrieveActivePathRequest
- [ ] RetrieveAllChildUsersSystemUserRequest
- [ ] RetrieveAnalyticsStoreDetailsRequest
- [ ] RetrieveAppComponentsRequest
- [ ] RetrieveApplicationRibbonRequest
- [ ] RetrieveAttributeChangeHistoryRequest
- [ ] RetrieveAuditDetailsRequest
- [ ] RetrieveAuditPartitionListRequest
- [ ] RetrieveAvailableLanguagesRequest
- [ ] RetrieveBusinessHierarchyBusinessUnitRequest
- [ ] RetrieveByGroupResourceRequest
- [ ] RetrieveByResourceResourceGroupRequest
- [ ] RetrieveByResourcesServiceRequest
- [ ] RetrieveByTopIncidentProductKbArticleRequest
- [ ] RetrieveByTopIncidentSubjectKbArticleRequest
- [ ] RetrieveChannelAccessProfilePrivilegesRequest
- [ ] RetrieveCurrentOrganizationRequest
- [ ] RetrieveDependenciesForDeleteRequest
- [ ] RetrieveDependenciesForUninstallRequest
- [ ] RetrieveDependentComponentsRequest
- [ ] RetrieveDeploymentLicenseTypeRequest
- [ ] RetrieveDeprovisionedLanguagesRequest
- [ ] RetrieveDuplicatesRequest
- [ ] RetrieveEntityRibbonRequest
- [ ] RetrieveExchangeAppointmentsRequest
- [ ] RetrieveExchangeRateRequest
- [ ] RetrieveFeatureControlSettingRequest
- [ ] RetrieveFeatureControlSettingsByNamespaceRequest
- [ ] RetrieveFeatureControlSettingsRequest
- [ ] RetrieveFilteredFormsRequest
- [ ] RetrieveFormattedImportJobResultsRequest
- [ ] RetrieveFormXmlRequest
- [ ] RetrieveInstalledLanguagePacksRequest
- [ ] RetrieveInstalledLanguagePackVersionRequest
- [ ] RetrieveLicenseInfoRequest
- [ ] RetrieveLocLabelsRequest
- [ ] RetrieveMailboxTrackingFoldersRequest
- [ ] RetrieveMembersBulkOperationRequest
- [ ] RetrieveMembersTeamRequest
- [ ] RetrieveMissingComponentsRequest
- [ ] RetrieveMissingDependenciesRequest
- [ ] RetrieveOrganizationInfoRequest
- [ ] RetrieveOrganizationResourcesRequest
- [ ] RetrieveParentGroupsResourceGroupRequest
- [ ] RetrieveParsedDataImportFileRequest
- [ ] RetrievePersonalWallRequest
- [ ] RetrievePrincipalAccessInfoRequest
- [ ] RetrievePrincipalAccessRequest
- [ ] RetrievePrincipalAttributePrivilegesRequest
- [ ] RetrievePrincipalSyncAttributeMappingsRequest
- [ ] RetrievePrivilegeSetRequest
- [ ] RetrieveProcessInstancesRequest
- [ ] RetrieveProductPropertiesRequest
- [ ] RetrieveProvisionedLanguagePackVersionRequest
- [ ] RetrieveProvisionedLanguagesRequest
- [ ] RetrieveRecordChangeHistoryRequest
- [ ] RetrieveRecordWallRequest
- [ ] RetrieveRequiredComponentsRequest
- [ ] RetrieveRolePrivilegesRoleRequest
- [ ] RetrieveSharedLinksRequest
- [ ] RetrieveSharedPrincipalsAndAccessRequest
- [ ] RetrieveSubGroupsResourceGroupRequest
- [ ] RetrieveSubsidiaryTeamsBusinessUnitRequest
- [ ] RetrieveSubsidiaryUsersBusinessUnitRequest
- [ ] RetrieveTeamPrivilegesRequest
- [ ] RetrieveTeamsSystemUserRequest
- [ ] RetrieveTimelineWallRecordsRequest
- [ ] RetrieveTotalRecordCountRequest
- [ ] RetrieveUnpublishedMultipleRequest
- [ ] RetrieveUnpublishedRequest
- [ ] RetrieveUserLicenseInfoRequest
- [ ] RetrieveUserPrivilegeByPrivilegeIdRequest
- [ ] RetrieveUserPrivilegeByPrivilegeNameRequest
- [ ] RetrieveUserPrivilegesRequest
- [ ] RetrieveUserQueuesRequest
- [ ] RetrieveUserSetOfPrivilegesByIdsRequest
- [ ] RetrieveUserSetOfPrivilegesByNamesRequest
- [ ] RetrieveUserSettingsSystemUserRequest
- [ ] RetrieveUsersPrivilegesThroughTeamsRequest
- [ ] RetrieveVersionRequest
- [ ] RevertProductRequest
- [ ] ReviseQuoteRequest
- [ ] RevokeSharedLinkRequest
- [ ] RollupRequest
- [ ] RouteToRequest

### S
- [ ] SearchByBodyKbArticleRequest
- [ ] SearchByKeywordsKbArticleRequest
- [ ] SearchByTitleKbArticleRequest
- [ ] SearchRequest
- [ ] SendBulkMailRequest
- [ ] SendEmailFromTemplateRequest
- [ ] SendEmailRequest
- [ ] SendFaxRequest
- [ ] SendTemplateRequest
- [ ] SetAutoNumberSeed1Request
- [ ] SetAutoNumberSeedRequest
- [ ] SetBusinessEquipmentRequest
- [ ] SetBusinessSystemUserRequest
- [ ] SetFeatureStatusRequest
- [ ] SetLocLabelsRequest
- [ ] SetParentBusinessUnitRequest
- [ ] SetParentSystemUserRequest
- [ ] SetParentTeamRequest
- [ ] SetPreferredSolutionRequest
- [ ] SetProcessRequest
- [ ] SetRelatedRequest
- [ ] SetReportRelatedRequest
- [ ] StageAndUpgradeAsyncRequest
- [ ] StageAndUpgradeRequest
- [ ] StageSolutionRequest
- [ ] SyncBulkOperationRequest

### T
- [ ] TransformImportRequest
- [ ] TriggerServiceEndpointCheckRequest

### U
- [ ] UninstallSampleDataRequest
- [ ] UninstallSolutionAsyncRequest
- [ ] UnlockInvoicePricingRequest
- [ ] UnlockSalesOrderPricingRequest
- [ ] UnpublishDuplicateRuleRequest
- [ ] UpdateFeatureConfigRequest
- [ ] UpdateProductPropertiesRequest
- [ ] UpdateRibbonClientMetadataRequest
- [ ] UpdateSolutionComponentRequest
- [ ] UpdateUserSettingsSystemUserRequest
- [ ] UploadBlockRequest
- [ ] UtcTimeFromLocalTimeRequest

### V
- [ ] ValidateAppRequest
- [ ] ValidateFetchXmlExpressionRequest
- [ ] ValidateRecurrenceRuleRequest
- [ ] ValidateRequest
- [ ] ValidateSavedQueryRequest
- [ ] ValidateUnpublishedRequest

### W
- [ ] WhoAmIRequest → *(Already implemented as Get-DataverseWhoAmI)*
- [ ] WinOpportunityRequest
- [ ] WinQuoteRequest

---

## Build & Test Process

```bash
# Build cmdlets
dotnet build -c Release ./Rnwood.Dataverse.Data.PowerShell.Cmdlets/Rnwood.Dataverse.Data.PowerShell.Cmdlets.csproj

# Build loader
dotnet build -c Release ./Rnwood.Dataverse.Data.PowerShell.Loader/Rnwood.Dataverse.Data.PowerShell.Loader.csproj

# Assemble module
mkdir -p Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0
cp Rnwood.Dataverse.Data.PowerShell/*.psd1 Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/
cp Rnwood.Dataverse.Data.PowerShell/*.psm1 Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/
mkdir -p Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/cmdlets
cp -r Rnwood.Dataverse.Data.PowerShell.Cmdlets/bin/Release/* Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/cmdlets/
mkdir -p Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/loader
cp -r Rnwood.Dataverse.Data.PowerShell.Loader/bin/Release/* Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/loader/
rm -rf out && mkdir -p out && cp -r Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0 out/Rnwood.Dataverse.Data.PowerShell

# Test
export TESTMODULEPATH=$(pwd)/out/Rnwood.Dataverse.Data.PowerShell
pwsh -Command "Invoke-Pester -Output Detailed -Path tests"
```

## Utility Classes Available

- **DataverseTypeConverter**: 
  - `ToEntityReference(object, tableName, paramName)` - Convert PSObject/Guid/string to EntityReference
  - `ToOptionSetValue(object, paramName)` - Convert int/string to OptionSetValue

- **DataverseEntityConverter**: For complex entity conversion (already exists)

## Notes

- All cmdlets must inherit from `OrganizationServiceCmdlet`
- Use `SupportsShouldProcess = true` for destructive operations
- Set appropriate `ConfirmImpact` (Low/Medium/High)
- Use DataverseTypeConverter for common conversions
- Document with XML comments on cmdlet class
- Create comprehensive markdown documentation
- Add tests with `-Skip:$true` if FakeXrmEasy doesn't support
- Follow PowerShell approved verbs (Get-Verb to see list)
