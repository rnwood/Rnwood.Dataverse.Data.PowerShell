---
title: "Set-DataverseSitemap - Clone a sitemap"
tags: ['Metadata']
source: "Set-DataverseSitemap.md"
---
Creates a copy of an existing sitemap with a different name.

```powershell
$source = Get-DataverseSitemap -Name "OriginalSitemap"
Set-DataverseSitemap -Name "ClonedSitemap" -SitemapXml $source.SitemapXml -PassThru

# c3d4e5f6-7890-12cd-ef34-56789012cdef

```
