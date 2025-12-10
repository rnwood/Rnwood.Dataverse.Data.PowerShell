---
title: "Remove-DataverseRelationshipMetadata - Delete a OneToMany relationship"
tags: ['Metadata']
source: "Remove-DataverseRelationshipMetadata.md"
---
Deletes the OneToMany relationship `new_project_contact` after prompting for confirmation.

```powershell
Remove-DataverseRelationshipMetadata -SchemaName new_project_contact

# Confirm
# Are you sure you want to perform this action?
# Performing the operation "Delete relationship 'new_project_contact'" on target "Dataverse organization".
# [Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"): Y

```
