---
title: "Remove-DataverseAttributeMetadata - Delete an attribute with confirmation"
tags: ['Metadata']
source: "Remove-DataverseAttributeMetadata.md"
---
Deletes the attribute `new_customfield` from the `account` entity after prompting for confirmation.

```powershell
Remove-DataverseAttributeMetadata -EntityName account -AttributeName new_customfield

# Confirm
# Are you sure you want to perform this action?
# Performing the operation "Delete attribute 'new_customfield'" on target "Entity 'account'".
# [Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"): Y

```
