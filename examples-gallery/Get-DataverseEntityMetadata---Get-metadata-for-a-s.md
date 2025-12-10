---
title: "Get-DataverseEntityMetadata - Get metadata for a specific entity"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Retrieves basic metadata for the `contact` entity.

```powershell
$metadata = Get-DataverseEntityMetadata -EntityName contact
$metadata

# LogicalName          : contact
# SchemaName           : Contact
# DisplayName          : Contact
# DisplayCollectionName: Contacts
# PrimaryIdAttribute   : contactid
# PrimaryNameAttribute : fullname
# OwnershipType        : UserOwned
# IsCustomEntity       : False

```
