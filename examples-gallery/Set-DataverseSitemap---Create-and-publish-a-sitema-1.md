---
title: "Set-DataverseSitemap - Create and publish a sitemap"
tags: ['Metadata']
source: "Set-DataverseSitemap.md"
---
Creates a new sitemap and immediately publishes it so it's available in model-driven apps.

```powershell
$sitemapXml = @"
# <SiteMap>
  <Area Id="SalesArea" ResourceId="SalesArea.Title">
    <Group Id="SalesGroup" ResourceId="SalesGroup.Title">
      <SubArea Id="Accounts" ResourceId="Accounts.Title" Entity="account" />
      <SubArea Id="Contacts" ResourceId="Contacts.Title" Entity="contact" />
    </Group>
  </Area>
# </SiteMap>
# "@

Set-DataverseSitemap -Name "SalesSitemap" -SitemapXml $sitemapXml -Publish

# Sitemap 'SalesSitemap' created successfully with ID: d4e5f6g7-8901-23de-f456-789012d3ef45
# Published sitemap with ID: d4e5f6g7-8901-23de-f456-789012d3ef45

```
