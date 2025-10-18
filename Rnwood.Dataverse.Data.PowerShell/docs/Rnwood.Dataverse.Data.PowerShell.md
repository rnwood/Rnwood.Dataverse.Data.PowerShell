---
Module Name: Rnwood.Dataverse.Data.PowerShell
Module Guid: {{ Update Module Guid }}
Download Help Link: {{ Update Download Link }}
Help Version: {{ Update Help Version }}
Locale: {{ Update Locale }}
---

# Rnwood.Dataverse.Data.PowerShell Module
## Description
{{ Fill in the Description }}

## Rnwood.Dataverse.Data.PowerShell Cmdlets
### [Get-DataverseConnection](Get-DataverseConnection.md)
Gets a connection to a Dataverse environment either interactively or silently and returns it.

All commands that need a connection to Dataverse expect you to provide the connection in `-connection` parameter.
So you can store the output of this command in a variable and pass it to each command that needs it.
See the examples for this pattern below.

### [Get-DataverseRecord](Get-DataverseRecord.md)
Retrieves records from Dataverse tables using a variety of strategies to specify what should be retrieved.

### [Get-DataverseRecordsFolder](Get-DataverseRecordsFolder.md)
Reads a folder of JSON files written out by `Set-DataverseRecordFolder` and converts back into a stream of PS objects.
Together these commands can be used to extract and import data to and from files, for instance for inclusion in source control, or build/deployment assets.

### [Get-DataverseWhoAmI](Get-DataverseWhoAmI.md)
Retrieves details about the current Dataverse user and organization specified by the connection provided.

### [Invoke-DataverseAddAppComponents](Invoke-DataverseAddAppComponents.md)
Contains the data that is needed to add app components to a business app.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddAppComponentsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddAppComponentsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAddChannelAccessProfilePrivileges](Invoke-DataverseAddChannelAccessProfilePrivileges.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddChannelAccessProfilePrivilegesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddChannelAccessProfilePrivilegesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAddItemCampaign](Invoke-DataverseAddItemCampaign.md)
Contains the data that is needed to add an item to a campaign.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddItemCampaignRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddItemCampaignRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAddItemCampaignActivity](Invoke-DataverseAddItemCampaignActivity.md)
Contains the data that is needed to add an item to a campaign activity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddItemCampaignActivityRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddItemCampaignActivityRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAddListMembersList](Invoke-DataverseAddListMembersList.md)
Contains the data that is needed to add members to the list.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddListMembersListRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddListMembersListRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAddMemberList](Invoke-DataverseAddMemberList.md)
Contains the data that is needed to add a member to a list (marketing list).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddMemberListRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddMemberListRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAddMembersTeam](Invoke-DataverseAddMembersTeam.md)
Contains the data that is needed to add members to a team.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddMembersTeamRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddMembersTeamRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAddPrincipalToQueue](Invoke-DataverseAddPrincipalToQueue.md)
Contains the data to add the specified principal to the list of queue members. If the principal is a team, add each team member to the queue.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddPrincipalToQueueRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddPrincipalToQueueRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAddPrivilegesRole](Invoke-DataverseAddPrivilegesRole.md)
Contains the data that is needed to add a set of existing privileges to an existing role.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddPrivilegesRoleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddPrivilegesRoleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAddRecurrence](Invoke-DataverseAddRecurrence.md)
Contains the data that is needed to add recurrence information to an existing appointment.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddRecurrenceRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddRecurrenceRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAddSolutionComponent](Invoke-DataverseAddSolutionComponent.md)
Contains the data that is needed to add a solution component to an unmanaged solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddSolutionComponentRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddSolutionComponentRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAddToQueue](Invoke-DataverseAddToQueue.md)
Contains the data that is needed to move an entity record from a source queue to a destination queue.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddToQueueRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddToQueueRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAddUserToRecordTeam](Invoke-DataverseAddUserToRecordTeam.md)
Contains the data that is needed to add a user to the auto created access team for the specified record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddUserToRecordTeamRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AddUserToRecordTeamRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseApplyRecordCreationAndUpdateRule](Invoke-DataverseApplyRecordCreationAndUpdateRule.md)
Contains data to apply record creation and update rules to activities in Dynamics 365 Customer Service created as a result of the integration with external applications.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ApplyRecordCreationAndUpdateRuleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ApplyRecordCreationAndUpdateRuleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseApplyRoutingRule](Invoke-DataverseApplyRoutingRule.md)
Contains the data that is needed to apply the active routing rule to an incident.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ApplyRoutingRuleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ApplyRoutingRuleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAssign](Invoke-DataverseAssign.md)
Contains the data that is needed to assign the specified record to a new owner (user or team) by changing the OwnerId attribute of the record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AssignRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AssignRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseAssociate](Invoke-DataverseAssociate.md)
Contains the data that is needed to related one or more records.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.AssociateRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.AssociateRequest)

### [Invoke-DataverseAutoMapEntity](Invoke-DataverseAutoMapEntity.md)
Contains the data that is needed to generate a new set of attribute mappings based on the metadata.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AutoMapEntityRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.AutoMapEntityRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseBackgroundSendEmail](Invoke-DataverseBackgroundSendEmail.md)
Contains the data that is needed to send email messages asynchronously.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.BackgroundSendEmailRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.BackgroundSendEmailRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseBook](Invoke-DataverseBook.md)
Contains the data that is needed to schedule or "book" an appointment, recurring appointment, or service appointment (service activity).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.BookRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.BookRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseBulkDelete](Invoke-DataverseBulkDelete.md)
Contains the data that's needed to submit a bulk delete job that deletes selected records in bulk. This job runs asynchronously in the background without blocking other activities.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.BulkDeleteRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.BulkDeleteRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseBulkDetectDuplicates](Invoke-DataverseBulkDetectDuplicates.md)
Contains the data that is needed to submit an asynchronous system job that detects and logs multiple duplicate records.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.BulkDetectDuplicatesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.BulkDetectDuplicatesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCalculateActualValueOpportunity](Invoke-DataverseCalculateActualValueOpportunity.md)
Contains the data that is needed to calculate the value of an opportunity that is in the "Won" state.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CalculateActualValueOpportunityRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CalculateActualValueOpportunityRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCalculatePrice](Invoke-DataverseCalculatePrice.md)
Contains the data that is needed to calculate price in an opportunity, quote, order, and invoice. This is used internally for custom pricing calculation when the default system pricing is overridden.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CalculatePriceRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CalculatePriceRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCalculateRollupField](Invoke-DataverseCalculateRollupField.md)
Contains the data that is needed to calculate the value of a rollup column.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CalculateRollupFieldRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CalculateRollupFieldRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCalculateTotalTimeIncident](Invoke-DataverseCalculateTotalTimeIncident.md)
Contains the data that is needed to calculate the total time, in minutes, that you used while you worked on an incident (case).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CalculateTotalTimeIncidentRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CalculateTotalTimeIncidentRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCanBeReferenced](Invoke-DataverseCanBeReferenced.md)
Contains the data that is needed to check whether the specified table can be the primary table (one) in a one-to-many relationship.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CanBeReferencedRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CanBeReferencedRequest)

### [Invoke-DataverseCanBeReferencing](Invoke-DataverseCanBeReferencing.md)
Contains the data that is needed to check whether a table can be the referencing table in a one-to-many relationship.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CanBeReferencingRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CanBeReferencingRequest)

### [Invoke-DataverseCancelContract](Invoke-DataverseCancelContract.md)
Contains the data that is needed to cancel a contract.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CancelContractRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CancelContractRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCancelSalesOrder](Invoke-DataverseCancelSalesOrder.md)
Contains the data that is needed to cancel a sales order (order).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CancelSalesOrderRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CancelSalesOrderRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCanManyToMany](Invoke-DataverseCanManyToMany.md)
Contains the data that is needed to check whether a table can participate in a many-to-many relationship.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CanManyToManyRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CanManyToManyRequest)

### [Invoke-DataverseCheckIncomingEmail](Invoke-DataverseCheckIncomingEmail.md)
Contains the data that is needed to check whether the incoming email message is relevant to Dataverse.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CheckIncomingEmailRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CheckIncomingEmailRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCheckPromoteEmail](Invoke-DataverseCheckPromoteEmail.md)
Contains the data that is needed to check whether the incoming email message should be promoted to Dataverse.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CheckPromoteEmailRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CheckPromoteEmailRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCloneAsPatch](Invoke-DataverseCloneAsPatch.md)
Contains the data that is needed to create a solution patch from a managed or unmanaged solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CloneAsPatchRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CloneAsPatchRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCloneAsSolution](Invoke-DataverseCloneAsSolution.md)
Contains the data that is needed to create a new copy of an unmanaged solution that contains the original solution plus all of its patches.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CloneAsSolutionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CloneAsSolutionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCloneContract](Invoke-DataverseCloneContract.md)
Contains the data that is needed to copy an existing contract and its line items.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CloneContractRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CloneContractRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCloneMobileOfflineProfile](Invoke-DataverseCloneMobileOfflineProfile.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CloneMobileOfflineProfileRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CloneMobileOfflineProfileRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCloneProduct](Invoke-DataverseCloneProduct.md)
Contains the data that is needed to copy an existing product family, product, or bundle under the same parent record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CloneProductRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CloneProductRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCloseIncident](Invoke-DataverseCloseIncident.md)
Contains the data that is needed to close an incident (case).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CloseIncidentRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CloseIncidentRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCloseQuote](Invoke-DataverseCloseQuote.md)
Contains the data that is needed to close a quote.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CloseQuoteRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CloseQuoteRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCommitAnnotationBlocksUpload](Invoke-DataverseCommitAnnotationBlocksUpload.md)
Contains the data needed to commit the uploaded data blocks to the annotation store.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CommitAnnotationBlocksUploadRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CommitAnnotationBlocksUploadRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCommitAttachmentBlocksUpload](Invoke-DataverseCommitAttachmentBlocksUpload.md)
Contains the data needed to commit the uploaded data blocks to the attachment store.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CommitAttachmentBlocksUploadRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CommitAttachmentBlocksUploadRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCommitFileBlocksUpload](Invoke-DataverseCommitFileBlocksUpload.md)
Contains the data needed to commit the uploaded data blocks to the file store.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CommitFileBlocksUploadRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CommitFileBlocksUploadRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCompoundUpdateDuplicateDetectionRule](Invoke-DataverseCompoundUpdateDuplicateDetectionRule.md)
Contains the data that is needed to update a duplicate rule (duplicate detection rule) and its related duplicate rule conditions.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CompoundUpdateDuplicateDetectionRuleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CompoundUpdateDuplicateDetectionRuleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseConvertDateAndTimeBehavior](Invoke-DataverseConvertDateAndTimeBehavior.md)
Contains the data to convert existing UTC date and time values in the database to DateOnly values.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.ConvertDateAndTimeBehaviorRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.ConvertDateAndTimeBehaviorRequest)

