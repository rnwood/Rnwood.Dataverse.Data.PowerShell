---
title: "Get-DataverseAttributeMetadata - Query only published metadata"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Demonstrates retrieving only published metadata vs unpublished (draft) metadata. By default, the cmdlet retrieves unpublished metadata which may include attributes that have been created or modified but not yet published.

```powershell
# Get only published attribute metadata
$publishedAttrs = Get-DataverseAttributeMetadata -EntityName account -Published
$publishedAttrs.Count
# 145

# Default behavior includes unpublished (draft) attributes
$unpublishedAttrs = Get-DataverseAttributeMetadata -EntityName account
$unpublishedAttrs.Count
# 150

```
