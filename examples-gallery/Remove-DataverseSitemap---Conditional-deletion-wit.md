---
title: "Remove-DataverseSitemap - Conditional deletion with confirmation"
tags: ['Metadata']
source: "Remove-DataverseSitemap.md"
---
Deletes a sitemap if it hasn't been modified in the last 6 months, bypassing confirmation.

```powershell
$sitemap = Get-DataverseSitemap -Name "MySitemap"
if ($sitemap.ModifiedOn -lt (Get-Date).AddMonths(-6)) {
# >>     Remove-DataverseSitemap -Id $sitemap.Id -Confirm:$false
# >> }

```