### [Invoke-DataverseConvertKitToProduct](Invoke-DataverseConvertKitToProduct.md)
Deprecated. Contains the data that is needed to convert a kit to a product.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ConvertKitToProductRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ConvertKitToProductRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseConvertOwnerTeamToAccessTeam](Invoke-DataverseConvertOwnerTeamToAccessTeam.md)
Contains the data that is needed to convert a team of type owner to a team of type access.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ConvertOwnerTeamToAccessTeamRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ConvertOwnerTeamToAccessTeamRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseConvertProductToKit](Invoke-DataverseConvertProductToKit.md)
Deprecated. Contains the data that is needed to convert a product to a kit.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ConvertProductToKitRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ConvertProductToKitRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseConvertQuoteToSalesOrder](Invoke-DataverseConvertQuoteToSalesOrder.md)
Contains the data that is needed to convert a quote to a sales order.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ConvertQuoteToSalesOrderRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ConvertQuoteToSalesOrderRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseConvertSalesOrderToInvoice](Invoke-DataverseConvertSalesOrderToInvoice.md)
Contains the data that is needed to convert a sales order to an invoice.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ConvertSalesOrderToInvoiceRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ConvertSalesOrderToInvoiceRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCopyCampaign](Invoke-DataverseCopyCampaign.md)
Contains the data that is needed to copy a campaign.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CopyCampaignRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CopyCampaignRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCopyCampaignResponse](Invoke-DataverseCopyCampaignResponse.md)
Contains the data that is needed to create a copy of the campaign response.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CopyCampaignResponseRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CopyCampaignResponseRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCopyDynamicListToStatic](Invoke-DataverseCopyDynamicListToStatic.md)
Contains the data that is needed to create a static list from the specified dynamic list and add the members that satisfy the dynamic list query criteria to the static list.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CopyDynamicListToStaticRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CopyDynamicListToStaticRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCopyMembersList](Invoke-DataverseCopyMembersList.md)
Contains the data that is needed to copy the members from the source list to the target list without creating duplicates.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CopyMembersListRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CopyMembersListRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCopySystemForm](Invoke-DataverseCopySystemForm.md)
Contains the data that is needed to create a new form for a table that is based on an existing form.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CopySystemFormRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CopySystemFormRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCreate](Invoke-DataverseCreate.md)
Contains the data that is needed to create a record.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CreateRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CreateRequest)

### [Invoke-DataverseCreateActivitiesList](Invoke-DataverseCreateActivitiesList.md)
Contains the data that is needed to create a quick campaign to distribute an activity to members of a list (marketing list).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CreateActivitiesListRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CreateActivitiesListRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCreateAsyncJobToRevokeInheritedAccess](Invoke-DataverseCreateAsyncJobToRevokeInheritedAccess.md)
Contains the data that is needed to create and execute an asynchronous cleanup job to revoke inherited access granted through cascading inheritance.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CreateAsyncJobToRevokeInheritedAccessRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CreateAsyncJobToRevokeInheritedAccessRequest)

### [Invoke-DataverseCreateAttribute](Invoke-DataverseCreateAttribute.md)
Contains the data that is needed to create a new column, and optionally, to add it to a specified unmanaged solution.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CreateAttributeRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CreateAttributeRequest)

### [Invoke-DataverseCreateCustomerRelationships](Invoke-DataverseCreateCustomerRelationships.md)
Contains the data that is needed to create a new customer lookup column, and optionally, to add it to a specified unmanaged solution.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CreateCustomerRelationshipsRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CreateCustomerRelationshipsRequest)

### [Invoke-DataverseCreateEntity](Invoke-DataverseCreateEntity.md)
Contains the data that is needed to create a table, and optionally, to add it to a specified unmanaged solution.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CreateEntityRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CreateEntityRequest)

### [Invoke-DataverseCreateEntityKey](Invoke-DataverseCreateEntityKey.md)
Contains data that is needed to create an alternate key.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CreateEntityKeyRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CreateEntityKeyRequest)

### [Invoke-DataverseCreateException](Invoke-DataverseCreateException.md)
Contains the data that is needed to create an exception for the recurring appointment instance.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CreateExceptionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CreateExceptionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCreateInstance](Invoke-DataverseCreateInstance.md)
Contains the data that is needed to create future unexpanded instances for the recurring appointment master.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CreateInstanceRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CreateInstanceRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCreateKnowledgeArticleTranslation](Invoke-DataverseCreateKnowledgeArticleTranslation.md)
Contains the data that is required to create translation of a knowledge article record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CreateKnowledgeArticleTranslationRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CreateKnowledgeArticleTranslationRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCreateKnowledgeArticleVersion](Invoke-DataverseCreateKnowledgeArticleVersion.md)
Contains the data that is required to create a major or minor version of a knowledge article record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CreateKnowledgeArticleVersionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CreateKnowledgeArticleVersionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCreateManyToMany](Invoke-DataverseCreateManyToMany.md)
Contains the data that is needed to create a new Many-to-Many (N:N) table relationship.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CreateManyToManyRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CreateManyToManyRequest)

### [Invoke-DataverseCreateMultiple](Invoke-DataverseCreateMultiple.md)
Contains the data to create multiple records of the same type with a single web request.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CreateMultipleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CreateMultipleRequest)

### [Invoke-DataverseCreateOneToMany](Invoke-DataverseCreateOneToMany.md)
Contains the data that is needed to create a new One-to-Many (1:N) table relationship.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CreateOneToManyRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CreateOneToManyRequest)

### [Invoke-DataverseCreateOptionSet](Invoke-DataverseCreateOptionSet.md)
Contains the data that is needed to create a new global choice.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.CreateOptionSetRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.CreateOptionSetRequest)

### [Invoke-DataverseCreatePolymorphicLookupAttribute](Invoke-DataverseCreatePolymorphicLookupAttribute.md)
Contains the data to create a multi-table lookup column.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CreatePolymorphicLookupAttributeRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CreatePolymorphicLookupAttributeRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseCreateWorkflowFromTemplate](Invoke-DataverseCreateWorkflowFromTemplate.md)
Contains the data that is needed to create a workflow (process) from a workflow template.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CreateWorkflowFromTemplateRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.CreateWorkflowFromTemplateRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDelete](Invoke-DataverseDelete.md)
Contains the data that's needed to submit a bulk delete job that deletes selected records in bulk. This job runs asynchronously in the background without blocking other activities.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.DeleteRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.DeleteRequest)

### [Invoke-DataverseDeleteAndPromote](Invoke-DataverseDeleteAndPromote.md)
Contains the data needed to replace a managed solution plus all of its patches.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeleteAndPromoteRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DeleteAndPromoteRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDeleteAndPromoteAsync](Invoke-DataverseDeleteAndPromoteAsync.md)
Asynchronously replaces managed solution (A) plus all of its patches with managed solution (B) that is the clone of (A) and all of its patches.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeleteAndPromoteAsyncRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DeleteAndPromoteAsyncRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDeleteAttribute](Invoke-DataverseDeleteAttribute.md)
Contains the data that is needed to delete a column.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.DeleteAttributeRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.DeleteAttributeRequest)

### [Invoke-DataverseDeleteAuditData](Invoke-DataverseDeleteAuditData.md)
Contains the data that is needed for customers using customer-managed encryption keys to delete all audit data records up until a specified end date.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeleteAuditDataRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DeleteAuditDataRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDeleteEntity](Invoke-DataverseDeleteEntity.md)
Contains the data that is needed to delete a table.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.DeleteEntityRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.DeleteEntityRequest)

### [Invoke-DataverseDeleteEntityKey](Invoke-DataverseDeleteEntityKey.md)
Contains the data that is needed to delete the specified key for an entity.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.DeleteEntityKeyRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.DeleteEntityKeyRequest)

### [Invoke-DataverseDeleteFile](Invoke-DataverseDeleteFile.md)
Contains the data needed to delete a stored binary file.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeleteFileRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DeleteFileRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDeleteMultiple](Invoke-DataverseDeleteMultiple.md)
Executes a DeleteMultipleRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.DeleteMultipleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.DeleteMultipleRequest)

### [Invoke-DataverseDeleteOpenInstances](Invoke-DataverseDeleteOpenInstances.md)
Contains the data that is needed to delete instances of a recurring appointment master that have an 'Open' state.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeleteOpenInstancesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DeleteOpenInstancesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDeleteOptionSet](Invoke-DataverseDeleteOptionSet.md)
Contains the data that is needed to delete a global choice.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.DeleteOptionSetRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.DeleteOptionSetRequest)

### [Invoke-DataverseDeleteOptionValue](Invoke-DataverseDeleteOptionValue.md)
Contains the data that is needed to delete an option value in a global or local choice.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.DeleteOptionValueRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.DeleteOptionValueRequest)

### [Invoke-DataverseDeleteRecordChangeHistory](Invoke-DataverseDeleteRecordChangeHistory.md)
Contains the data that is needed to delete all audit change history records for a particular record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeleteRecordChangeHistoryRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DeleteRecordChangeHistoryRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDeleteRecordChangeHistory1](Invoke-DataverseDeleteRecordChangeHistory1.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeleteRecordChangeHistory1Request](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DeleteRecordChangeHistory1Request?view=dataverse-sdk-latest)

### [Invoke-DataverseDeleteRelationship](Invoke-DataverseDeleteRelationship.md)
Contains the data that is needed to delete a table relationship.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.DeleteRelationshipRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.DeleteRelationshipRequest)

### [Invoke-DataverseDeliverImmediatePromoteEmail](Invoke-DataverseDeliverImmediatePromoteEmail.md)
Contains the data that is needed to deliver an email from an email client.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeliverImmediatePromoteEmailRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DeliverImmediatePromoteEmailRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDeliverIncomingEmail](Invoke-DataverseDeliverIncomingEmail.md)
Contains the data that is needed to create an email activity record from an incoming email message (Track in CRM).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeliverIncomingEmailRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DeliverIncomingEmailRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDeliverPromoteEmail](Invoke-DataverseDeliverPromoteEmail.md)
Contains the data that is needed to create an email activity record from the specified email message (Track in CRM).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeliverPromoteEmailRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DeliverPromoteEmailRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDeprovisionLanguage](Invoke-DataverseDeprovisionLanguage.md)
Contains the data that is needed to deprovision a language.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeprovisionLanguageRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DeprovisionLanguageRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDisassociate](Invoke-DataverseDisassociate.md)
Contains the data that is needed to remove associations between records.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.DisassociateRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.DisassociateRequest)

