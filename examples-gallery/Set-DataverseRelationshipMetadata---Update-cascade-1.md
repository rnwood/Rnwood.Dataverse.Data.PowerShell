---
title: "Set-DataverseRelationshipMetadata - Update cascade behavior on existing relationship"
tags: ['Metadata']
source: "Set-DataverseRelationshipMetadata.md"
---
Updates an existing OneToMany relationship to change its cascade behaviors.

```powershell
Set-DataverseRelationshipMetadata -SchemaName "new_project_contact" `
   -RelationshipType "OneToMany" `
   -ReferencedEntity "new_project" `
   -ReferencingEntity "contact" `
   -CascadeDelete "Cascade" `
   -CascadeAssign "Cascade"

```
