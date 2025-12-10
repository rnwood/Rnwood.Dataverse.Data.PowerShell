---
title: "Set-DataverseSitemap - Load sitemap XML from file and create sitemap"
tags: ['Metadata']
source: "Set-DataverseSitemap.md"
---
Loads sitemap XML from a file and creates a new sitemap.

```powershell
$sitemapXml = Get-Content -Path "MySitemap.xml" -Raw
Set-DataverseSitemap -Name "ImportedSitemap" -SitemapXml $sitemapXml

# Sitemap 'ImportedSitemap' created successfully with ID: b2c3d4e5-6789-01bc-def2-345678901bcd

```