### [Invoke-DataverseDistributeCampaignActivity](Invoke-DataverseDistributeCampaignActivity.md)
Contains the data that is needed to create a bulk operation that distributes a campaign activity. The appropriate activities, such as a phone call or fax, are created for the members of the list that is associated with the specified campaign activity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DistributeCampaignActivityRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DistributeCampaignActivityRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDownloadBlock](Invoke-DataverseDownloadBlock.md)
Contains the data needed to download a data block.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DownloadBlockRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DownloadBlockRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDownloadReportDefinition](Invoke-DataverseDownloadReportDefinition.md)
Contains the data that is needed to download a report definition.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DownloadReportDefinitionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DownloadReportDefinitionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseDownloadSolutionExportData](Invoke-DataverseDownloadSolutionExportData.md)
Contains the data needed to download a solution file that was exported by an asynchronous job.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DownloadSolutionExportDataRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.DownloadSolutionExportDataRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseExecuteAsync](Invoke-DataverseExecuteAsync.md)
Contains the data that is needed to execute a message asynchronously.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.ExecuteAsyncRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.ExecuteAsyncRequest)

### [Invoke-DataverseExecuteByIdSavedQuery](Invoke-DataverseExecuteByIdSavedQuery.md)
Contains the data that is needed to execute a saved query (view) that has the specified ID.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExecuteByIdSavedQueryRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ExecuteByIdSavedQueryRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseExecuteByIdUserQuery](Invoke-DataverseExecuteByIdUserQuery.md)
Contains the data that is needed to execute the user query (saved view) that has the specified ID.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExecuteByIdUserQueryRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ExecuteByIdUserQueryRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseExecuteMultiple](Invoke-DataverseExecuteMultiple.md)
Contains the data that is needed to execute one or more message requests as a single batch operation, and optionally return a collection of results.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest)

### [Invoke-DataverseExecuteTransaction](Invoke-DataverseExecuteTransaction.md)
Contains the data that is needed to execute one or more message requests in a single database transaction, and optionally return a collection of results.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.ExecuteTransactionRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.ExecuteTransactionRequest)

### [Invoke-DataverseExecuteWorkflow](Invoke-DataverseExecuteWorkflow.md)
Contains the data that's needed to execute a workflow.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExecuteWorkflowRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ExecuteWorkflowRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseExpandCalendar](Invoke-DataverseExpandCalendar.md)
Contains the data that is needed to convert the calendar rules to an array of available time blocks for the specified period.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExpandCalendarRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ExpandCalendarRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseExportFieldTranslation](Invoke-DataverseExportFieldTranslation.md)
Contains the data that is needed to export localizable fields values to a compressed file.For the Web API use ExportFieldTranslation Function.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExportFieldTranslationRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ExportFieldTranslationRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseExportMappingsImportMap](Invoke-DataverseExportMappingsImportMap.md)
Contains the data that is needed to export a data map as an XML formatted data.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExportMappingsImportMapRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ExportMappingsImportMapRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseExportSolution](Invoke-DataverseExportSolution.md)
Contains the data needed to export a solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExportSolutionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ExportSolutionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseExportSolutionAsync](Invoke-DataverseExportSolutionAsync.md)
Contains the data to export a solution using an asynchronous job.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExportSolutionAsyncRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ExportSolutionAsyncRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseExportTranslation](Invoke-DataverseExportTranslation.md)
Contains the data that is needed to export all translations for a specific solution to a compressed file.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ExportTranslationRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ExportTranslationRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseFetchXmlToQueryExpression](Invoke-DataverseFetchXmlToQueryExpression.md)
Contains the data that is needed to convert a query in FetchXML to a QueryExpression.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.FetchXmlToQueryExpressionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseFindParentResourceGroup](Invoke-DataverseFindParentResourceGroup.md)
Contains the data that is needed to find a parent resource group (scheduling group) for the specified resource groups (scheduling groups).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.FindParentResourceGroupRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.FindParentResourceGroupRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseFormatAddress](Invoke-DataverseFormatAddress.md)
Contains the data to compute an address based on country and format parameters.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.FormatAddressRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.FormatAddressRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseFulfillSalesOrder](Invoke-DataverseFulfillSalesOrder.md)
Contains the data that is needed to fulfill the sales order (order).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.FulfillSalesOrderRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.FulfillSalesOrderRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseFullTextSearchKnowledgeArticle](Invoke-DataverseFullTextSearchKnowledgeArticle.md)
Contains the data that is needed to perform a full-text search on knowledge articles in CRM using the specified search text.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.FullTextSearchKnowledgeArticleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.FullTextSearchKnowledgeArticleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGenerateInvoiceFromOpportunity](Invoke-DataverseGenerateInvoiceFromOpportunity.md)
Contains the data that is needed to generate an invoice from an opportunity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GenerateInvoiceFromOpportunityRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GenerateInvoiceFromOpportunityRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGenerateQuoteFromOpportunity](Invoke-DataverseGenerateQuoteFromOpportunity.md)
Contains the data that is needed to generate a quote from an opportunity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GenerateQuoteFromOpportunityRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GenerateQuoteFromOpportunityRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGenerateSalesOrderFromOpportunity](Invoke-DataverseGenerateSalesOrderFromOpportunity.md)
Contains the data that is needed to generate a sales order (order) from an opportunity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GenerateSalesOrderFromOpportunityRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GenerateSalesOrderFromOpportunityRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGenerateSharedLink](Invoke-DataverseGenerateSharedLink.md)
Creates a link to a table row that can be shared with other system users.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GenerateSharedLinkRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GenerateSharedLinkRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGenerateSocialProfile](Invoke-DataverseGenerateSocialProfile.md)
Contains the data to return an existing social profile record if one exists, otherwise generates a new one and returns it.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GenerateSocialProfileRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GenerateSocialProfileRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetAllTimeZonesWithDisplayName](Invoke-DataverseGetAllTimeZonesWithDisplayName.md)
Contains the data that is needed to retrieve all the time zone definitions for the specified locale and to return only the display name attribute.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetAllTimeZonesWithDisplayNameRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetAllTimeZonesWithDisplayNameRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetAutoNumberSeed](Invoke-DataverseGetAutoNumberSeed.md)
Executes a GetAutoNumberSeedRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetAutoNumberSeedRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetAutoNumberSeedRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetAutoNumberSeed1](Invoke-DataverseGetAutoNumberSeed1.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetAutoNumberSeed1Request](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetAutoNumberSeed1Request?view=dataverse-sdk-latest)

### [Invoke-DataverseGetDecryptionKey](Invoke-DataverseGetDecryptionKey.md)
For internal use only. See .

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetDecryptionKeyRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetDecryptionKeyRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetDefaultPriceLevel](Invoke-DataverseGetDefaultPriceLevel.md)
Contains the data that is needed to retrieve the default price level (price list) for the current user based on the user's territory relationship with the price level.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetDefaultPriceLevelRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetDefaultPriceLevelRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetDistinctValuesImportFile](Invoke-DataverseGetDistinctValuesImportFile.md)
Contains the data that is needed to retrieve distinct values from the parse table for a column in the source file that contains list values.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetDistinctValuesImportFileRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetDistinctValuesImportFileRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetFileSasUrl](Invoke-DataverseGetFileSasUrl.md)
Contains the data that is needed to retrieve a shared access signature URL to download a file or image from Dataverse.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetFileSasUrlRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetFileSasUrlRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetHeaderColumnsImportFile](Invoke-DataverseGetHeaderColumnsImportFile.md)
Contains the data that is needed to retrieve the source-file column headings; or retrieve the system-generated column headings if the source file does not contain column headings.For the Web API use GetHeaderColumnsImportFile Function.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetHeaderColumnsImportFileRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetHeaderColumnsImportFileRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetInvoiceProductsFromOpportunity](Invoke-DataverseGetInvoiceProductsFromOpportunity.md)
Contains the data that is needed to retrieve the products from an opportunity and copy them to the invoice.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetInvoiceProductsFromOpportunityRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetInvoiceProductsFromOpportunityRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetNextAutoNumberValue](Invoke-DataverseGetNextAutoNumberValue.md)
Executes a GetNextAutoNumberValueRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetNextAutoNumberValueRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetNextAutoNumberValueRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetNextAutoNumberValue1](Invoke-DataverseGetNextAutoNumberValue1.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetNextAutoNumberValue1Request](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetNextAutoNumberValue1Request?view=dataverse-sdk-latest)

### [Invoke-DataverseGetPreferredSolution](Invoke-DataverseGetPreferredSolution.md)
Executes a GetPreferredSolutionRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetPreferredSolutionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetPreferredSolutionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetQuantityDecimal](Invoke-DataverseGetQuantityDecimal.md)
Contains the data that is needed to get the quantity decimal value of a product for the specified entity in the target.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetQuantityDecimalRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetQuantityDecimalRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetQuoteProductsFromOpportunity](Invoke-DataverseGetQuoteProductsFromOpportunity.md)
Contains the data that is needed to retrieve the products from an opportunity and copy them to the quote.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetQuoteProductsFromOpportunityRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetQuoteProductsFromOpportunityRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetReportHistoryLimit](Invoke-DataverseGetReportHistoryLimit.md)
Contains the data that is needed to retrieve the history limit for a report.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetReportHistoryLimitRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetReportHistoryLimitRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetSalesOrderProductsFromOpportunity](Invoke-DataverseGetSalesOrderProductsFromOpportunity.md)
Contains the data that is needed to retrieve the products from an opportunity and copy them to the sales order (order).For the Web API use GetSalesOrderProductsFromOpportunity Action.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetSalesOrderProductsFromOpportunityRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetSalesOrderProductsFromOpportunityRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetTimeZoneCodeByLocalizedName](Invoke-DataverseGetTimeZoneCodeByLocalizedName.md)
Contains the data that is needed to retrieve the time zone code for the specified localized time zone name.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetTimeZoneCodeByLocalizedNameRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetTimeZoneCodeByLocalizedNameRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetTrackingTokenEmail](Invoke-DataverseGetTrackingTokenEmail.md)
Contains the data that is needed to return a tracking token that can then be passed as a parameter to the message.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GetTrackingTokenEmailRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GetTrackingTokenEmailRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGetValidManyToMany](Invoke-DataverseGetValidManyToMany.md)
Contains the data that is needed to retrieve a list of all the tables that can participate in a many-to-many table relationship.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.GetValidManyToManyRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.GetValidManyToManyRequest)

