---
title: "Get-DataverseRelationshipMetadata - Export relationship metadata to CSV"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Exports relationship metadata to CSV for documentation or analysis.

```powershell
$relationships = Get-DataverseRelationshipMetadata -EntityName account
$relationships | Select-Object SchemaName, RelationshipType, ReferencedEntity, ReferencingEntity,
    @{Name="CascadeDelete"; Expression={$_.CascadeDelete.Value}},
    @{Name="IsCustom"; Expression={$_.IsCustomRelationship}} | 
    Export-Csv -Path "account_relationships.csv" -NoTypeInformation

```
