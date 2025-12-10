---
title: "Get-DataverseOptionSetMetadata"
tags: ['CRUD', 'Metadata']
source: "Metadata-CRUD-Examples.md"
---
Retrieves option set values for choice fields.

```powershell
# Get option set for an entity attribute
Get-DataverseOptionSetMetadata -EntityName contact -AttributeName gendercode

# Get a global option set by name
Get-DataverseOptionSetMetadata -Name my_globaloptions

# List all global option sets
Get-DataverseOptionSetMetadata

# Get options and display them
$optionSet = Get-DataverseOptionSetMetadata -EntityName contact -AttributeName preferredcontactmethodcode
$optionSet.Options | Format-Table Value, Label

```