### [Invoke-DataverseGetValidReferencedEntities](Invoke-DataverseGetValidReferencedEntities.md)
Contains the data that is needed to retrieve a list of table logical names that are valid as the primary table (one) from the specified table in a one-to-many relationship.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.GetValidReferencedEntitiesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.GetValidReferencedEntitiesRequest)

### [Invoke-DataverseGetValidReferencingEntities](Invoke-DataverseGetValidReferencingEntities.md)
Contains the data that is needed to retrieve the set of entities that are valid as the referencing entity (many) to the specified entity in a one-to-many relationship.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.GetValidReferencingEntitiesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.GetValidReferencingEntitiesRequest)

### [Invoke-DataverseGrantAccess](Invoke-DataverseGrantAccess.md)
Contains the data that is needed to grant a security principal (user, team, or organization) access to the specified record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GrantAccessRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GrantAccessRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseGrantAccessUsingSharedLink](Invoke-DataverseGrantAccessUsingSharedLink.md)
Adds a system user to the shared link access team of the target table row.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GrantAccessUsingSharedLinkRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.GrantAccessUsingSharedLinkRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseImmediateBook](Invoke-DataverseImmediateBook.md)
Contains the data to book an appointment transactionally, obeying the constraints specified by the associated service and the supplied appointment request.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImmediateBookRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ImmediateBookRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseImportCardTypeSchema](Invoke-DataverseImportCardTypeSchema.md)
Contains the data to import and create a new cardtype required by the installed solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportCardTypeSchemaRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ImportCardTypeSchemaRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseImportFieldTranslation](Invoke-DataverseImportFieldTranslation.md)
Contains the data that is needed to import translations from a compressed file.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportFieldTranslationRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ImportFieldTranslationRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseImportMappingsImportMap](Invoke-DataverseImportMappingsImportMap.md)
Contains the data that is needed to import the XML representation of a data map and create an import map (data map) based on this data.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportMappingsImportMapRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ImportMappingsImportMapRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseImportRecordsImport](Invoke-DataverseImportRecordsImport.md)
Contains the data that is needed to submit an asynchronous job that uploads the transformed data into Dataverse.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportRecordsImportRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ImportRecordsImportRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseImportSolution](Invoke-DataverseImportSolution.md)
Contains the data that is needed to import a solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportSolutionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ImportSolutionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseImportSolutionAsync](Invoke-DataverseImportSolutionAsync.md)
Contains the data that is needed to import a solution using an asynchronous job.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportSolutionAsyncRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ImportSolutionAsyncRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseImportSolutions](Invoke-DataverseImportSolutions.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportSolutionsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ImportSolutionsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseImportTranslation](Invoke-DataverseImportTranslation.md)
Contains the data that is needed to import translations from a compressed file.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportTranslationRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ImportTranslationRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseImportTranslationAsync](Invoke-DataverseImportTranslationAsync.md)
Executes a ImportTranslationAsyncRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportTranslationAsyncRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ImportTranslationAsyncRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseIncrementKnowledgeArticleViewCount](Invoke-DataverseIncrementKnowledgeArticleViewCount.md)
Contains the data that is required to increment the per day view count of a knowledge article record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.IncrementKnowledgeArticleViewCountRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.IncrementKnowledgeArticleViewCountRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseInitializeAnnotationBlocksDownload](Invoke-DataverseInitializeAnnotationBlocksDownload.md)
Contains the data needed to initialize the download of one or more annotation data blocks.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InitializeAnnotationBlocksDownloadRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.InitializeAnnotationBlocksDownloadRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseInitializeAnnotationBlocksUpload](Invoke-DataverseInitializeAnnotationBlocksUpload.md)
Contains the data needed to initialize annotation storage for receiving (uploading) one or more annotation data blocks.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InitializeAnnotationBlocksUploadRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.InitializeAnnotationBlocksUploadRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseInitializeAttachmentBlocksDownload](Invoke-DataverseInitializeAttachmentBlocksDownload.md)
Contains the data needed to initialize the download of one or more attachment data blocks.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InitializeAttachmentBlocksDownloadRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.InitializeAttachmentBlocksDownloadRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseInitializeAttachmentBlocksUpload](Invoke-DataverseInitializeAttachmentBlocksUpload.md)
Contains the data needed to initialize attachment storage for receiving (uploading) one or more attachment data blocks.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InitializeAttachmentBlocksUploadRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.InitializeAttachmentBlocksUploadRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseInitializeFileBlocksDownload](Invoke-DataverseInitializeFileBlocksDownload.md)
Contains the data needed to initialize the download of one or more binary data blocks.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InitializeFileBlocksDownloadRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.InitializeFileBlocksDownloadRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseInitializeFileBlocksUpload](Invoke-DataverseInitializeFileBlocksUpload.md)
Contains the data needed to initialize file storage for receiving (uploading) one or more binary data blocks.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InitializeFileBlocksUploadRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.InitializeFileBlocksUploadRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseInitializeFrom](Invoke-DataverseInitializeFrom.md)
Contains the data that is needed to initialize a new record from an existing record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InitializeFromRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.InitializeFromRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseInitializeModernFlowFromAsyncWorkflow](Invoke-DataverseInitializeModernFlowFromAsyncWorkflow.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InitializeModernFlowFromAsyncWorkflowRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.InitializeModernFlowFromAsyncWorkflowRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseInsertOptionValue](Invoke-DataverseInsertOptionValue.md)
Contains the data that is needed to insert a new option value for a global choice or local choice column.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.InsertOptionValueRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.InsertOptionValueRequest)

### [Invoke-DataverseInsertStatusValue](Invoke-DataverseInsertStatusValue.md)
Contains the data that is needed to insert a new option into a column.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.InsertStatusValueRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.InsertStatusValueRequest)

### [Invoke-DataverseInstallSampleData](Invoke-DataverseInstallSampleData.md)
Contains the data that is needed to install the sample data.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InstallSampleDataRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.InstallSampleDataRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseInstantiateFilters](Invoke-DataverseInstantiateFilters.md)
Contains the data that is needed to instantiate a set of filters for Dynamics 365 for Outlook for the specified user.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InstantiateFiltersRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.InstantiateFiltersRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseInstantiateTemplate](Invoke-DataverseInstantiateTemplate.md)
Contains the parameters that are needed to create an email message from a template (email template).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InstantiateTemplateRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.InstantiateTemplateRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseIsComponentCustomizable](Invoke-DataverseIsComponentCustomizable.md)
Contains the data that is needed to determine whether a solution component is customizable.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.IsComponentCustomizableRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.IsComponentCustomizableRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseIsDataEncryptionActive](Invoke-DataverseIsDataEncryptionActive.md)
Contains the data that is needed to check if data encryption is currently running (active or inactive).

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.IsDataEncryptionActiveRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.IsDataEncryptionActiveRequest)

### [Invoke-DataverseIsValidStateTransition](Invoke-DataverseIsValidStateTransition.md)
Contains the data that is needed to validate the state transition.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.IsValidStateTransitionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.IsValidStateTransitionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseLocalTimeFromUtcTime](Invoke-DataverseLocalTimeFromUtcTime.md)
Contains the data that is needed to retrieve the local time for the specified Coordinated Universal Time (UTC).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.LocalTimeFromUtcTimeRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.LocalTimeFromUtcTimeRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseLockInvoicePricing](Invoke-DataverseLockInvoicePricing.md)
Contains the data that is needed to lock the total price of products and services that are specified in the invoice.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.LockInvoicePricingRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.LockInvoicePricingRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseLockSalesOrderPricing](Invoke-DataverseLockSalesOrderPricing.md)
Contains the data to lock sales order pricing.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.LockSalesOrderPricingRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.LockSalesOrderPricingRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseLoseOpportunity](Invoke-DataverseLoseOpportunity.md)
Contains the data that is needed to set the state of an opportunity to Lost.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.LoseOpportunityRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.LoseOpportunityRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseMerge](Invoke-DataverseMerge.md)
Contains the data that's needed to merge the information from two entity records of the same type.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.MergeRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.MergeRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseModifyAccess](Invoke-DataverseModifyAccess.md)
Contains the data that is needed to replace the access rights on the target record for the specified security principal (user, team, or organization).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ModifyAccessRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ModifyAccessRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseOrderOption](Invoke-DataverseOrderOption.md)
Contains the data that is needed to set the order for an option set.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.OrderOptionRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.OrderOptionRequest)

### [Invoke-DataverseParallel](Invoke-DataverseParallel.md)
Processes input objects in parallel using chunked batches with cloned Dataverse connections.

### [Invoke-DataverseParseImport](Invoke-DataverseParseImport.md)
Contains the data that is needed to submit an asynchronous job that parses all import files that are associated with the specified import (data import).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ParseImportRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ParseImportRequest?view=dataverse-sdk-latest)

### [Invoke-DataversePickFromQueue](Invoke-DataversePickFromQueue.md)
Contains the data that is needed to assign a queue item to a user and optionally remove the queue item from the queue.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.PickFromQueueRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.PickFromQueueRequest?view=dataverse-sdk-latest)

### [Invoke-DataversePreferredSolutionUsedBy](Invoke-DataversePreferredSolutionUsedBy.md)
Executes a PreferredSolutionUsedByRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.PreferredSolutionUsedByRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.PreferredSolutionUsedByRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseProcessInboundEmail](Invoke-DataverseProcessInboundEmail.md)
Contains the data that is needed to process the email responses from a marketing campaign.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ProcessInboundEmailRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ProcessInboundEmailRequest?view=dataverse-sdk-latest)

### [Invoke-DataversePropagateByExpression](Invoke-DataversePropagateByExpression.md)
Contains the data that is needed to create a quick campaign to distribute an activity to accounts, contacts, or leads that are selected by a query.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.PropagateByExpressionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.PropagateByExpressionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseProvisionLanguage](Invoke-DataverseProvisionLanguage.md)
Contains the data that is needed to provision a new language.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ProvisionLanguageRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ProvisionLanguageRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseProvisionLanguageAsync](Invoke-DataverseProvisionLanguageAsync.md)
Contains the data that is needed to provision a new language using a background job.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ProvisionLanguageAsyncRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ProvisionLanguageAsyncRequest?view=dataverse-sdk-latest)

### [Invoke-DataversePublishAllXml](Invoke-DataversePublishAllXml.md)
Contains the data that is needed to publish all changes to solution components.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.PublishAllXmlRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.PublishAllXmlRequest?view=dataverse-sdk-latest)

