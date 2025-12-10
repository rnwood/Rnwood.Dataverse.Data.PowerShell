---
title: "Get-DataverseEntityMetadata - Query only published metadata"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Demonstrates the difference between querying published vs unpublished (draft) metadata. By default, the cmdlet retrieves unpublished metadata which includes all changes. Use the -Published switch to retrieve only published metadata.

```powershell
# Get only published metadata (excludes unpublished changes)
$publishedMetadata = Get-DataverseEntityMetadata -EntityName account -Published
$publishedMetadata.LogicalName
# account

# Default behavior retrieves unpublished metadata (includes draft changes)
$unpublishedMetadata = Get-DataverseEntityMetadata -EntityName account

```
