---
title: "Set-DataverseRelationshipMetadata - Create a OneToMany relationship"
tags: ['Metadata']
source: "Set-DataverseRelationshipMetadata.md"
---
Creates a OneToMany relationship from new_project to contact with a lookup field called new_ProjectId.

```powershell
Set-DataverseRelationshipMetadata -SchemaName "new_project_contact" `
   -RelationshipType "OneToMany" `
   -ReferencedEntity "new_project" `
   -ReferencingEntity "contact" `
   -LookupAttributeSchemaName "new_ProjectId" `
   -LookupAttributeDisplayName "Project" `
   -CascadeDelete "RemoveLink"

```