### [Invoke-DataversePublishAllXmlAsync](Invoke-DataversePublishAllXmlAsync.md)
Executes a PublishAllXmlAsyncRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.PublishAllXmlAsyncRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.PublishAllXmlAsyncRequest?view=dataverse-sdk-latest)

### [Invoke-DataversePublishDuplicateRule](Invoke-DataversePublishDuplicateRule.md)
Contains the data that is needed to submit an asynchronous job to publish a duplicate rule.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.PublishDuplicateRuleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.PublishDuplicateRuleRequest?view=dataverse-sdk-latest)

### [Invoke-DataversePublishProductHierarchy](Invoke-DataversePublishProductHierarchy.md)
Contain the data that is needed to publish a product family record and all its child records.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.PublishProductHierarchyRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.PublishProductHierarchyRequest?view=dataverse-sdk-latest)

### [Invoke-DataversePublishTheme](Invoke-DataversePublishTheme.md)
Contains the data that is needed to publish a theme and set it as the current theme.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.PublishThemeRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.PublishThemeRequest?view=dataverse-sdk-latest)

### [Invoke-DataversePublishXml](Invoke-DataversePublishXml.md)
Contains the data that is needed to publish specified solution components.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.PublishXmlRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.PublishXmlRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseQualifyLead](Invoke-DataverseQualifyLead.md)
Contains the data that is needed to qualify a lead and create account, contact, and opportunity records that are linked to the originating lead record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.QualifyLeadRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.QualifyLeadRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseQualifyMemberList](Invoke-DataverseQualifyMemberList.md)
Contains the data that is needed to qualify the specified list and either override the list members or remove them according to the specified option.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.QualifyMemberListRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.QualifyMemberListRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseQueryExpressionToFetchXml](Invoke-DataverseQueryExpressionToFetchXml.md)
Contains the data that is needed to convert a query, which is represented as a class, to its equivalent query, which is represented as FetchXML.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.QueryExpressionToFetchXmlRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.QueryExpressionToFetchXmlRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseQueryMultipleSchedules](Invoke-DataverseQueryMultipleSchedules.md)
Contains the data that is needed to search multiple resources for available time block that match the specified parameters.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.QueryMultipleSchedulesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.QueryMultipleSchedulesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseQuerySchedule](Invoke-DataverseQuerySchedule.md)
Contains the data that is needed to search the specified resource for an available time block that matches the specified parameters.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.QueryScheduleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.QueryScheduleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseQueueUpdateRibbonClientMetadata](Invoke-DataverseQueueUpdateRibbonClientMetadata.md)
Contains the data to queue UpdateRibbonClientMetadata to a background job.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.QueueUpdateRibbonClientMetadataRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.QueueUpdateRibbonClientMetadataRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseReactivateEntityKey](Invoke-DataverseReactivateEntityKey.md)
Contains data that is needed to submit a new asynchronous system job to create the index for the key.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.ReactivateEntityKeyRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.ReactivateEntityKeyRequest)

### [Invoke-DataverseReassignObjectsOwner](Invoke-DataverseReassignObjectsOwner.md)
Contains the data that is needed to reassign all records that are owned by the security principal (user, team, or organization) to another security principal (user, team, or organization).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ReassignObjectsOwnerRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ReassignObjectsOwnerRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseReassignObjectsSystemUser](Invoke-DataverseReassignObjectsSystemUser.md)
Contains the data that is needed to reassign all records that are owned by a specified user to another security principal (user, team, or organization).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ReassignObjectsSystemUserRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ReassignObjectsSystemUserRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRecalculate](Invoke-DataverseRecalculate.md)
Contains the data that is needed to recalculate system-computed values for rollup fields in the goal hierarchy.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RecalculateRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RecalculateRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseReleaseToQueue](Invoke-DataverseReleaseToQueue.md)
Contains the data that is needed to assign a queue item back to the queue owner so others can pick it.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ReleaseToQueueRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ReleaseToQueueRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRemoveAppComponents](Invoke-DataverseRemoveAppComponents.md)
Contains the data that is needed to remove a component from an app.For the Web API, use the RemoveAppComponents Action.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveAppComponentsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RemoveAppComponentsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRemoveFromQueue](Invoke-DataverseRemoveFromQueue.md)
Contains the data that is needed to remove a queue item from a queue.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveFromQueueRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RemoveFromQueueRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRemoveItemCampaign](Invoke-DataverseRemoveItemCampaign.md)
Contains the data that is needed to remove an item from a campaign.This message does not have a corresponding Web API action or function in Microsoft Dynamics 365 (online &amp; on-premises). More information: Missing functions and actions for some organization service messages.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveItemCampaignRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RemoveItemCampaignRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRemoveItemCampaignActivity](Invoke-DataverseRemoveItemCampaignActivity.md)
Contains the data that is needed to remove an item from a campaign activity.This message does not have a corresponding Web API action or function in Microsoft Dynamics 365 (online &amp; on-premises). More information: Missing functions and actions for some organization service messages.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveItemCampaignActivityRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RemoveItemCampaignActivityRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRemoveMemberList](Invoke-DataverseRemoveMemberList.md)
Contains the data that is needed to remove a member from a list (marketing list).This message does not have a corresponding Web API action or function in Microsoft Dynamics 365 (online &amp; on-premises). More information: Missing functions and actions for some organization service messages.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveMemberListRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RemoveMemberListRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRemoveMembersTeam](Invoke-DataverseRemoveMembersTeam.md)
Contains the data that is needed to remove members from a team.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveMembersTeamRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RemoveMembersTeamRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRemoveParent](Invoke-DataverseRemoveParent.md)
Contains the data that is needed to remove the parent for a system user (user) record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveParentRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RemoveParentRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRemovePrivilegeRole](Invoke-DataverseRemovePrivilegeRole.md)
Contains the data that is needed to remove a privilege from an existing role.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemovePrivilegeRoleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RemovePrivilegeRoleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRemoveRelated](Invoke-DataverseRemoveRelated.md)
Use the class. Contains the data that is needed to remove the relationship between the specified records for specific relationships.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveRelatedRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RemoveRelatedRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRemoveSolutionComponent](Invoke-DataverseRemoveSolutionComponent.md)
Contains the data that is needed to remove a component from an unmanaged solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveSolutionComponentRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RemoveSolutionComponentRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRemoveUserFromRecordTeam](Invoke-DataverseRemoveUserFromRecordTeam.md)
Contains the data that is needed to remove a user from the auto created access team for the specified record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveUserFromRecordTeamRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RemoveUserFromRecordTeamRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRenewContract](Invoke-DataverseRenewContract.md)
Contains the data that is needed to renew a contract and create the contract details for a new contract.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RenewContractRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RenewContractRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRenewEntitlement](Invoke-DataverseRenewEntitlement.md)
Contains the data that is needed to renew an entitlement.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RenewEntitlementRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RenewEntitlementRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseReplacePrivilegesRole](Invoke-DataverseReplacePrivilegesRole.md)
Contains the data that is needed to replace the privilege set of an existing role.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ReplacePrivilegesRoleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ReplacePrivilegesRoleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRequest](Invoke-DataverseRequest.md)
Invokes an arbitrary Dataverse request and returns the response.

### [Invoke-DataverseReschedule](Invoke-DataverseReschedule.md)
Contains the data that is needed to reschedule an appointment, recurring appointment, or service appointment (service activity).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RescheduleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RescheduleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseResetUserFilters](Invoke-DataverseResetUserFilters.md)
Contains the data that is needed to reset the offline data filters for the calling user to the default filters for the organization.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ResetUserFiltersRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ResetUserFiltersRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieve](Invoke-DataverseRetrieve.md)
Contains the data that is needed to retrieve a record.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveRequest)

### [Invoke-DataverseRetrieveAadUserPrivileges](Invoke-DataverseRetrieveAadUserPrivileges.md)
Executes a RetrieveAadUserPrivilegesRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAadUserPrivilegesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveAadUserPrivilegesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveAadUserRoles](Invoke-DataverseRetrieveAadUserRoles.md)
Executes a RetrieveAadUserRolesRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAadUserRolesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveAadUserRolesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveAadUserSetOfPrivilegesByIds](Invoke-DataverseRetrieveAadUserSetOfPrivilegesByIds.md)
Executes a RetrieveAadUserSetOfPrivilegesByIdsRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAadUserSetOfPrivilegesByIdsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveAadUserSetOfPrivilegesByIdsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveAadUserSetOfPrivilegesByNames](Invoke-DataverseRetrieveAadUserSetOfPrivilegesByNames.md)
Executes a RetrieveAadUserSetOfPrivilegesByNamesRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAadUserSetOfPrivilegesByNamesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveAadUserSetOfPrivilegesByNamesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveAbsoluteAndSiteCollectionUrl](Invoke-DataverseRetrieveAbsoluteAndSiteCollectionUrl.md)
Contains the data that is needed to retrieve the absolute URL and the site collection URL for a SharePoint location record in Dataverse.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAbsoluteAndSiteCollectionUrlRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveAbsoluteAndSiteCollectionUrlRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveActivePath](Invoke-DataverseRetrieveActivePath.md)
Contains the data to retrieve a collection of stages currently in the active path for a business process flow instance.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveActivePathRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveActivePathRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveAllChildUsersSystemUser](Invoke-DataverseRetrieveAllChildUsersSystemUser.md)
Contains the data that is needed to retrieve the collection of users that report to the specified system user (user).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAllChildUsersSystemUserRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveAllChildUsersSystemUserRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveAllEntities](Invoke-DataverseRetrieveAllEntities.md)
Contains the data that is needed to retrieve schema information about all tables.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveAllEntitiesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveAllEntitiesRequest)

### [Invoke-DataverseRetrieveAllManagedProperties](Invoke-DataverseRetrieveAllManagedProperties.md)
Contains the data that is needed to retrieve all managed property definitions.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveAllManagedPropertiesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveAllManagedPropertiesRequest)

### [Invoke-DataverseRetrieveAllOptionSets](Invoke-DataverseRetrieveAllOptionSets.md)
Contains the data that is needed to retrieve information about all global choices.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveAllOptionSetsRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveAllOptionSetsRequest)

