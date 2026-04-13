# Request Handler Reference

This reference compares Dataverse SDK request types with Fake4Dataverse handler support.

Fake4Dataverse includes **245 implemented** request handlers out of **392** discovered SDK request types.
Of those, **110** are stubs and **147** are currently not implemented.

## Microsoft.Crm.Sdk.Messages

| Request Type | Request Name | Status | Fidelity | Description |
|---|---|---|---|---|
| `AddAppComponentsRequest` | `AddAppComponents` | ❌ Not implemented | — | — |
| `AddChannelAccessProfilePrivilegesRequest` | `AddChannelAccessProfilePrivileges` | ❌ Not implemented | — | — |
| `AddItemCampaignActivityRequest` | `AddItemCampaignActivity` | ✅ Implemented | Stub | — |
| `AddItemCampaignRequest` | `AddItemCampaign` | ✅ Implemented | Stub | — |
| `AddListMembersListRequest` | `AddListMembersList` | ✅ Implemented | Partial | — |
| `AddMemberListRequest` | `AddMemberList` | ✅ Implemented | Stub | — |
| `AddMembersTeamRequest` | `AddMembersTeam` | ✅ Implemented | Partial | — |
| `AddPrincipalToQueueRequest` | `AddPrincipalToQueue` | ✅ Implemented | Stub | — |
| `AddPrivilegesRoleRequest` | `AddPrivilegesRole` | ✅ Implemented | Stub | — |
| `AddProductToKitRequest` | `AddProductToKit` | ❌ Not implemented | — | — |
| `AddRecurrenceRequest` | `AddRecurrence` | ❌ Not implemented | — | — |
| `AddSolutionComponentRequest` | `AddSolutionComponent` | ✅ Implemented | Stub | — |
| `AddSubstituteProductRequest` | `AddSubstituteProduct` | ✅ Implemented | Stub | — |
| `AddToQueueRequest` | `AddToQueue` | ✅ Implemented | Partial | — |
| `AddUserToRecordTeamRequest` | `AddUserToRecordTeam` | ✅ Implemented | Partial | — |
| `ApplyRecordCreationAndUpdateRuleRequest` | `ApplyRecordCreationAndUpdateRule` | ❌ Not implemented | — | — |
| `ApplyRoutingRuleRequest` | `ApplyRoutingRule` | ❌ Not implemented | — | — |
| `AssignRequest` | `Assign` | ✅ Implemented | Full | — |
| `AssociateEntitiesRequest` | `AssociateEntities` | ❌ Not implemented | — | — |
| `AutoMapEntityRequest` | `AutoMapEntity` | ❌ Not implemented | — | — |
| `BackgroundSendEmailRequest` | `BackgroundSendEmail` | ✅ Implemented | Full | — |
| `BookRequest` | `Book` | ❌ Not implemented | — | — |
| `BulkDeleteRequest` | `BulkDelete` | ✅ Implemented | Partial | — |
| `BulkDetectDuplicatesRequest` | `BulkDetectDuplicates` | ✅ Implemented | Stub | — |
| `CalculateActualValueOpportunityRequest` | `CalculateActualValueOpportunity` | ✅ Implemented | Stub | — |
| `CalculatePriceRequest` | `CalculatePrice` | ❌ Not implemented | — | — |
| `CalculateRollupFieldRequest` | `CalculateRollupField` | ✅ Implemented | Partial | — |
| `CalculateTotalTimeIncidentRequest` | `CalculateTotalTimeIncident` | ✅ Implemented | Stub | — |
| `CancelContractRequest` | `CancelContract` | ✅ Implemented | Stub | — |
| `CancelSalesOrderRequest` | `CancelSalesOrder` | ✅ Implemented | Full | — |
| `CheckIncomingEmailRequest` | `CheckIncomingEmail` | ✅ Implemented | Partial | — |
| `CheckPromoteEmailRequest` | `CheckPromoteEmail` | ✅ Implemented | Partial | — |
| `CloneAsPatchRequest` | `CloneAsPatch` | ❌ Not implemented | — | — |
| `CloneAsSolutionRequest` | `CloneAsSolution` | ❌ Not implemented | — | — |
| `CloneContractRequest` | `CloneContract` | ✅ Implemented | Stub | — |
| `CloneMobileOfflineProfileRequest` | `CloneMobileOfflineProfile` | ❌ Not implemented | — | — |
| `CloneProductRequest` | `CloneProduct` | ✅ Implemented | Stub | — |
| `CloseIncidentRequest` | `CloseIncident` | ✅ Implemented | Full | — |
| `CloseQuoteRequest` | `CloseQuote` | ✅ Implemented | Full | — |
| `CommitAnnotationBlocksUploadRequest` | `CommitAnnotationBlocksUpload` | ✅ Implemented | Partial | — |
| `CommitAttachmentBlocksUploadRequest` | `CommitAttachmentBlocksUpload` | ✅ Implemented | Partial | — |
| `CommitFileBlocksUploadRequest` | `CommitFileBlocksUpload` | ✅ Implemented | Partial | — |
| `CompoundCreateRequest` | `CompoundCreate` | ❌ Not implemented | — | — |
| `CompoundUpdateDuplicateDetectionRuleRequest` | `CompoundUpdateDuplicateDetectionRule` | ❌ Not implemented | — | — |
| `CompoundUpdateRequest` | `CompoundUpdate` | ❌ Not implemented | — | — |
| `ConvertKitToProductRequest` | `ConvertKitToProduct` | ❌ Not implemented | — | — |
| `ConvertOwnerTeamToAccessTeamRequest` | `ConvertOwnerTeamToAccessTeam` | ✅ Implemented | Stub | — |
| `ConvertProductToKitRequest` | `ConvertProductToKit` | ❌ Not implemented | — | — |
| `ConvertQuoteToSalesOrderRequest` | `ConvertQuoteToSalesOrder` | ✅ Implemented | Partial | — |
| `ConvertSalesOrderToInvoiceRequest` | `ConvertSalesOrderToInvoice` | ✅ Implemented | Partial | — |
| `CopyCampaignRequest` | `CopyCampaign` | ✅ Implemented | Stub | — |
| `CopyCampaignResponseRequest` | `CopyCampaignResponse` | ✅ Implemented | Stub | — |
| `CopyDynamicListToStaticRequest` | `CopyDynamicListToStatic` | ✅ Implemented | Stub | — |
| `CopyMembersListRequest` | `CopyMembersList` | ✅ Implemented | Stub | — |
| `CopySystemFormRequest` | `CopySystemForm` | ❌ Not implemented | — | — |
| `CreateActivitiesListRequest` | `CreateActivitiesList` | ❌ Not implemented | — | — |
| `CreateExceptionRequest` | `CreateException` | ❌ Not implemented | — | — |
| `CreateInstanceRequest` | `CreateInstance` | ❌ Not implemented | — | — |
| `CreateKnowledgeArticleTranslationRequest` | `CreateKnowledgeArticleTranslation` | ✅ Implemented | Stub | — |
| `CreateKnowledgeArticleVersionRequest` | `CreateKnowledgeArticleVersion` | ✅ Implemented | Stub | — |
| `CreatePolymorphicLookupAttributeRequest` | `CreatePolymorphicLookupAttribute` | ❌ Not implemented | — | — |
| `CreateWorkflowFromTemplateRequest` | `CreateWorkflowFromTemplate` | ❌ Not implemented | — | — |
| `DeleteAndPromoteAsyncRequest` | `DeleteAndPromoteAsync` | ❌ Not implemented | — | — |
| `DeleteAndPromoteRequest` | `DeleteAndPromote` | ❌ Not implemented | — | — |
| `DeleteAuditDataRequest` | `DeleteAuditData` | ✅ Implemented | Stub | — |
| `DeleteFileRequest` | `DeleteFile` | ✅ Implemented | Partial | — |
| `DeleteOpenInstancesRequest` | `DeleteOpenInstances` | ❌ Not implemented | — | — |
| `DeleteRecordChangeHistoryRequest` | `DeleteRecordChangeHistory` | ✅ Implemented | Stub | — |
| `DeliverImmediatePromoteEmailRequest` | `DeliverImmediatePromoteEmail` | ❌ Not implemented | — | — |
| `DeliverIncomingEmailRequest` | `DeliverIncomingEmail` | ❌ Not implemented | — | — |
| `DeliverPromoteEmailRequest` | `DeliverPromoteEmail` | ❌ Not implemented | — | — |
| `DeprovisionLanguageRequest` | `DeprovisionLanguage` | ❌ Not implemented | — | — |
| `DisassociateEntitiesRequest` | `DisassociateEntities` | ❌ Not implemented | — | — |
| `DistributeCampaignActivityRequest` | `DistributeCampaignActivity` | ❌ Not implemented | — | — |
| `DownloadBlockRequest` | `DownloadBlock` | ✅ Implemented | Partial | — |
| `DownloadReportDefinitionRequest` | `DownloadReportDefinition` | ❌ Not implemented | — | — |
| `DownloadSolutionExportDataRequest` | `DownloadSolutionExportData` | ❌ Not implemented | — | — |
| `ExecuteByIdSavedQueryRequest` | `ExecuteByIdSavedQuery` | ✅ Implemented | Partial | — |
| `ExecuteByIdUserQueryRequest` | `ExecuteByIdUserQuery` | ✅ Implemented | Partial | — |
| `ExecuteFetchRequest` | `ExecuteFetch` | ✅ Implemented | Partial | — |
| `ExecuteWorkflowRequest` | `ExecuteWorkflow` | ✅ Implemented | Stub | — |
| `ExpandCalendarRequest` | `ExpandCalendar` | ❌ Not implemented | — | — |
| `ExportFieldTranslationRequest` | `ExportFieldTranslation` | ❌ Not implemented | — | — |
| `ExportMappingsImportMapRequest` | `ExportMappingsImportMap` | ❌ Not implemented | — | — |
| `ExportSolutionAsyncRequest` | `ExportSolutionAsync` | ❌ Not implemented | — | — |
| `ExportSolutionRequest` | `ExportSolution` | ❌ Not implemented | — | — |
| `ExportTranslationRequest` | `ExportTranslation` | ❌ Not implemented | — | — |
| `FetchXmlToQueryExpressionRequest` | `FetchXmlToQueryExpression` | ✅ Implemented | Partial | — |
| `FindParentResourceGroupRequest` | `FindParentResourceGroup` | ❌ Not implemented | — | — |
| `FormatAddressRequest` | `FormatAddress` | ✅ Implemented | Stub | — |
| `FulfillSalesOrderRequest` | `FulfillSalesOrder` | ✅ Implemented | Full | — |
| `FullTextSearchKnowledgeArticleRequest` | `FullTextSearchKnowledgeArticle` | ❌ Not implemented | — | — |
| `GenerateInvoiceFromOpportunityRequest` | `GenerateInvoiceFromOpportunity` | ✅ Implemented | Partial | — |
| `GenerateQuoteFromOpportunityRequest` | `GenerateQuoteFromOpportunity` | ✅ Implemented | Partial | — |
| `GenerateSalesOrderFromOpportunityRequest` | `GenerateSalesOrderFromOpportunity` | ✅ Implemented | Partial | — |
| `GenerateSharedLinkRequest` | `GenerateSharedLink` | ❌ Not implemented | — | — |
| `GenerateSocialProfileRequest` | `GenerateSocialProfile` | ❌ Not implemented | — | — |
| `GetAllTimeZonesWithDisplayNameRequest` | `GetAllTimeZonesWithDisplayName` | ✅ Implemented | Stub | — |
| `GetAutoNumberSeedRequest` | `GetAutoNumberSeed` | ✅ Implemented | Partial | — |
| `GetDecryptionKeyRequest` | `GetDecryptionKey` | ❌ Not implemented | — | — |
| `GetDefaultPriceLevelRequest` | `GetDefaultPriceLevel` | ✅ Implemented | Stub | — |
| `GetDistinctValuesImportFileRequest` | `GetDistinctValuesImportFile` | ❌ Not implemented | — | — |
| `GetFileSasUrlRequest` | `GetFileSasUrl` | ✅ Implemented | Stub | — |
| `GetHeaderColumnsImportFileRequest` | `GetHeaderColumnsImportFile` | ❌ Not implemented | — | — |
| `GetInvoiceProductsFromOpportunityRequest` | `GetInvoiceProductsFromOpportunity` | ✅ Implemented | Stub | — |
| `GetNextAutoNumberValueRequest` | `GetNextAutoNumberValue` | ✅ Implemented | Partial | — |
| `GetPreferredSolutionRequest` | `GetPreferredSolution` | ❌ Not implemented | — | — |
| `GetQuantityDecimalRequest` | `GetQuantityDecimal` | ✅ Implemented | Stub | — |
| `GetQuoteProductsFromOpportunityRequest` | `GetQuoteProductsFromOpportunity` | ✅ Implemented | Stub | — |
| `GetReportHistoryLimitRequest` | `GetReportHistoryLimit` | ✅ Implemented | Stub | — |
| `GetSalesOrderProductsFromOpportunityRequest` | `GetSalesOrderProductsFromOpportunity` | ✅ Implemented | Stub | — |
| `GetTimeZoneCodeByLocalizedNameRequest` | `GetTimeZoneCodeByLocalizedName` | ✅ Implemented | Stub | — |
| `GetTrackingTokenEmailRequest` | `GetTrackingTokenEmail` | ✅ Implemented | Partial | — |
| `GrantAccessRequest` | `GrantAccess` | ✅ Implemented | Full | — |
| `GrantAccessUsingSharedLinkRequest` | `GrantAccessUsingSharedLink` | ❌ Not implemented | — | — |
| `ImmediateBookRequest` | `ImmediateBook` | ❌ Not implemented | — | — |
| `ImportCardTypeSchemaRequest` | `ImportCardTypeSchema` | ❌ Not implemented | — | — |
| `ImportFieldTranslationRequest` | `ImportFieldTranslation` | ❌ Not implemented | — | — |
| `ImportMappingsImportMapRequest` | `ImportMappingsImportMap` | ❌ Not implemented | — | — |
| `ImportRecordsImportRequest` | `ImportRecordsImport` | ❌ Not implemented | — | — |
| `ImportSolutionAsyncRequest` | `ImportSolutionAsync` | ❌ Not implemented | — | — |
| `ImportSolutionRequest` | `ImportSolution` | ❌ Not implemented | — | — |
| `ImportSolutionsRequest` | `ImportSolutions` | ❌ Not implemented | — | — |
| `ImportTranslationAsyncRequest` | `ImportTranslationAsync` | ❌ Not implemented | — | — |
| `ImportTranslationRequest` | `ImportTranslation` | ❌ Not implemented | — | — |
| `IncrementKnowledgeArticleViewCountRequest` | `IncrementKnowledgeArticleViewCount` | ✅ Implemented | Stub | — |
| `InitializeAnnotationBlocksDownloadRequest` | `InitializeAnnotationBlocksDownload` | ✅ Implemented | Partial | — |
| `InitializeAnnotationBlocksUploadRequest` | `InitializeAnnotationBlocksUpload` | ✅ Implemented | Partial | — |
| `InitializeAttachmentBlocksDownloadRequest` | `InitializeAttachmentBlocksDownload` | ✅ Implemented | Partial | — |
| `InitializeAttachmentBlocksUploadRequest` | `InitializeAttachmentBlocksUpload` | ✅ Implemented | Partial | — |
| `InitializeFileBlocksDownloadRequest` | `InitializeFileBlocksDownload` | ✅ Implemented | Partial | — |
| `InitializeFileBlocksUploadRequest` | `InitializeFileBlocksUpload` | ✅ Implemented | Full | — |
| `InitializeFromRequest` | `InitializeFrom` | ✅ Implemented | Partial | — |
| `InitializeModernFlowFromAsyncWorkflowRequest` | `InitializeModernFlowFromAsyncWorkflow` | ❌ Not implemented | — | — |
| `InstallSampleDataRequest` | `InstallSampleData` | ❌ Not implemented | — | — |
| `InstantiateFiltersRequest` | `InstantiateFilters` | ❌ Not implemented | — | — |
| `InstantiateTemplateRequest` | `InstantiateTemplate` | ✅ Implemented | Full | — |
| `IsBackOfficeInstalledRequest` | `IsBackOfficeInstalled` | ✅ Implemented | Stub | — |
| `IsComponentCustomizableRequest` | `IsComponentCustomizable` | ✅ Implemented | Stub | — |
| `IsValidStateTransitionRequest` | `IsValidStateTransition` | ✅ Implemented | Full | — |
| `LocalTimeFromUtcTimeRequest` | `LocalTimeFromUtcTime` | ✅ Implemented | Stub | — |
| `LockInvoicePricingRequest` | `LockInvoicePricing` | ✅ Implemented | Full | — |
| `LockSalesOrderPricingRequest` | `LockSalesOrderPricing` | ✅ Implemented | Full | — |
| `LoseOpportunityRequest` | `LoseOpportunity` | ✅ Implemented | Full | — |
| `MakeAvailableToOrganizationReportRequest` | `MakeAvailableToOrganizationReport` | ❌ Not implemented | — | — |
| `MakeAvailableToOrganizationTemplateRequest` | `MakeAvailableToOrganizationTemplate` | ✅ Implemented | Stub | — |
| `MakeUnavailableToOrganizationReportRequest` | `MakeUnavailableToOrganizationReport` | ❌ Not implemented | — | — |
| `MakeUnavailableToOrganizationTemplateRequest` | `MakeUnavailableToOrganizationTemplate` | ✅ Implemented | Stub | — |
| `MergeRequest` | `Merge` | ✅ Implemented | Partial | — |
| `ModifyAccessRequest` | `ModifyAccess` | ✅ Implemented | Full | — |
| `ParseImportRequest` | `ParseImport` | ❌ Not implemented | — | — |
| `PickFromQueueRequest` | `PickFromQueue` | ✅ Implemented | Partial | — |
| `PreferredSolutionUsedByRequest` | `PreferredSolutionUsedBy` | ❌ Not implemented | — | — |
| `ProcessInboundEmailRequest` | `ProcessInboundEmail` | ❌ Not implemented | — | — |
| `PropagateByExpressionRequest` | `PropagateByExpression` | ❌ Not implemented | — | — |
| `ProvisionLanguageAsyncRequest` | `ProvisionLanguageAsync` | ❌ Not implemented | — | — |
| `ProvisionLanguageRequest` | `ProvisionLanguage` | ❌ Not implemented | — | — |
| `PublishAllXmlAsyncRequest` | `PublishAllXmlAsync` | ✅ Implemented | ? | — |
| `PublishAllXmlRequest` | `PublishAllXml` | ✅ Implemented | ? | — |
| `PublishDuplicateRuleRequest` | `PublishDuplicateRule` | ✅ Implemented | Stub | — |
| `PublishProductHierarchyRequest` | `PublishProductHierarchy` | ✅ Implemented | Stub | — |
| `PublishThemeRequest` | `PublishTheme` | ✅ Implemented | Stub | — |
| `PublishXmlRequest` | `PublishXml` | ✅ Implemented | ? | — |
| `QualifyLeadRequest` | `QualifyLead` | ✅ Implemented | Full | — |
| `QualifyMemberListRequest` | `QualifyMemberList` | ✅ Implemented | Stub | — |
| `QueryExpressionToFetchXmlRequest` | `QueryExpressionToFetchXml` | ✅ Implemented | Partial | — |
| `QueryMultipleSchedulesRequest` | `QueryMultipleSchedules` | ❌ Not implemented | — | — |
| `QueryScheduleRequest` | `QuerySchedule` | ❌ Not implemented | — | — |
| `QueueUpdateRibbonClientMetadataRequest` | `QueueUpdateRibbonClientMetadata` | ❌ Not implemented | — | — |
| `ReassignObjectsOwnerRequest` | `ReassignObjectsOwner` | ✅ Implemented | Stub | — |
| `ReassignObjectsSystemUserRequest` | `ReassignObjectsSystemUser` | ✅ Implemented | Stub | — |
| `RecalculateRequest` | `Recalculate` | ✅ Implemented | Partial | — |
| `ReleaseToQueueRequest` | `ReleaseToQueue` | ✅ Implemented | Partial | — |
| `RemoveAppComponentsRequest` | `RemoveAppComponents` | ❌ Not implemented | — | — |
| `RemoveFromQueueRequest` | `RemoveFromQueue` | ✅ Implemented | Partial | — |
| `RemoveItemCampaignActivityRequest` | `RemoveItemCampaignActivity` | ✅ Implemented | Stub | — |
| `RemoveItemCampaignRequest` | `RemoveItemCampaign` | ✅ Implemented | Stub | — |
| `RemoveMemberListRequest` | `RemoveMemberList` | ✅ Implemented | Partial | — |
| `RemoveMembersTeamRequest` | `RemoveMembersTeam` | ✅ Implemented | Partial | — |
| `RemoveParentRequest` | `RemoveParent` | ✅ Implemented | Full | — |
| `RemovePrivilegeRoleRequest` | `RemovePrivilegeRole` | ✅ Implemented | Stub | — |
| `RemoveProductFromKitRequest` | `RemoveProductFromKit` | ❌ Not implemented | — | — |
| `RemoveRelatedRequest` | `RemoveRelated` | ❌ Not implemented | — | — |
| `RemoveSolutionComponentRequest` | `RemoveSolutionComponent` | ✅ Implemented | Stub | — |
| `RemoveSubstituteProductRequest` | `RemoveSubstituteProduct` | ✅ Implemented | Stub | — |
| `RemoveUserFromRecordTeamRequest` | `RemoveUserFromRecordTeam` | ✅ Implemented | Partial | — |
| `RenewContractRequest` | `RenewContract` | ✅ Implemented | Partial | — |
| `RenewEntitlementRequest` | `RenewEntitlement` | ✅ Implemented | Stub | — |
| `ReplacePrivilegesRoleRequest` | `ReplacePrivilegesRole` | ✅ Implemented | Stub | — |
| `RescheduleRequest` | `Reschedule` | ✅ Implemented | Partial | — |
| `ResetUserFiltersRequest` | `ResetUserFilters` | ❌ Not implemented | — | — |
| `RetrieveAadUserPrivilegesRequest` | `RetrieveAadUserPrivileges` | ❌ Not implemented | — | — |
| `RetrieveAadUserRolesRequest` | `RetrieveAadUserRoles` | ❌ Not implemented | — | — |
| `RetrieveAadUserSetOfPrivilegesByIdsRequest` | `RetrieveAadUserSetOfPrivilegesByIds` | ❌ Not implemented | — | — |
| `RetrieveAadUserSetOfPrivilegesByNamesRequest` | `RetrieveAadUserSetOfPrivilegesByNames` | ❌ Not implemented | — | — |
| `RetrieveAbsoluteAndSiteCollectionUrlRequest` | `RetrieveAbsoluteAndSiteCollectionUrl` | ❌ Not implemented | — | — |
| `RetrieveActivePathRequest` | `RetrieveActivePath` | ❌ Not implemented | — | — |
| `RetrieveAllChildUsersSystemUserRequest` | `RetrieveAllChildUsersSystemUser` | ✅ Implemented | Stub | — |
| `RetrieveAnalyticsStoreDetailsRequest` | `RetrieveAnalyticsStoreDetails` | ❌ Not implemented | — | — |
| `RetrieveAppComponentsRequest` | `RetrieveAppComponents` | ❌ Not implemented | — | — |
| `RetrieveApplicationRibbonRequest` | `RetrieveApplicationRibbon` | ❌ Not implemented | — | — |
| `RetrieveAttributeChangeHistoryRequest` | `RetrieveAttributeChangeHistory` | ✅ Implemented | Stub | — |
| `RetrieveAuditDetailsRequest` | `RetrieveAuditDetails` | ✅ Implemented | Stub | — |
| `RetrieveAuditPartitionListRequest` | `RetrieveAuditPartitionList` | ✅ Implemented | Stub | — |
| `RetrieveAvailableLanguagesRequest` | `RetrieveAvailableLanguages` | ✅ Implemented | Stub | — |
| `RetrieveBusinessHierarchyBusinessUnitRequest` | `RetrieveBusinessHierarchyBusinessUnit` | ✅ Implemented | Stub | — |
| `RetrieveByGroupResourceRequest` | `RetrieveByGroupResource` | ❌ Not implemented | — | — |
| `RetrieveByResourceResourceGroupRequest` | `RetrieveByResourceResourceGroup` | ❌ Not implemented | — | — |
| `RetrieveByResourcesServiceRequest` | `RetrieveByResourcesService` | ❌ Not implemented | — | — |
| `RetrieveByTopIncidentProductKbArticleRequest` | `RetrieveByTopIncidentProductKbArticle` | ❌ Not implemented | — | — |
| `RetrieveByTopIncidentSubjectKbArticleRequest` | `RetrieveByTopIncidentSubjectKbArticle` | ❌ Not implemented | — | — |
| `RetrieveChannelAccessProfilePrivilegesRequest` | `RetrieveChannelAccessProfilePrivileges` | ❌ Not implemented | — | — |
| `RetrieveCurrentOrganizationRequest` | `RetrieveCurrentOrganization` | ✅ Implemented | Full | — |
| `RetrieveDependenciesForDeleteRequest` | `RetrieveDependenciesForDelete` | ❌ Not implemented | — | — |
| `RetrieveDependenciesForUninstallRequest` | `RetrieveDependenciesForUninstall` | ❌ Not implemented | — | — |
| `RetrieveDependentComponentsRequest` | `RetrieveDependentComponents` | ❌ Not implemented | — | — |
| `RetrieveDeploymentLicenseTypeRequest` | `RetrieveDeploymentLicenseType` | ✅ Implemented | Stub | — |
| `RetrieveDeprovisionedLanguagesRequest` | `RetrieveDeprovisionedLanguages` | ❌ Not implemented | — | — |
| `RetrieveDuplicatesRequest` | `RetrieveDuplicates` | ✅ Implemented | Stub | — |
| `RetrieveEntityRibbonRequest` | `RetrieveEntityRibbon` | ❌ Not implemented | — | — |
| `RetrieveExchangeAppointmentsRequest` | `RetrieveExchangeAppointments` | ❌ Not implemented | — | — |
| `RetrieveExchangeRateRequest` | `RetrieveExchangeRate` | ✅ Implemented | Full | — |
| `RetrieveFeatureControlSettingRequest` | `RetrieveFeatureControlSetting` | ✅ Implemented | Stub | — |
| `RetrieveFeatureControlSettingsByNamespaceRequest` | `RetrieveFeatureControlSettingsByNamespace` | ✅ Implemented | Stub | — |
| `RetrieveFeatureControlSettingsRequest` | `RetrieveFeatureControlSettings` | ✅ Implemented | Stub | — |
| `RetrieveFilteredFormsRequest` | `RetrieveFilteredForms` | ❌ Not implemented | — | — |
| `RetrieveFormattedImportJobResultsRequest` | `RetrieveFormattedImportJobResults` | ❌ Not implemented | — | — |
| `RetrieveFormXmlRequest` | `RetrieveFormXml` | ❌ Not implemented | — | — |
| `RetrieveInstalledLanguagePacksRequest` | `RetrieveInstalledLanguagePacks` | ✅ Implemented | Stub | — |
| `RetrieveInstalledLanguagePackVersionRequest` | `RetrieveInstalledLanguagePackVersion` | ✅ Implemented | Stub | — |
| `RetrieveLicenseInfoRequest` | `RetrieveLicenseInfo` | ✅ Implemented | Stub | — |
| `RetrieveLocLabelsRequest` | `RetrieveLocLabels` | ❌ Not implemented | — | — |
| `RetrieveMailboxTrackingFoldersRequest` | `RetrieveMailboxTrackingFolders` | ❌ Not implemented | — | — |
| `RetrieveMembersBulkOperationRequest` | `RetrieveMembersBulkOperation` | ❌ Not implemented | — | — |
| `RetrieveMembersTeamRequest` | `RetrieveMembersTeam` | ✅ Implemented | Partial | — |
| `RetrieveMissingComponentsRequest` | `RetrieveMissingComponents` | ❌ Not implemented | — | — |
| `RetrieveMissingDependenciesRequest` | `RetrieveMissingDependencies` | ❌ Not implemented | — | — |
| `RetrieveOrganizationInfoRequest` | `RetrieveOrganizationInfo` | ✅ Implemented | Stub | — |
| `RetrieveOrganizationResourcesRequest` | `RetrieveOrganizationResources` | ❌ Not implemented | — | — |
| `RetrieveParentGroupsResourceGroupRequest` | `RetrieveParentGroupsResourceGroup` | ❌ Not implemented | — | — |
| `RetrieveParsedDataImportFileRequest` | `RetrieveParsedDataImportFile` | ❌ Not implemented | — | — |
| `RetrievePersonalWallRequest` | `RetrievePersonalWall` | ❌ Not implemented | — | — |
| `RetrievePrincipalAccessInfoRequest` | `RetrievePrincipalAccessInfo` | ✅ Implemented | Stub | — |
| `RetrievePrincipalAccessRequest` | `RetrievePrincipalAccess` | ✅ Implemented | Full | — |
| `RetrievePrincipalAttributePrivilegesRequest` | `RetrievePrincipalAttributePrivileges` | ❌ Not implemented | — | — |
| `RetrievePrincipalSyncAttributeMappingsRequest` | `RetrievePrincipalSyncAttributeMappings` | ❌ Not implemented | — | — |
| `RetrievePrivilegeSetRequest` | `RetrievePrivilegeSet` | ❌ Not implemented | — | — |
| `RetrieveProcessInstancesRequest` | `RetrieveProcessInstances` | ✅ Implemented | Stub | — |
| `RetrieveProductPropertiesRequest` | `RetrieveProductProperties` | ✅ Implemented | Stub | — |
| `RetrieveProvisionedLanguagePackVersionRequest` | `RetrieveProvisionedLanguagePackVersion` | ✅ Implemented | Stub | — |
| `RetrieveProvisionedLanguagesRequest` | `RetrieveProvisionedLanguages` | ✅ Implemented | Stub | — |
| `RetrieveRecordChangeHistoryRequest` | `RetrieveRecordChangeHistory` | ✅ Implemented | Stub | — |
| `RetrieveRecordWallRequest` | `RetrieveRecordWall` | ❌ Not implemented | — | — |
| `RetrieveRequiredComponentsRequest` | `RetrieveRequiredComponents` | ❌ Not implemented | — | — |
| `RetrieveRolePrivilegesRoleRequest` | `RetrieveRolePrivilegesRole` | ✅ Implemented | Stub | — |
| `RetrieveSharedLinksRequest` | `RetrieveSharedLinks` | ❌ Not implemented | — | — |
| `RetrieveSharedPrincipalsAndAccessRequest` | `RetrieveSharedPrincipalsAndAccess` | ✅ Implemented | Full | — |
| `RetrieveSubGroupsResourceGroupRequest` | `RetrieveSubGroupsResourceGroup` | ❌ Not implemented | — | — |
| `RetrieveSubsidiaryTeamsBusinessUnitRequest` | `RetrieveSubsidiaryTeamsBusinessUnit` | ✅ Implemented | Stub | — |
| `RetrieveSubsidiaryUsersBusinessUnitRequest` | `RetrieveSubsidiaryUsersBusinessUnit` | ✅ Implemented | Stub | — |
| `RetrieveTeamPrivilegesRequest` | `RetrieveTeamPrivileges` | ✅ Implemented | Stub | — |
| `RetrieveTeamsSystemUserRequest` | `RetrieveTeamsSystemUser` | ✅ Implemented | Stub | — |
| `RetrieveTimelineWallRecordsRequest` | `RetrieveTimelineWallRecords` | ❌ Not implemented | — | — |
| `RetrieveTotalRecordCountRequest` | `RetrieveTotalRecordCount` | ✅ Implemented | Stub | — |
| `RetrieveUnpublishedMultipleRequest` | `RetrieveUnpublishedMultiple` | ✅ Implemented | ? | — |
| `RetrieveUnpublishedRequest` | `RetrieveUnpublished` | ✅ Implemented | ? | — |
| `RetrieveUserLicenseInfoRequest` | `RetrieveUserLicenseInfo` | ✅ Implemented | Stub | — |
| `RetrieveUserPrivilegeByPrivilegeIdRequest` | `RetrieveUserPrivilegeByPrivilegeId` | ✅ Implemented | Stub | — |
| `RetrieveUserPrivilegeByPrivilegeNameRequest` | `RetrieveUserPrivilegeByPrivilegeName` | ✅ Implemented | Stub | — |
| `RetrieveUserPrivilegesRequest` | `RetrieveUserPrivileges` | ✅ Implemented | Partial | — |
| `RetrieveUserQueuesRequest` | `RetrieveUserQueues` | ✅ Implemented | Partial | — |
| `RetrieveUserSetOfPrivilegesByIdsRequest` | `RetrieveUserSetOfPrivilegesByIds` | ❌ Not implemented | — | — |
| `RetrieveUserSetOfPrivilegesByNamesRequest` | `RetrieveUserSetOfPrivilegesByNames` | ❌ Not implemented | — | — |
| `RetrieveUserSettingsSystemUserRequest` | `RetrieveUserSettingsSystemUser` | ✅ Implemented | Stub | — |
| `RetrieveUsersPrivilegesThroughTeamsRequest` | `RetrieveUsersPrivilegesThroughTeams` | ✅ Implemented | Stub | — |
| `RetrieveVersionRequest` | `RetrieveVersion` | ✅ Implemented | Stub | — |
| `RevertProductRequest` | `RevertProduct` | ✅ Implemented | Stub | — |
| `ReviseQuoteRequest` | `ReviseQuote` | ✅ Implemented | Full | — |
| `RevokeAccessRequest` | `RevokeAccess` | ✅ Implemented | Full | — |
| `RevokeSharedLinkRequest` | `RevokeSharedLink` | ❌ Not implemented | — | — |
| `RollupRequest` | `Rollup` | ❌ Not implemented | — | — |
| `RouteToRequest` | `RouteTo` | ✅ Implemented | Partial | — |
| `SearchByBodyKbArticleRequest` | `SearchByBodyKbArticle` | ❌ Not implemented | — | — |
| `SearchByKeywordsKbArticleRequest` | `SearchByKeywordsKbArticle` | ❌ Not implemented | — | — |
| `SearchByTitleKbArticleRequest` | `SearchByTitleKbArticle` | ❌ Not implemented | — | — |
| `SearchRequest` | `Search` | ❌ Not implemented | — | — |
| `SendBulkMailRequest` | `SendBulkMail` | ❌ Not implemented | — | — |
| `SendEmailFromTemplateRequest` | `SendEmailFromTemplate` | ✅ Implemented | Full | — |
| `SendEmailRequest` | `SendEmail` | ✅ Implemented | Full | — |
| `SendFaxRequest` | `SendFax` | ✅ Implemented | Full | — |
| `SendTemplateRequest` | `SendTemplate` | ✅ Implemented | Full | — |
| `SetAutoNumberSeedRequest` | `SetAutoNumberSeed` | ✅ Implemented | Partial | — |
| `SetBusinessEquipmentRequest` | `SetBusinessEquipment` | ❌ Not implemented | — | — |
| `SetBusinessSystemUserRequest` | `SetBusinessSystemUser` | ✅ Implemented | Stub | — |
| `SetFeatureStatusRequest` | `SetFeatureStatus` | ✅ Implemented | Stub | — |
| `SetLocLabelsRequest` | `SetLocLabels` | ❌ Not implemented | — | — |
| `SetParentBusinessUnitRequest` | `SetParentBusinessUnit` | ✅ Implemented | Stub | — |
| `SetParentSystemUserRequest` | `SetParentSystemUser` | ✅ Implemented | Stub | — |
| `SetParentTeamRequest` | `SetParentTeam` | ✅ Implemented | Stub | — |
| `SetPreferredSolutionRequest` | `SetPreferredSolution` | ❌ Not implemented | — | — |
| `SetProcessRequest` | `SetProcess` | ✅ Implemented | Stub | — |
| `SetRelatedRequest` | `SetRelated` | ❌ Not implemented | — | — |
| `SetReportRelatedRequest` | `SetReportRelated` | ❌ Not implemented | — | — |
| `SetStateRequest` | `SetState` | ✅ Implemented | Full | — |
| `StageAndUpgradeAsyncRequest` | `StageAndUpgradeAsync` | ❌ Not implemented | — | — |
| `StageAndUpgradeRequest` | `StageAndUpgrade` | ❌ Not implemented | — | — |
| `StageSolutionRequest` | `StageSolution` | ❌ Not implemented | — | — |
| `SyncBulkOperationRequest` | `SyncBulkOperation` | ❌ Not implemented | — | — |
| `TransformImportRequest` | `TransformImport` | ❌ Not implemented | — | — |
| `TriggerServiceEndpointCheckRequest` | `TriggerServiceEndpointCheck` | ❌ Not implemented | — | — |
| `UninstallSampleDataRequest` | `UninstallSampleData` | ❌ Not implemented | — | — |
| `UninstallSolutionAsyncRequest` | `UninstallSolutionAsync` | ❌ Not implemented | — | — |
| `UnlockInvoicePricingRequest` | `UnlockInvoicePricing` | ✅ Implemented | Full | — |
| `UnlockSalesOrderPricingRequest` | `UnlockSalesOrderPricing` | ✅ Implemented | Full | — |
| `UnpublishDuplicateRuleRequest` | `UnpublishDuplicateRule` | ✅ Implemented | Stub | — |
| `UpdateFeatureConfigRequest` | `UpdateFeatureConfig` | ✅ Implemented | Stub | — |
| `UpdateProductPropertiesRequest` | `UpdateProductProperties` | ✅ Implemented | Stub | — |
| `UpdateRibbonClientMetadataRequest` | `UpdateRibbonClientMetadata` | ❌ Not implemented | — | — |
| `UpdateSolutionComponentRequest` | `UpdateSolutionComponent` | ❌ Not implemented | — | — |
| `UpdateUserSettingsSystemUserRequest` | `UpdateUserSettingsSystemUser` | ✅ Implemented | Stub | — |
| `UploadBlockRequest` | `UploadBlock` | ✅ Implemented | Full | — |
| `UtcTimeFromLocalTimeRequest` | `UtcTimeFromLocalTime` | ✅ Implemented | Stub | — |
| `ValidateAppRequest` | `ValidateApp` | ❌ Not implemented | — | — |
| `ValidateFetchXmlExpressionRequest` | `ValidateFetchXmlExpression` | ✅ Implemented | Stub | — |
| `ValidateRecurrenceRuleRequest` | `ValidateRecurrenceRule` | ❌ Not implemented | — | — |
| `ValidateRequest` | `Validate` | ❌ Not implemented | — | — |
| `ValidateSavedQueryRequest` | `ValidateSavedQuery` | ✅ Implemented | Stub | — |
| `ValidateUnpublishedRequest` | `ValidateUnpublished` | ❌ Not implemented | — | — |
| `WhoAmIRequest` | `WhoAmI` | ✅ Implemented | Full | — |
| `WinOpportunityRequest` | `WinOpportunity` | ✅ Implemented | Full | — |
| `WinQuoteRequest` | `WinQuote` | ✅ Implemented | Full | — |

