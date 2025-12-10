---
title: "Get-DataverseSitemap - Export sitemap XML to file"
tags: ['Metadata']
source: "Get-DataverseSitemap.md"
---
Retrieves a sitemap and exports its XML definition to a file.

```powershell
$sitemap = Get-DataverseSitemap -Name "MySitemap"
$sitemap.SitemapXml | Out-File -FilePath "MySitemap.xml"

```
