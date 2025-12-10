---
title: "Set-DataverseEntityMetadata - Create entity with activities, notes, and audit"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates a full-featured entity with all available options enabled.

```powershell
Set-DataverseEntityMetadata -EntityName new_case `
   -SchemaName new_Case `
   -DisplayName "Support Case" `
   -DisplayCollectionName "Support Cases" `
   -OwnershipType UserOwned `
   -HasActivities `
   -HasNotes `
   -IsAuditEnabled `
   -ChangeTrackingEnabled `
   -PrimaryAttributeSchemaName new_casenumber `
   -PrimaryAttributeDisplayName "Case Number" `
   -PrimaryAttributeMaxLength 20

```