## Microsoft.Xrm.Sdk.Messages

| Request Type | Request Name | Status | Fidelity | Description |
|---|---|---|---|---|
| `AssociateRequest` | `Associate` | ✅ Implemented | Full | — |
| `CanBeReferencedRequest` | `CanBeReferenced` | ✅ Implemented | Partial | — |
| `CanBeReferencingRequest` | `CanBeReferencing` | ✅ Implemented | Partial | — |
| `CanManyToManyRequest` | `CanManyToMany` | ✅ Implemented | Partial | — |
| `ConvertDateAndTimeBehaviorRequest` | `ConvertDateAndTimeBehavior` | ✅ Implemented | Stub | — |
| `CreateAsyncJobToRevokeInheritedAccessRequest` | `CreateAsyncJobToRevokeInheritedAccess` | ✅ Implemented | Stub | — |
| `CreateAttributeRequest` | `CreateAttribute` | ✅ Implemented | Partial | — |
| `CreateCustomerRelationshipsRequest` | `CreateCustomerRelationships` | ✅ Implemented | Partial | — |
| `CreateEntityKeyRequest` | `CreateEntityKey` | ✅ Implemented | Partial | — |
| `CreateEntityRequest` | `CreateEntity` | ✅ Implemented | Partial | — |
| `CreateManyToManyRequest` | `CreateManyToMany` | ✅ Implemented | Partial | — |
| `CreateMultipleRequest` | `CreateMultiple` | ✅ Implemented | Full | Organization request for CreateMultiple SDK message. |
| `CreateOneToManyRequest` | `CreateOneToMany` | ✅ Implemented | Partial | — |
| `CreateOptionSetRequest` | `CreateOptionSet` | ✅ Implemented | Partial | — |
| `CreateRequest` | `Create` | ✅ Implemented | Full | — |
| `DeleteAttributeRequest` | `DeleteAttribute` | ✅ Implemented | Partial | — |
| `DeleteEntityKeyRequest` | `DeleteEntityKey` | ✅ Implemented | Partial | — |
| `DeleteEntityRequest` | `DeleteEntity` | ✅ Implemented | Partial | — |
| `DeleteMultipleRequest` | `DeleteMultiple` | ✅ Implemented | Full | Organization request for DeleteMultiple SDK message. |
| `DeleteOptionSetRequest` | `DeleteOptionSet` | ✅ Implemented | Partial | — |
| `DeleteOptionValueRequest` | `DeleteOptionValue` | ✅ Implemented | Partial | — |
| `DeleteRelationshipRequest` | `DeleteRelationship` | ✅ Implemented | Partial | — |
| `DeleteRequest` | `Delete` | ✅ Implemented | Full | — |
| `DisassociateRequest` | `Disassociate` | ✅ Implemented | Full | — |
| `ExecuteAsyncRequest` | `ExecuteAsync` | ✅ Implemented | Full | — |
| `ExecuteMultipleRequest` | `ExecuteMultiple` | ✅ Implemented | Full | — |
| `ExecuteTransactionRequest` | `ExecuteTransaction` | ✅ Implemented | Full | — |
| `GetValidManyToManyRequest` | `GetValidManyToMany` | ✅ Implemented | Partial | — |
| `GetValidReferencedEntitiesRequest` | `GetValidReferencedEntities` | ✅ Implemented | Partial | — |
| `GetValidReferencingEntitiesRequest` | `GetValidReferencingEntities` | ✅ Implemented | Partial | — |
| `InsertOptionValueRequest` | `InsertOptionValue` | ✅ Implemented | Partial | — |
| `InsertStatusValueRequest` | `InsertStatusValue` | ✅ Implemented | Stub | — |
| `IsDataEncryptionActiveRequest` | `IsDataEncryptionActive` | ✅ Implemented | Stub | — |
| `OrderOptionRequest` | `OrderOption` | ✅ Implemented | Partial | — |
| `ReactivateEntityKeyRequest` | `ReactivateEntityKey` | ✅ Implemented | Stub | — |
| `RetrieveAllEntitiesRequest` | `RetrieveAllEntities` | ✅ Implemented | Partial | — |
| `RetrieveAllManagedPropertiesRequest` | `RetrieveAllManagedProperties` | ✅ Implemented | Stub | — |
| `RetrieveAllOptionSetsRequest` | `RetrieveAllOptionSets` | ✅ Implemented | Partial | — |
| `RetrieveAttributeRequest` | `RetrieveAttribute` | ✅ Implemented | Partial | — |
| `RetrieveDataEncryptionKeyRequest` | `RetrieveDataEncryptionKey` | ✅ Implemented | Stub | — |
| `RetrieveEntityChangesRequest` | `RetrieveEntityChanges` | ✅ Implemented | Stub | — |
| `RetrieveEntityKeyRequest` | `RetrieveEntityKey` | ✅ Implemented | Partial | — |
| `RetrieveEntityRequest` | `RetrieveEntity` | ✅ Implemented | Partial | — |
| `RetrieveManagedPropertyRequest` | `RetrieveManagedProperty` | ✅ Implemented | Stub | — |
| `RetrieveMetadataChangesRequest` | `RetrieveMetadataChanges` | ✅ Implemented | Partial | — |
| `RetrieveMultipleRequest` | `RetrieveMultiple` | ✅ Implemented | Full | — |
| `RetrieveOptionSetRequest` | `RetrieveOptionSet` | ✅ Implemented | Partial | — |
| `RetrieveRelationshipRequest` | `RetrieveRelationship` | ✅ Implemented | Partial | — |
| `RetrieveRequest` | `Retrieve` | ✅ Implemented | Full | — |
| `RetrieveTimestampRequest` | `RetrieveTimestamp` | ✅ Implemented | Partial | — |
| `SetDataEncryptionKeyRequest` | `SetDataEncryptionKey` | ✅ Implemented | Stub | — |
| `UpdateAttributeRequest` | `UpdateAttribute` | ✅ Implemented | Partial | — |
| `UpdateEntityRequest` | `UpdateEntity` | ✅ Implemented | Partial | — |
| `UpdateMultipleRequest` | `UpdateMultiple` | ✅ Implemented | Full | Organization request for UpdateMultiple SDK message. |
| `UpdateOptionSetRequest` | `UpdateOptionSet` | ✅ Implemented | Partial | — |
| `UpdateOptionValueRequest` | `UpdateOptionValue` | ✅ Implemented | Partial | — |
| `UpdateRelationshipRequest` | `UpdateRelationship` | ✅ Implemented | Partial | — |
| `UpdateRequest` | `Update` | ✅ Implemented | Full | — |
| `UpdateStateValueRequest` | `UpdateStateValue` | ✅ Implemented | Stub | — |
| `UpsertMultipleRequest` | `UpsertMultiple` | ✅ Implemented | Full | Organization request for UpsertMultiple SDK message. |
| `UpsertRequest` | `Upsert` | ✅ Implemented | Full | — |

## Implemented handlers not discovered in reflected SDK types

| Request Name | Fidelity |
|---|---|
| `ExportPdfDocument` | Stub |

---

## Fidelity legend

| Level | Meaning |
|---|---|
| **Full** | Behavior closely matches Dataverse in common scenarios |
| **Partial** | Core behavior is supported with some known limitations |
| **Stub** | Returns structurally valid but minimal placeholder behavior |
| **—** | No built-in handler currently available |
