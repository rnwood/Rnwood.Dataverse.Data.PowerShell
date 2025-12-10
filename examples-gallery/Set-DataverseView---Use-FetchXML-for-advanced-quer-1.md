---
title: "Set-DataverseView - Use FetchXML for advanced queries"
tags: ['Metadata']
source: "Set-DataverseView.md"
---
Creates a view using FetchXML for complex query definitions with date-based filters and sorting.

```powershell
$fetchXml = @"
# <fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <attribute name="emailaddress1" />
    <filter type="and">
    <condition attribute="statecode" operator="eq" value="0" />
      <condition attribute="createdon" operator="last-x-days" value="30" />
    </filter>
    <order attribute="createdon" descending="true" />
  </entity>
# </fetch>
# "@

Set-DataverseView -PassThru `
   -Name "Recent Contacts" `
   -TableName contact `
 -FetchXml $fetchXml

```

