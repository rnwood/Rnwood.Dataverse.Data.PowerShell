---
title: "Get-DataverseOptionSetMetadata - Query only published metadata"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Demonstrates retrieving only published option set metadata vs unpublished (draft) metadata. By default, the cmdlet retrieves unpublished metadata which may include option sets that have been modified but not yet published.

```powershell
# Get only published option sets
$publishedOptionSets = Get-DataverseOptionSetMetadata -Published
$publishedOptionSets.Count
# 52

# Default behavior includes unpublished (draft) option sets
$unpublishedOptionSets = Get-DataverseOptionSetMetadata
$unpublishedOptionSets.Count
# 55

```
