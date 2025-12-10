---
title: "Set-DataverseRelationshipMetadata - Create relationship with full cascade configuration"
tags: ['Metadata']
source: "Set-DataverseRelationshipMetadata.md"
---
Creates a fully cascading parent-child relationship with all cascade operations set to Cascade.

```powershell
Set-DataverseRelationshipMetadata -SchemaName "new_task_project" `
   -RelationshipType "OneToMany" `
   -ReferencedEntity "new_project" `
   -ReferencingEntity "new_task" `
   -LookupAttributeSchemaName "new_ProjectId" `
   -LookupAttributeDisplayName "Project" `
   -LookupAttributeRequiredLevel "ApplicationRequired" `
   -CascadeAssign "Cascade" `
   -CascadeShare "Cascade" `
   -CascadeUnshare "Cascade" `
   -CascadeReparent "Cascade" `
   -CascadeDelete "Cascade" `
   -CascadeMerge "Cascade" `
   -PassThru

```
