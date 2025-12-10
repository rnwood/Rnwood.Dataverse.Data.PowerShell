---
title: "Set-DataverseRelationshipMetadata - Create a ManyToMany relationship"
tags: ['Metadata']
source: "Set-DataverseRelationshipMetadata.md"
---
Creates a ManyToMany relationship between new_project and contact tables.

```powershell
Set-DataverseRelationshipMetadata -SchemaName "new_project_contact" `
   -RelationshipType "ManyToMany" `
   -ReferencedEntity "new_project" `
   -ReferencingEntity "contact" `
   -IntersectEntitySchemaName "new_project_contact"

```
