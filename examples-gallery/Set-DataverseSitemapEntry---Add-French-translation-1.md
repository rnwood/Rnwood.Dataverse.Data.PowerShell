---
title: "Set-DataverseSitemapEntry - Add French translation to existing entry"
tags: ['Metadata']
source: "Set-DataverseSitemapEntry.md"
---
Adds a French title to an existing Area entry (additively - English title is preserved).

```powershell
$frenchTitles = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
$frenchTitles.Add(1036, "Ventes et Marketing")
Set-DataverseSitemapEntry -SitemapUniqueName "MySitemap" -Area -EntryId "SalesArea" -Titles $frenchTitles

```
