---
title: "Get-DataverseOptionSetMetadata - Get option set for an entity attribute"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Retrieves the options for the `gendercode` choice field on the `contact` entity.

```powershell
$optionSet = Get-DataverseOptionSetMetadata -EntityName contact -AttributeName gendercode
$optionSet.Options | Select-Object Value, Label

# Value Label
# ----- -----
# 1     Male
# 2     Female

```
