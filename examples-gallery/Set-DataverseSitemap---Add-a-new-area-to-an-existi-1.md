---
title: "Set-DataverseSitemap - Add a new area to an existing sitemap"
tags: ['Metadata']
source: "Set-DataverseSitemap.md"
---
Demonstrates programmatic manipulation of sitemap XML to add a new navigation area.

```powershell
$sitemap = Get-DataverseSitemap -Name "MySitemap"
$xml = [xml]$sitemap.SitemapXml

# Create new area element
$newArea = $xml.CreateElement("Area")
$newArea.SetAttribute("Id", "NewArea")
$newArea.SetAttribute("ResourceId", "NewArea.Title")

# Add to sitemap
$xml.SiteMap.AppendChild($newArea)

# Update sitemap
Set-DataverseSitemap -Name "MySitemap" -Id $sitemap.Id -SitemapXml $xml.OuterXml

# Sitemap 'MySitemap' updated successfully.

```