### [Invoke-DataverseRetrieveAnalyticsStoreDetails](Invoke-DataverseRetrieveAnalyticsStoreDetails.md)
Contains the data to retrieves Analytics Store (aka 'Azure Data Lake' Storage container) details.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAnalyticsStoreDetailsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveAnalyticsStoreDetailsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveAppComponents](Invoke-DataverseRetrieveAppComponents.md)
Contains the data to return components of an App.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAppComponentsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveAppComponentsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveApplicationRibbon](Invoke-DataverseRetrieveApplicationRibbon.md)
Contains the data that is needed to retrieve the data that defines the content and behavior of the application ribbon.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveApplicationRibbonRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveApplicationRibbonRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveAttribute](Invoke-DataverseRetrieveAttribute.md)
Contains the data that is needed to retrieve attribute metadata.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveAttributeRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveAttributeRequest)

### [Invoke-DataverseRetrieveAttributeChangeHistory](Invoke-DataverseRetrieveAttributeChangeHistory.md)
Contains the data that is needed to retrieve all metadata changes to a specific attribute.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAttributeChangeHistoryRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveAttributeChangeHistoryRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveAuditDetails](Invoke-DataverseRetrieveAuditDetails.md)
Contains the data that is needed to retrieve the full audit details from an record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAuditDetailsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveAuditDetailsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveAuditPartitionList](Invoke-DataverseRetrieveAuditPartitionList.md)
Contains the data that is needed to retrieve the list of database partitions that are used to store audited history data.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAuditPartitionListRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveAuditPartitionListRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveAvailableLanguages](Invoke-DataverseRetrieveAvailableLanguages.md)
Contains the data that is needed to retrieve the list of language packs that are installed and enabled on the server.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAvailableLanguagesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveAvailableLanguagesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveBusinessHierarchyBusinessUnit](Invoke-DataverseRetrieveBusinessHierarchyBusinessUnit.md)
Contains the data that is needed to retrieve all business units (including the specified business unit) from the business unit hierarchy.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveBusinessHierarchyBusinessUnitRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveBusinessHierarchyBusinessUnitRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveByGroupResource](Invoke-DataverseRetrieveByGroupResource.md)
Contains the data that is needed to retrieve all resources that are related to the specified resource group (scheduling group).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveByGroupResourceRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveByGroupResourceRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveByResourceResourceGroup](Invoke-DataverseRetrieveByResourceResourceGroup.md)
Contains the data that is needed to retrieve the resource groups (scheduling groups) that contain the specified resource.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveByResourceResourceGroupRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveByResourceResourceGroupRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveByResourcesService](Invoke-DataverseRetrieveByResourcesService.md)
Contains the data that is needed to retrieve the collection of services that are related to the specified set of resources.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveByResourcesServiceRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveByResourcesServiceRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveByTopIncidentProductKbArticle](Invoke-DataverseRetrieveByTopIncidentProductKbArticle.md)
Contains the data that is needed to retrieve the top-ten articles about a specified product from the knowledge base of articles for your organization.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveByTopIncidentProductKbArticleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveByTopIncidentProductKbArticleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveByTopIncidentSubjectKbArticle](Invoke-DataverseRetrieveByTopIncidentSubjectKbArticle.md)
Contains the data that is needed to retrieve the top-ten articles about a specified subject from the knowledge base of articles for your organization.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveByTopIncidentSubjectKbArticleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveByTopIncidentSubjectKbArticleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveChannelAccessProfilePrivileges](Invoke-DataverseRetrieveChannelAccessProfilePrivileges.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveChannelAccessProfilePrivilegesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveChannelAccessProfilePrivilegesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveCurrentOrganization](Invoke-DataverseRetrieveCurrentOrganization.md)
Contains the data that's needed to retrieve information about the current organization.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveCurrentOrganizationRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveCurrentOrganizationRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveDataEncryptionKey](Invoke-DataverseRetrieveDataEncryptionKey.md)
Contains the data that is needed to retrieve the data encryption key value.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveDataEncryptionKeyRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveDataEncryptionKeyRequest)

### [Invoke-DataverseRetrieveDependenciesForDelete](Invoke-DataverseRetrieveDependenciesForDelete.md)
Contains the data that is needed to retrieve a collection of dependency records that describe any solution components that would prevent a solution component from being deleted.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveDependenciesForDeleteRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveDependenciesForDeleteRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveDependenciesForUninstall](Invoke-DataverseRetrieveDependenciesForUninstall.md)
Contains the data that is needed to retrieve a list of the solution component dependencies that can prevent you from uninstalling a managed solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveDependenciesForUninstallRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveDependenciesForUninstallRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveDependentComponents](Invoke-DataverseRetrieveDependentComponents.md)
Contains the data that is needed to retrieves a list dependencies for solution components that directly depend on a solution component.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveDependentComponentsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveDependentComponentsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveDeploymentLicenseType](Invoke-DataverseRetrieveDeploymentLicenseType.md)
Contains the data that is needed to retrieve the type of license for a deployment of Microsoft Dynamics 365.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveDeploymentLicenseTypeRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveDeploymentLicenseTypeRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveDeprovisionedLanguages](Invoke-DataverseRetrieveDeprovisionedLanguages.md)
Contains the data that is needed to retrieve a list of language packs that are installed on the server that have been disabled.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveDeprovisionedLanguagesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveDeprovisionedLanguagesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveDuplicates](Invoke-DataverseRetrieveDuplicates.md)
Contains the data that is needed to detect and retrieve duplicates for a specified record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveDuplicatesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveDuplicatesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveEntity](Invoke-DataverseRetrieveEntity.md)
Contains the data that is needed to retrieve the definition of a table.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveEntityRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveEntityRequest)

### [Invoke-DataverseRetrieveEntityChanges](Invoke-DataverseRetrieveEntityChanges.md)
Contains data that is needed to retrieve the changes for an record.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveEntityChangesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveEntityChangesRequest)

### [Invoke-DataverseRetrieveEntityKey](Invoke-DataverseRetrieveEntityKey.md)
Contains data that is needed to retrieve an alternate key.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveEntityKeyRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveEntityKeyRequest)

### [Invoke-DataverseRetrieveEntityRibbon](Invoke-DataverseRetrieveEntityRibbon.md)
Contains the data that is needed to retrieve ribbon definitions for an entity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveEntityRibbonRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveEntityRibbonRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveExchangeAppointments](Invoke-DataverseRetrieveExchangeAppointments.md)
Retrieves the appointments for the current user for a specific date range from the exchange web service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveExchangeAppointmentsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveExchangeAppointmentsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveExchangeRate](Invoke-DataverseRetrieveExchangeRate.md)
Contains the data that is needed to retrieve the exchange rate.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveExchangeRateRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveExchangeRateRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveFeatureControlSetting](Invoke-DataverseRetrieveFeatureControlSetting.md)
Executes a RetrieveFeatureControlSettingRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveFeatureControlSettingRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveFeatureControlSettingRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveFeatureControlSettings](Invoke-DataverseRetrieveFeatureControlSettings.md)
Executes a RetrieveFeatureControlSettingsRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveFeatureControlSettingsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveFeatureControlSettingsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveFeatureControlSettingsByNamespace](Invoke-DataverseRetrieveFeatureControlSettingsByNamespace.md)
Executes a RetrieveFeatureControlSettingsByNamespaceRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveFeatureControlSettingsByNamespaceRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveFeatureControlSettingsByNamespaceRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveFilteredForms](Invoke-DataverseRetrieveFilteredForms.md)
Contains the data that is needed to retrieve the entity forms that are available for a specified user.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveFilteredFormsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveFilteredFormsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveFormattedImportJobResults](Invoke-DataverseRetrieveFormattedImportJobResults.md)
Contains the data that is needed to retrieve the formatted results from an import job.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveFormattedImportJobResultsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveFormattedImportJobResultsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveFormXml](Invoke-DataverseRetrieveFormXml.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveFormXmlRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveFormXmlRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveInstalledLanguagePacks](Invoke-DataverseRetrieveInstalledLanguagePacks.md)
Contains the data that is needed to retrieve the list of language packs that are installed on the server.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveInstalledLanguagePacksRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveInstalledLanguagePacksRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveInstalledLanguagePackVersion](Invoke-DataverseRetrieveInstalledLanguagePackVersion.md)
Contains the data that is needed to retrieve the version of an installed language pack.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveInstalledLanguagePackVersionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveInstalledLanguagePackVersionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveLicenseInfo](Invoke-DataverseRetrieveLicenseInfo.md)
Contains the data that is needed to retrieve the number of used and available licenses for a deployment of Microsoft Dynamics 365.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveLicenseInfoRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveLicenseInfoRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveLocLabels](Invoke-DataverseRetrieveLocLabels.md)
Contains the data that is needed to retrieve localized labels for a limited set of entity attributes.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveLocLabelsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveLocLabelsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveMailboxTrackingFolders](Invoke-DataverseRetrieveMailboxTrackingFolders.md)
Contains the data needed to retrieve folder-level tracking rules for a mailbox.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveMailboxTrackingFoldersRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveMailboxTrackingFoldersRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveManagedProperty](Invoke-DataverseRetrieveManagedProperty.md)
Contains the data that is needed to retrieve a managed property definition.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveManagedPropertyRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveManagedPropertyRequest)

### [Invoke-DataverseRetrieveMembersBulkOperation](Invoke-DataverseRetrieveMembersBulkOperation.md)
Contains the data that is needed to retrieve the members of a bulk operation.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveMembersBulkOperationRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveMembersBulkOperationRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveMetadataChanges](Invoke-DataverseRetrieveMetadataChanges.md)
Contains the data that is needed to retrieve a collection of records that satisfy the specified criteria. The returns a value that can be used with this request at a later time to return information about how schema definitions have changed since the last request.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveMetadataChangesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveMetadataChangesRequest)

### [Invoke-DataverseRetrieveMissingComponents](Invoke-DataverseRetrieveMissingComponents.md)
Contains the data that is needed to retrieve a list of missing components in the target organization.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveMissingComponentsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveMissingComponentsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveMissingDependencies](Invoke-DataverseRetrieveMissingDependencies.md)
Contains the data that is needed to retrieve any required solution components that are not included in the solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveMissingDependenciesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveMissingDependenciesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveMultiple](Invoke-DataverseRetrieveMultiple.md)
Contains the data that is needed to retrieve a collection of records that satisfy the specified query criteria.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest)

### [Invoke-DataverseRetrieveOptionSet](Invoke-DataverseRetrieveOptionSet.md)
Contains the data that is needed to retrieve a global choice.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveOptionSetRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveOptionSetRequest)

