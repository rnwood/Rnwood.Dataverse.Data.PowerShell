---
title: "Remove-DataverseSitemapEntry - Remove an entry with -IfExists for idempotent scripts"
tags: ['Metadata']
source: "Remove-DataverseSitemapEntry.md"
---
Removes the Group with ID "Marketing" if it exists, without raising an error if it doesn't exist. Useful in deployment scripts that need to be idempotent.

```powershell
Remove-DataverseSitemapEntry -SitemapName "Default" -EntryType Group -EntryId "Marketing" -IfExists

```
