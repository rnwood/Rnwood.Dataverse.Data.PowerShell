---
title: "Remove-DataverseSitemap - Remove multiple sitemaps by piping"
tags: ['Metadata']
source: "Remove-DataverseSitemap.md"
---
Retrieves all unmanaged sitemaps with names starting with "Test" and deletes them.

```powershell
Get-DataverseSitemap -Unmanaged | Where-Object { $_.Name -like "Test*" } | Remove-DataverseSitemap

# Sitemap 'TestSitemap1' deleted successfully.
# Sitemap 'TestSitemap2' deleted successfully.
# Sitemap 'TestSitemap3' deleted successfully.

```