### [Invoke-DataverseRetrieveOrganizationInfo](Invoke-DataverseRetrieveOrganizationInfo.md)
Contains the data that is needed to retrieve information about an organization such as the instance type and solutions available in the organization.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveOrganizationInfoRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveOrganizationInfoRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveOrganizationResources](Invoke-DataverseRetrieveOrganizationResources.md)
Contains the data that is needed to retrieve the resources that are used by an organization.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveOrganizationResourcesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveOrganizationResourcesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveParentGroupsResourceGroup](Invoke-DataverseRetrieveParentGroupsResourceGroup.md)
Contains the data needed to retrieve the collection of the parent resource groups of the specified resource group (scheduling group).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveParentGroupsResourceGroupRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveParentGroupsResourceGroupRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveParsedDataImportFile](Invoke-DataverseRetrieveParsedDataImportFile.md)
Contains the data that is needed to retrieve the data from the parse table.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveParsedDataImportFileRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveParsedDataImportFileRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrievePersonalWall](Invoke-DataverseRetrievePersonalWall.md)
Contains the data that is needed to retrieve pages of posts, including comments for each post, for all records that the calling user is following.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrievePersonalWallRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrievePersonalWallRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrievePrincipalAccess](Invoke-DataverseRetrievePrincipalAccess.md)
Contains the data that is needed to retrieve the access rights of the specified security principal (user, team, or organization) to the specified record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrievePrincipalAccessRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrievePrincipalAccessRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrievePrincipalAccessInfo](Invoke-DataverseRetrievePrincipalAccessInfo.md)
Executes a RetrievePrincipalAccessInfoRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrievePrincipalAccessInfoRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrievePrincipalAccessInfoRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrievePrincipalAttributePrivileges](Invoke-DataverseRetrievePrincipalAttributePrivileges.md)
Contains the data that is needed to retrieves all the secured attribute privileges a user or team has through direct or indirect (through team membership) associations with the entity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrievePrincipalAttributePrivilegesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrievePrincipalAttributePrivilegesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrievePrincipalSyncAttributeMappings](Invoke-DataverseRetrievePrincipalSyncAttributeMappings.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrievePrincipalSyncAttributeMappingsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrievePrincipalSyncAttributeMappingsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrievePrivilegeSet](Invoke-DataverseRetrievePrivilegeSet.md)
Contains the data needed to retrieve the set of privileges defined in the system.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrievePrivilegeSetRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrievePrivilegeSetRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveProcessInstances](Invoke-DataverseRetrieveProcessInstances.md)
Contains the data that is needed to retrieve all the process instances for an entity record across all business process definitions.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveProcessInstancesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveProcessInstancesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveProductProperties](Invoke-DataverseRetrieveProductProperties.md)
Contains data that is needed to retrieve all the property instances (dynamic property instances) for a product added to an opportunity, quote, order, or invoice.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveProductPropertiesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveProductPropertiesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveProvisionedLanguagePackVersion](Invoke-DataverseRetrieveProvisionedLanguagePackVersion.md)
Contains the data that is needed to retrieve the version of a provisioned language pack.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagePackVersionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveProvisionedLanguagePackVersionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveProvisionedLanguages](Invoke-DataverseRetrieveProvisionedLanguages.md)
Contains the data that is needed to retrieve the list of provisioned languages.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveProvisionedLanguagesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveRecordChangeHistory](Invoke-DataverseRetrieveRecordChangeHistory.md)
Contains the data that is needed to retrieve all attribute data changes for a specific entity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveRecordChangeHistoryRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveRecordChangeHistoryRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveRecordWall](Invoke-DataverseRetrieveRecordWall.md)
Contains the data that is needed to retrieve pages of posts, including comments for each post, for a specified record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveRecordWallRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveRecordWallRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveRelationship](Invoke-DataverseRetrieveRelationship.md)
Contains the data that is needed to retrieve table relationship metadata.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveRelationshipRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveRelationshipRequest)

### [Invoke-DataverseRetrieveRequiredComponents](Invoke-DataverseRetrieveRequiredComponents.md)
Contains the data that is needed to retrieve a collection of solution components that are required for a solution component.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveRequiredComponentsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveRequiredComponentsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveRolePrivilegesRole](Invoke-DataverseRetrieveRolePrivilegesRole.md)
Contains the data that is needed to retrieve the privileges that are assigned to the specified role.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveRolePrivilegesRoleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveRolePrivilegesRoleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveSharedLinks](Invoke-DataverseRetrieveSharedLinks.md)
Retrieve all shared links on the table row.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveSharedLinksRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveSharedLinksRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveSharedPrincipalsAndAccess](Invoke-DataverseRetrieveSharedPrincipalsAndAccess.md)
Contains the data that is needed to retrieve all security principals (users, teams, and organizations) that have access to, and access rights for, the specified record because it was shared with them.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveSharedPrincipalsAndAccessRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveSharedPrincipalsAndAccessRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveSubGroupsResourceGroup](Invoke-DataverseRetrieveSubGroupsResourceGroup.md)
Contains the data that is needed to retrieve the collection of child resource groups from the specified Scheduling Group (ResourceGroup).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveSubGroupsResourceGroupRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveSubGroupsResourceGroupRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveTeamPrivileges](Invoke-DataverseRetrieveTeamPrivileges.md)
Contains the data that is needed to retrieve the privileges for a team.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveTeamPrivilegesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveTeamPrivilegesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveTimelineWallRecords](Invoke-DataverseRetrieveTimelineWallRecords.md)
Contains the data to retrieve all the activity pointer entities along with the parties and attachments as dictated by the FetchXML query.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveTimelineWallRecordsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveTimelineWallRecordsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveTimestamp](Invoke-DataverseRetrieveTimestamp.md)
Contains the data that is needed to retrieves a time stamp for the metadata.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.RetrieveTimestampRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.RetrieveTimestampRequest)

### [Invoke-DataverseRetrieveTotalRecordCount](Invoke-DataverseRetrieveTotalRecordCount.md)
Contains the data to retrieve the total entity record count from within the last 24 hours.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveTotalRecordCountRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveTotalRecordCountRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveUnpublished](Invoke-DataverseRetrieveUnpublished.md)
Contains the data that is needed to retrieve an unpublished record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveUnpublishedRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveUnpublishedMultiple](Invoke-DataverseRetrieveUnpublishedMultiple.md)
Contains the data that is needed to retrieve a collection of unpublished organization-owned records that satisfy the specified query criteria.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveUnpublishedMultipleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveUserLicenseInfo](Invoke-DataverseRetrieveUserLicenseInfo.md)
Contains the data needed to retrieve the license information for the specified system user (user).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUserLicenseInfoRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveUserLicenseInfoRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveUserPrivilegeByPrivilegeId](Invoke-DataverseRetrieveUserPrivilegeByPrivilegeId.md)
Contains the data to retrieve a list of privileges a system user (user) has through their roles, and inherited privileges from their team membership, based on the specified privilege ID.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegeByPrivilegeIdRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveUserPrivilegeByPrivilegeIdRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveUserPrivilegeByPrivilegeName](Invoke-DataverseRetrieveUserPrivilegeByPrivilegeName.md)
Contains the data to retrieve a list of privileges for a system user (user) has through their roles, and inherited privileges from their team membership, based on the specified privilege name.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegeByPrivilegeNameRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveUserPrivilegeByPrivilegeNameRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveUserPrivileges](Invoke-DataverseRetrieveUserPrivileges.md)
Contains the data to retrieve the privileges a system user (user) has through their roles, and inherited privileges from their team membership.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveUserPrivilegesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveUserQueues](Invoke-DataverseRetrieveUserQueues.md)
Contains the data needed to retrieve all private queues of a specified user and optionally all public queues.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUserQueuesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveUserQueuesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveUserSetOfPrivilegesByIds](Invoke-DataverseRetrieveUserSetOfPrivilegesByIds.md)
Contains the data to retrieve a list of privileges a system user (user) has through their roles, and inherited privileges from their team membership, based on the specified privilege IDs.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUserSetOfPrivilegesByIdsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveUserSetOfPrivilegesByIdsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveUserSetOfPrivilegesByNames](Invoke-DataverseRetrieveUserSetOfPrivilegesByNames.md)
Contains the data to retrieve a list of privileges for a system user (user) has through their roles, and inherited privileges from their team membership, based on the specified privilege names.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUserSetOfPrivilegesByNamesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveUserSetOfPrivilegesByNamesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveUsersPrivilegesThroughTeams](Invoke-DataverseRetrieveUsersPrivilegesThroughTeams.md)
Contains the data to retrieve privileges which the user gets through team memberships.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUsersPrivilegesThroughTeamsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveUsersPrivilegesThroughTeamsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRetrieveVersion](Invoke-DataverseRetrieveVersion.md)
Contains the data that is needed to retrieve the Dataverse version number.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveVersionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RetrieveVersionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRevertProduct](Invoke-DataverseRevertProduct.md)
Contains the data that is needed to revert changes done to properties of a product family, product, or bundle record, and set it back to its last published () state.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RevertProductRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RevertProductRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseReviseQuote](Invoke-DataverseReviseQuote.md)
Contains the data that is needed to set the state of a quote to Draft.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ReviseQuoteRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ReviseQuoteRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRevokeAccess](Invoke-DataverseRevokeAccess.md)
Contains the data that is needed to replace the access rights on the target record for the specified security principal (user, team, or organization).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RevokeAccessRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RevokeAccessRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRevokeSharedLink](Invoke-DataverseRevokeSharedLink.md)
Revokes user access rights from a shared link.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RevokeSharedLinkRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RevokeSharedLinkRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRollup](Invoke-DataverseRollup.md)
Contains the data that is needed to retrieve all the entity records that are related to the specified record.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RollupRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RollupRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseRouteTo](Invoke-DataverseRouteTo.md)
Contains the data that is needed to route a queue item to a queue, a user, or a team.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RouteToRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.RouteToRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSearch](Invoke-DataverseSearch.md)
Contains the data needed to search for available time slots that fulfill the specified appointment request.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SearchRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SearchRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSearchByBodyKbArticle](Invoke-DataverseSearchByBodyKbArticle.md)
Contains the data that is needed to search for knowledge base articles that contain the specified body text.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SearchByBodyKbArticleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SearchByBodyKbArticleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSearchByKeywordsKbArticle](Invoke-DataverseSearchByKeywordsKbArticle.md)
Contains the data that is needed to search for knowledge base articles that contain the specified keywords.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SearchByKeywordsKbArticleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SearchByKeywordsKbArticleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSearchByTitleKbArticle](Invoke-DataverseSearchByTitleKbArticle.md)
Contains the data that is needed to search for knowledge base articles that contain the specified title.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SearchByTitleKbArticleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SearchByTitleKbArticleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSendBulkMail](Invoke-DataverseSendBulkMail.md)
Contains the data that is needed to send bulk email messages.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SendBulkMailRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SendBulkMailRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSendEmail](Invoke-DataverseSendEmail.md)
Contains the data that is needed to send an email message.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SendEmailRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SendEmailRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSendEmailFromTemplate](Invoke-DataverseSendEmailFromTemplate.md)
Contains the data that is needed to send an email message using a template.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SendEmailFromTemplateRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SendEmailFromTemplateRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSendFax](Invoke-DataverseSendFax.md)
Contains the data that is needed to send a fax.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SendFaxRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SendFaxRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSendTemplate](Invoke-DataverseSendTemplate.md)
Contains the data that is needed to send a bulk email message that is created from a template.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SendTemplateRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SendTemplateRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSetAutoNumberSeed](Invoke-DataverseSetAutoNumberSeed.md)
Requests the seed number being used for autonumber generation.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetAutoNumberSeedRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SetAutoNumberSeedRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSetAutoNumberSeed1](Invoke-DataverseSetAutoNumberSeed1.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetAutoNumberSeed1Request](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SetAutoNumberSeed1Request?view=dataverse-sdk-latest)

