---
title: "Set-DataverseSitemapEntry - Create a new Area with multilingual titles"
tags: ['Metadata']
source: "Set-DataverseSitemapEntry.md"
---
Creates a new Area entry with titles in English and French.

```powershell
$titles = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
$titles.Add(1033, "Sales")
$titles.Add(1036, "Ventes")
Set-DataverseSitemapEntry -SitemapUniqueName "MySitemap" -Area -EntryId "SalesArea" -Titles $titles -Icon "/_imgs/area/sales_24x24.gif"

```
