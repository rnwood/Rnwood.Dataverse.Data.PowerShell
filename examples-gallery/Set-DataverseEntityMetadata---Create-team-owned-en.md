---
title: "Set-DataverseEntityMetadata - Create team-owned entity"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates a team-owned entity where records are owned by teams rather than individual users.

```powershell
Set-DataverseEntityMetadata -EntityName new_resource `
   -SchemaName new_Resource `
   -DisplayName "Shared Resource" `
   -DisplayCollectionName "Shared Resources" `
   -OwnershipType TeamOwned `
   -PrimaryAttributeSchemaName new_name `
   -PrimaryAttributeDisplayName "Resource Name"

```
