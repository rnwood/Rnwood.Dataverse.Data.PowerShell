---
title: "Set-DataverseSitemapEntry - Remove a language translation"
tags: ['Metadata']
source: "Set-DataverseSitemapEntry.md"
---
Removes the French translation from an Area entry (setting a language to $null removes it).

```powershell
$removeFrench = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
$removeFrench.Add(1036, $null)
Set-DataverseSitemapEntry -SitemapUniqueName "MySitemap" -Area -EntryId "SalesArea" -Titles $removeFrench

```