### [Invoke-DataverseSetDataEncryptionKey](Invoke-DataverseSetDataEncryptionKey.md)
Contains the data that is needed to set or restore the data encryption key.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.SetDataEncryptionKeyRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.SetDataEncryptionKeyRequest)

### [Invoke-DataverseSetFeatureStatus](Invoke-DataverseSetFeatureStatus.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetFeatureStatusRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SetFeatureStatusRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSetLocLabels](Invoke-DataverseSetLocLabels.md)
Contains the data that is needed to set localized labels for a limited set of entity attributes.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetLocLabelsRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SetLocLabelsRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSetParentBusinessUnit](Invoke-DataverseSetParentBusinessUnit.md)
Contains the data that is needed to set the parent business unit for a business unit.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetParentBusinessUnitRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SetParentBusinessUnitRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSetPreferredSolution](Invoke-DataverseSetPreferredSolution.md)
Executes a SetPreferredSolutionRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetPreferredSolutionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SetPreferredSolutionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSetProcess](Invoke-DataverseSetProcess.md)
Contains the data that is needed to set another business process flow instance as the active process instance for the target entity.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetProcessRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SetProcessRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSetRelated](Invoke-DataverseSetRelated.md)
Contains the data needed to create a relationship between a set of records that participate in specific relationships.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetRelatedRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SetRelatedRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSetReportRelated](Invoke-DataverseSetReportRelated.md)
Contains the data needed to link an instance of a report entity to related entities.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SetReportRelatedRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SetReportRelatedRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSql](Invoke-DataverseSql.md)
Invokes a Dataverse SQL query using Sql4Cds and writes any resulting rows to the pipeline.

### [Invoke-DataverseStageAndUpgrade](Invoke-DataverseStageAndUpgrade.md)
Contains the data to import a solution, stage it for upgrade, and apply the upgrade as the default (when applicable).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.StageAndUpgradeRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.StageAndUpgradeRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseStageAndUpgradeAsync](Invoke-DataverseStageAndUpgradeAsync.md)
Contains the data to asynchronously import a solution, stage it for upgrade, and apply the upgrade as the default (when applicable).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.StageAndUpgradeAsyncRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.StageAndUpgradeAsyncRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseStageSolution](Invoke-DataverseStageSolution.md)
Contains the data needed to stage a solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.StageSolutionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.StageSolutionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseSyncBulkOperation](Invoke-DataverseSyncBulkOperation.md)
Contains data to perform bulk operations to cancel, resume, or pause workflows.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SyncBulkOperationRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.SyncBulkOperationRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseTransformImport](Invoke-DataverseTransformImport.md)
Contains the data that is needed to submit an asynchronous job that transforms the parsed data.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.TransformImportRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.TransformImportRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseTriggerServiceEndpointCheck](Invoke-DataverseTriggerServiceEndpointCheck.md)
Contains the data that is needed to validate the configuration of a Microsoft Azure Service Bus solution's service endpoint.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.TriggerServiceEndpointCheckRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.TriggerServiceEndpointCheckRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseUninstallSampleData](Invoke-DataverseUninstallSampleData.md)
Uninstalls the sample data.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UninstallSampleDataRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.UninstallSampleDataRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseUninstallSolutionAsync](Invoke-DataverseUninstallSolutionAsync.md)
Executes a UninstallSolutionAsyncRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UninstallSolutionAsyncRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.UninstallSolutionAsyncRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseUnlockInvoicePricing](Invoke-DataverseUnlockInvoicePricing.md)
Contains the data that is needed to unlock pricing for an invoice.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UnlockInvoicePricingRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.UnlockInvoicePricingRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseUnlockSalesOrderPricing](Invoke-DataverseUnlockSalesOrderPricing.md)
Contains the data that is needed to unlock pricing for a sales order (order).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UnlockSalesOrderPricingRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.UnlockSalesOrderPricingRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseUnpublishDuplicateRule](Invoke-DataverseUnpublishDuplicateRule.md)
Contains the data that is needed to submit an asynchronous job to unpublish a duplicate rule.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UnpublishDuplicateRuleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.UnpublishDuplicateRuleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseUpdate](Invoke-DataverseUpdate.md)
Contains the data that is needed to update an existing record.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.UpdateRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.UpdateRequest)

### [Invoke-DataverseUpdateAttribute](Invoke-DataverseUpdateAttribute.md)
Contains the data that is needed to update the definition of a column.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.UpdateAttributeRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.UpdateAttributeRequest)

### [Invoke-DataverseUpdateEntity](Invoke-DataverseUpdateEntity.md)
Contains the data that is needed to update the definition of a table.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.UpdateEntityRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.UpdateEntityRequest)

### [Invoke-DataverseUpdateFeatureConfig](Invoke-DataverseUpdateFeatureConfig.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UpdateFeatureConfigRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.UpdateFeatureConfigRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseUpdateMultiple](Invoke-DataverseUpdateMultiple.md)
Contains the data to update multiple records of the same type with a single web request.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.UpdateMultipleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.UpdateMultipleRequest)

### [Invoke-DataverseUpdateOptionSet](Invoke-DataverseUpdateOptionSet.md)
Contains the data that is needed to update the definition of a global choice.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.UpdateOptionSetRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.UpdateOptionSetRequest)

### [Invoke-DataverseUpdateOptionValue](Invoke-DataverseUpdateOptionValue.md)
Contains the data that is needed to update an option value in a global or local choice.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.UpdateOptionValueRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.UpdateOptionValueRequest)

### [Invoke-DataverseUpdateProductProperties](Invoke-DataverseUpdateProductProperties.md)
Contains the data that is needed to update values of the property instances (dynamic property instances) for a product added to an opportunity, quote, order, or invoice.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UpdateProductPropertiesRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.UpdateProductPropertiesRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseUpdateRelationship](Invoke-DataverseUpdateRelationship.md)
Contains the data that is needed to update the definition of an table relationship.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.UpdateRelationshipRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.UpdateRelationshipRequest)

### [Invoke-DataverseUpdateRibbonClientMetadata](Invoke-DataverseUpdateRibbonClientMetadata.md)
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UpdateRibbonClientMetadataRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.UpdateRibbonClientMetadataRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseUpdateSolutionComponent](Invoke-DataverseUpdateSolutionComponent.md)
Contains the data that is needed to update a component in an unmanaged solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UpdateSolutionComponentRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.UpdateSolutionComponentRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseUpdateStateValue](Invoke-DataverseUpdateStateValue.md)
Contains the data that is needed to update an option set value in for a column.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.UpdateStateValueRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.UpdateStateValueRequest)

### [Invoke-DataverseUploadBlock](Invoke-DataverseUploadBlock.md)
Contains the data needed to upload a block of data to storage.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UploadBlockRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.UploadBlockRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseUpsert](Invoke-DataverseUpsert.md)
Contains data that is needed to update or insert a record in Dataverse.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.UpsertRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.UpsertRequest)

### [Invoke-DataverseUpsertMultiple](Invoke-DataverseUpsertMultiple.md)
Contains the data to create or update multiple records of the same type in a single request.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.UpsertMultipleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.UpsertMultipleRequest)

### [Invoke-DataverseUtcTimeFromLocalTime](Invoke-DataverseUtcTimeFromLocalTime.md)
Contains the data that is needed to retrieve the Coordinated Universal Time (UTC) for the specified local time.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UtcTimeFromLocalTimeRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.UtcTimeFromLocalTimeRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseValidate](Invoke-DataverseValidate.md)
Contains the data that is needed to verify that an appointment or service appointment (service activity) has valid available resources for the activity, duration, and site, as appropriate.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ValidateRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ValidateRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseValidateApp](Invoke-DataverseValidateApp.md)
Contains the data to validate an App.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ValidateAppRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ValidateAppRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseValidateFetchXmlExpression](Invoke-DataverseValidateFetchXmlExpression.md)
Executes a ValidateFetchXmlExpressionRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ValidateFetchXmlExpressionRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ValidateFetchXmlExpressionRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseValidateRecurrenceRule](Invoke-DataverseValidateRecurrenceRule.md)
Contains the data that is needed to validate a rule for a recurring appointment.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ValidateRecurrenceRuleRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ValidateRecurrenceRuleRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseValidateSavedQuery](Invoke-DataverseValidateSavedQuery.md)
Contains the data that is needed to validate a saved query (view).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ValidateSavedQueryRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ValidateSavedQueryRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseValidateUnpublished](Invoke-DataverseValidateUnpublished.md)
Contains the data to validate that a mobile offline profile, including unpublished changes, can be published.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ValidateUnpublishedRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.ValidateUnpublishedRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseWhoAmI](Invoke-DataverseWhoAmI.md)
Contains the data that is needed to retrieve the system user ID for the currently logged on user or the user under whose context the code is running.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.WhoAmIRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.WhoAmIRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseWinOpportunity](Invoke-DataverseWinOpportunity.md)
Contains the data that is needed to set the state of an opportunity to Won.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.WinOpportunityRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.WinOpportunityRequest?view=dataverse-sdk-latest)

### [Invoke-DataverseWinQuote](Invoke-DataverseWinQuote.md)
Contains the data that is needed to set the state of a quote to Won.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.WinQuoteRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.WinQuoteRequest?view=dataverse-sdk-latest)

### [Remove-DataverseRecord](Remove-DataverseRecord.md)
Deletes an existing Dataverse record, including M:M association records.

### [Set-DataverseRecord](Set-DataverseRecord.md)
Creates or updates Dataverse records including M:M association/disassociation, status and assignment changes.

### [Set-DataverseRecordsFolder](Set-DataverseRecordsFolder.md)
Writes a list of Dataverse records to a folder of JSON files.

