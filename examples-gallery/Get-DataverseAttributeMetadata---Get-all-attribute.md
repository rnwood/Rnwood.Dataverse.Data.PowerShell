---
title: "Get-DataverseAttributeMetadata - Get all attributes for an entity"
tags: ['Metadata']
source: "Get-DataverseAttributeMetadata.md"
---
Retrieves all attributes for the `account` entity.

```powershell
$attributes = Get-DataverseAttributeMetadata -EntityName account
$attributes.Count
# 150

$attributes | Select-Object -First 10 LogicalName, AttributeType

# LogicalName       AttributeType
# -----------      -------------
# accountid         Uniqueidentifier
# accountname       String
# accountnumber     String
# address1_city     String
# emailaddress1     String
# revenue           Money
# numberofemployees Integer
# createdon         DateTime

```
