---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseDependency

## SYNOPSIS
Retrieves dependencies that would prevent a component from being deleted.

## SYNTAX

```
Get-DataverseDependency [-ObjectId] <Guid> [-ComponentType] <Int32> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet retrieves dependencies that would prevent a component from being deleted from Dataverse using the RetrieveDependenciesForDeleteRequest message.

When you attempt to delete a component (such as an entity, attribute, form, or view), Dataverse checks whether other components depend on it. This cmdlet returns information about those dependencies so you can address them before attempting deletion.

The cmdlet returns a collection of dependency entities, each describing a dependency relationship between components.

## EXAMPLES

### Example 1: Check dependencies before deleting an entity
```powershell
PS C:\> $entityMetadata = Get-DataverseEntityMetadata -Connection $c -EntityName "new_customtable"
PS C:\> $dependencies = Get-DataverseDependency -Connection $c -ObjectId $entityMetadata.MetadataId -ComponentType 1
PS C:\> if ($dependencies) {
>>     Write-Host "Cannot delete entity - found $($dependencies.Count) dependencies"
>>     $dependencies | Format-Table dependentcomponentobjectid, dependentcomponenttype
>> } else {
>>     Write-Host "Entity can be safely deleted"
>> }
```

Checks if a custom entity has any dependencies before attempting to delete it.

### Example 2: Check attribute dependencies
```powershell
PS C:\> $attributeMetadata = Get-DataverseAttributeMetadata -Connection $c -EntityName "contact" -AttributeName "new_customfield"
PS C:\> $dependencies = Get-DataverseDependency -ObjectId $attributeMetadata.MetadataId -ComponentType 2
PS C:\> $dependencies | ForEach-Object {
>>     Write-Host "Dependent component: Type $($_.dependentcomponenttype), ID $($_.dependentcomponentobjectid)"
>> }
```

Lists all components that depend on a custom attribute.

### Example 3: Pipeline processing for multiple components
```powershell
PS C:\> $attributes = Get-DataverseAttributeMetadata -Connection $c -EntityName "account" | Where-Object { $_.IsCustomAttribute }
PS C:\> $attributes | ForEach-Object {
>>     $deps = Get-DataverseDependency -ObjectId $_.MetadataId -ComponentType 2
>>     if ($deps) {
>>         [PSCustomObject]@{
>>             AttributeName = $_.LogicalName
>>             DependencyCount = $deps.Count
>>         }
>>     }
>> } | Format-Table
```

Reports dependency counts for all custom attributes on the account entity.

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
Unique identifier (MetadataId) of the component to check for dependencies.

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
Returns dependency entities with information about each dependency relationship.

## NOTES
- This cmdlet uses the RetrieveDependenciesForDeleteRequest SDK message.
- The returned entities contain properties like dependentcomponentobjectid, dependentcomponenttype, requiredcomponentobjectid, and requiredcomponenttype.
- Empty result means the component can be deleted without dependency issues.

See https://learn.microsoft.com/en-us/power-apps/developer/data-platform/dependency-tracking-solution-components

## RELATED LINKS

[Get-DataverseDependentComponent](Get-DataverseDependentComponent.md)
[Get-DataverseMissingDependency](Get-DataverseMissingDependency.md)
[Get-DataverseUninstallDependency](Get-DataverseUninstallDependency.md)
