---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseDependentComponent

## SYNOPSIS
Retrieves components that depend on a specified component.

## SYNTAX

```
Get-DataverseDependentComponent [-ObjectId] <Guid> [-ComponentType] <Int32> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet retrieves all components that depend on a specified component using the RetrieveDependentComponentsRequest message.

This is useful for understanding the impact of changes to a component. For example, if you want to know what forms, views, or processes use a particular attribute, this cmdlet will show those dependencies.

The cmdlet returns a collection of dependency entities, each describing a component that depends on the specified component.

## EXAMPLES

### Example 1: Find components that depend on an attribute
```powershell
PS C:\> $attributeMetadata = Get-DataverseAttributeMetadata -Connection $c -EntityName "contact" -AttributeName "firstname"
PS C:\> $dependents = Get-DataverseDependentComponent -Connection $c -ObjectId $attributeMetadata.MetadataId -ComponentType 2
PS C:\> $dependents | Format-Table dependentcomponentobjectid, dependentcomponenttype
```

Retrieves all components (forms, views, etc.) that use the firstname attribute.

### Example 2: Find components that use an option set
```powershell
PS C:\> $optionSetMetadata = Get-DataverseOptionSetMetadata -Connection $c -Name "new_customchoice"
PS C:\> $dependents = Get-DataverseDependentComponent -ObjectId $optionSetMetadata.MetadataId -ComponentType 9
PS C:\> if ($dependents.Count -gt 0) {
>>     Write-Host "Option set is used by $($dependents.Count) components"
>> } else {
>>     Write-Host "Option set is not currently in use"
>> }
```

Checks if a custom option set is in use by any components.

### Example 3: Analyze entity usage
```powershell
PS C:\> $entityMetadata = Get-DataverseEntityMetadata -Connection $c -EntityName "account"
PS C:\> $dependents = Get-DataverseDependentComponent -ObjectId $entityMetadata.MetadataId -ComponentType 1
PS C:\> $dependents | Group-Object dependentcomponenttype | Select-Object Count, Name
```

Groups dependent components by type to understand how an entity is being used.

### Example 4: Find web resources that depend on an entity
```powershell
PS C:\> $entityMetadata = Get-DataverseEntityMetadata -Connection $c -EntityName "new_customentity"
PS C:\> $dependents = Get-DataverseDependentComponent -ObjectId $entityMetadata.MetadataId -ComponentType 1
PS C:\> $webResources = $dependents | Where-Object { $_.dependentcomponenttype -eq 29 }
PS C:\> $webResources | ForEach-Object {
>>     Write-Host "Web Resource ID: $($_.dependentcomponentobjectid)"
>> }
```

Finds all web resources (JavaScript, CSS, etc.) that reference a custom entity.

## PARAMETERS

### -ComponentType
Component type of the object to check. Common types:
- 1 = Entity (Table)
- 2 = Attribute (Column)
- 9 = Option Set (Choice)
- 10 = Relationship
- 24 = Form
- 26 = View
- 29 = Web Resource
- 60 = Chart
- 80 = Process (Workflow)

For a complete list, see [Microsoft's documentation](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/solution-component-file).

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.

If not provided, uses the default connection set via `Get-DataverseConnection -SetAsDefault`.

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ObjectId
Unique identifier (MetadataId) of the component to find dependents for.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases: ComponentId, MetadataId

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
You can pipe objects with ObjectId, ComponentId, or MetadataId properties to this cmdlet.

## OUTPUTS

### Microsoft.Xrm.Sdk.Entity
Returns dependency entities with information about each dependent component.

## NOTES
- This cmdlet uses the RetrieveDependentComponentsRequest SDK message.
- The returned entities contain properties like dependentcomponentobjectid, dependentcomponenttype, requiredcomponentobjectid, and requiredcomponenttype.
- An empty result means no other components depend on the specified component.

See https://learn.microsoft.com/en-us/power-apps/developer/data-platform/dependency-tracking-solution-components

## RELATED LINKS

[Get-DataverseDependency](Get-DataverseDependency.md)
[Get-DataverseMissingDependency](Get-DataverseMissingDependency.md)
[Get-DataverseUninstallDependency](Get-DataverseUninstallDependency.md)
