---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseAppModuleComponent

## SYNOPSIS
Retrieves app module component information from a Dataverse environment.

## SYNTAX

```
Get-DataverseAppModuleComponent [[-Id] <Guid>] [-AppModuleId <Guid>] [-ObjectId <Guid>]
 [-ComponentType <AppModuleComponentType>] [-Raw] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseAppModuleComponent cmdlet retrieves components that belong to a model-driven app. Components define what entities, dashboards, business process flows, and other elements are included in an app.

By default, the cmdlet returns parsed component information with key properties. Use the -Raw parameter to return all attribute values from the appmodulecomponent record.

Components can be filtered by ID, app module ID, object ID, or component type.

## EXAMPLES

### Example 1: Get all components for an app module
```powershell
PS C:\> $app = Get-DataverseAppModule -Connection $c -UniqueName "myapp"
PS C:\> Get-DataverseAppModuleComponent -Connection $c -AppModuleIdValue $app.Id
```

Retrieves all components associated with a specific app module.

### Example 2: Get a specific component by ID
```powershell
PS C:\> Get-DataverseAppModuleComponent -Connection $c -Id "12345678-1234-1234-1234-123456789012"
```

Retrieves a specific component by its ID.

### Example 3: Get components by component type
```powershell
PS C:\> Get-DataverseAppModuleComponent -Connection $c -AppModuleIdValue $appId -ComponentType 1
```

Retrieves all entity components (type 1) for an app module.

### Example 4: Get components for a specific entity
```powershell
PS C:\> $entityId = [Guid]::Parse("...")
PS C:\> Get-DataverseAppModuleComponent -Connection $c -ObjectId $entityId
```

Finds which app modules include a specific entity.

## PARAMETERS

### -ComponentType
Filter components by component type (1=Entity, 29=Business Process Flow, 60=Chart, 62=Sitemap, etc.)

```yaml
Type: AppModuleComponentType
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g.
http://server.com/MyOrg/).
If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -Id
The ID of the app module component to retrieve.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ObjectId
Filter components by object ID (the ID of the component entity record).

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Raw
Return raw values instead of display values

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
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

### -AppModuleId
Filter components by app module ID.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

**Component Types:**
- 1 = Entity
- 29 = Business Process Flow
- 60 = Chart
- 62 = Sitemap
- 80 = Dashboard

**Filtering:**
- Filter by AppModuleIdValue to get all components for an app
- Filter by ObjectId to find which apps use a specific component
- Filter by ComponentType to get components of a specific type
- Combine filters for more precise queries

**Common Use Cases:**
- Auditing what components are included in an app
- Finding which apps use a specific entity or dashboard
- Validating app configuration
- Cloning app structures across environments

## RELATED LINKS

[Set-DataverseAppModuleComponent](Set-DataverseAppModuleComponent.md)

[Remove-DataverseAppModuleComponent](Remove-DataverseAppModuleComponent.md)

[Get-DataverseAppModule](Get-DataverseAppModule.md)