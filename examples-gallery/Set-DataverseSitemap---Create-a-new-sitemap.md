---
title: "Set-DataverseSitemap - Create a new sitemap"
tags: ['Metadata']
source: "Set-DataverseSitemap.md"
---
Creates a new sitemap with the specified name and XML definition, returning the new sitemap ID.

```powershell
$sitemapXml = @"
# <SiteMap>
  <Area Id="Area1" ResourceId="Area1.Title">
    <Group Id="Group1" ResourceId="Group1.Title">
      <SubArea Id="SubArea1" ResourceId="SubArea1.Title" Entity="account" />
    </Group>
  </Area>
# </SiteMap>
# "@

Set-DataverseSitemap -Name "MySitemap" -SitemapXml $sitemapXml -PassThru

# a1b2c3d4-5678-90ab-cdef-1234567890ab

```
