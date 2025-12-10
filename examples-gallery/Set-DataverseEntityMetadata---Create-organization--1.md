---
title: "Set-DataverseEntityMetadata - Create organization-owned entity"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates an organization-owned entity (not owned by specific users or teams).

```powershell
Set-DataverseEntityMetadata -EntityName new_setting `
   -SchemaName new_Setting `
   -DisplayName "Application Setting" `
   -DisplayCollectionName "Application Settings" `
   -OwnershipType OrganizationOwned `
   -PrimaryAttributeSchemaName new_name `
   -PrimaryAttributeDisplayName "Setting Name"

```
