---
title: "Get-DataverseEntityMetadata - Pipeline entity name to get metadata"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Pipelines multiple entity names to retrieve their metadata.

```powershell
@("account", "contact", "lead") | Get-DataverseEntityMetadata | 
    Select-Object LogicalName, DisplayName, PrimaryNameAttribute

# LogicalName DisplayName PrimaryNameAttribute
# ----------- ----------- --------------------
# account     Account     name
# contact     Contact     fullname
# lead        Lead        fullname

```
