---
title: "Clear-DataverseMetadataCache - Clear cache for multiple connections"
tags: ['System']
source: "Clear-DataverseMetadataCache.md"
---
Clears cache for multiple connections.

```powershell
$dev = Get-DataverseConnection -Url "https://dev.crm.dynamics.com" -Interactive
$test = Get-DataverseConnection -Url "https://test.crm.dynamics.com" -Interactive

# Clear cache for dev
Clear-DataverseMetadataCache -Connection $dev

# Clear cache for test
Clear-DataverseMetadataCache -Connection $test 

# Or clear all at once
Clear-DataverseMetadataCache

```
