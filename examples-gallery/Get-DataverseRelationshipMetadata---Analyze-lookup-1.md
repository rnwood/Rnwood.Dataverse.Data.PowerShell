---
title: "Get-DataverseRelationshipMetadata - Analyze lookup attributes from relationships"
tags: ['Metadata']
source: "Get-DataverseRelationshipMetadata.md"
---
Analyzes lookup attributes created by OneToMany relationships.

```powershell
$oneToMany = Get-DataverseRelationshipMetadata -RelationshipType OneToMany -EntityName account
foreach ($rel in $oneToMany) {
    # Get the lookup attribute metadata
    $lookupAttr = Get-DataverseAttributeMetadata -EntityName $rel.ReferencingEntity -AttributeName $rel.ReferencingAttribute
    
    [PSCustomObject]@{
        Relationship = $rel.SchemaName
        LookupAttribute = $lookupAttr.LogicalName
        DisplayName = $lookupAttr.DisplayName.UserLocalizedLabel.Label
        RequiredLevel = $lookupAttr.RequiredLevel.Value
        IsSearchable = $lookupAttr.IsValidForAdvancedFind.Value
    }
}

```
