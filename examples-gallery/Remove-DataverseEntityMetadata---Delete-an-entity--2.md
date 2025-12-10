---
title: "Remove-DataverseEntityMetadata - Delete an entity with confirmation"
tags: ['Metadata']
source: "Remove-DataverseEntityMetadata.md"
---
Deletes the entity `new_customentity` after prompting for confirmation.

```powershell
Remove-DataverseEntityMetadata -EntityName new_customentity

# Confirm
# Are you sure you want to perform this action?
# Performing the operation "Delete entity" on target "new_customentity".
# [Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"): Y

```
