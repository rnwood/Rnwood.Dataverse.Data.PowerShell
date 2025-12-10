---
title: "Remove-DataverseSitemap - Remove sitemap only if it exists"
tags: ['Metadata']
source: "Remove-DataverseSitemap.md"
---
Attempts to delete a sitemap but doesn't raise an error if it doesn't exist.

```powershell
Remove-DataverseSitemap -Name "OptionalSitemap" -IfExists

# Sitemap 'OptionalSitemap' not found. Skipping deletion.

```
