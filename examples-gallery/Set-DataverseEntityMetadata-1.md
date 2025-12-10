---
title: "Set-DataverseEntityMetadata"
tags: ['CRUD', 'Metadata']
source: "Metadata-CRUD-Examples.md"
---
Creates or updates entity metadata.

```powershell
# Create a new custom entity
Set-DataverseEntityMetadata -EntityName new_project `
   -SchemaName new_Project `
   -DisplayName "Project" `
   -DisplayCollectionName "Projects" `
   -Description "Manages projects" `
   -OwnershipType UserOwned `
   -PrimaryAttributeSchemaName new_name `
   -PrimaryAttributeDisplayName "Project Name" `
   -PrimaryAttributeMaxLength 200 `
   -HasActivities `
   -HasNotes `
   -IsAuditEnabled `
   -PassThru

# Update an existing entity
Set-DataverseEntityMetadata -EntityName new_project `
   -DisplayName "Projects (Updated)" `
   -Description "Updated description for projects" `
   -IsAuditEnabled `
   -ChangeTrackingEnabled `
   -Force `
   -PassThru

```
