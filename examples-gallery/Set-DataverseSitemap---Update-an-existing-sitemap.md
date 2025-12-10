---
title: "Set-DataverseSitemap - Update an existing sitemap"
tags: ['Metadata']
source: "Set-DataverseSitemap.md"
---
Retrieves an existing sitemap, modifies its XML, and updates it.

```powershell
$sitemap = Get-DataverseSitemap -Name "MySitemap"
$updatedXml = $sitemap.SitemapXml -replace 'Area1', 'UpdatedArea1'
Set-DataverseSitemap -Name "MySitemap" -Id $sitemap.Id -SitemapXml $updatedXml

# Sitemap 'MySitemap' updated successfully.

```
