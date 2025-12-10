---
title: "Get-DataverseEntityMetadata - Access icon properties from entity metadata"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Retrieves and displays icon properties for an entity. Icon properties specify the visual representation of the entity in the UI.

```powershell
$metadata = Get-DataverseEntityMetadata -EntityName account
$metadata | Select-Object LogicalName, IconVectorName, IconLargeName, IconMediumName, IconSmallName

# LogicalName IconVectorName    IconLargeName      IconMediumName     IconSmallName
# ----------- --------------   -------------     --------------    -------------
# account     svg_account       Entity/account.png Entity/account.png Entity/account.png

```
