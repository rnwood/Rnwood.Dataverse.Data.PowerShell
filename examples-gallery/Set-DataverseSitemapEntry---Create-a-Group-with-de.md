---
title: "Set-DataverseSitemapEntry - Create a Group with descriptions"
tags: ['Metadata']
source: "Set-DataverseSitemapEntry.md"
---
Creates a new Group at the first position (index 0) within the SalesArea with titles and descriptions.

```powershell
$titles = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
$titles.Add(1033, "Leads")
$descriptions = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
$descriptions.Add(1033, "Manage potential customers")
Set-DataverseSitemapEntry -SitemapUniqueName "MySitemap" -Group -EntryId "LeadsGroup" -ParentAreaId "SalesArea" -Titles $titles -Descriptions $descriptions -Index 0

```
