---
title: "Set-DataverseEntityMetadata - Create entity with activities and notes support"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates an entity that supports activities (tasks, appointments) and notes (annotations).

```powershell
Set-DataverseEntityMetadata -EntityName new_project `
   -SchemaName new_Project `
   -DisplayName "Project" `
   -DisplayCollectionName "Projects" `
   -OwnershipType UserOwned `
   -HasActivities `
   -HasNotes `
   -PrimaryAttributeSchemaName new_name `
   -PrimaryAttributeDisplayName "Project Name"

```
