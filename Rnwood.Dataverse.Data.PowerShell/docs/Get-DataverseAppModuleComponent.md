---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseAppModuleComponent

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

```
Get-DataverseAppModuleComponent [[-Id] <Guid>] [-AppModuleIdValue <Guid>] [-ObjectId <Guid>]
 [-ComponentType <Int32>] [-Raw] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```powershell
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -AppModuleIdValue
Filter components by app module ID.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases: AppModuleId

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ComponentType
Filter components by component type (1=Entity, 29=Business Process Flow, 60=Chart, 62=Sitemap, etc.)

```yaml
Type: Int32
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

Get-DataverseAppModuleComponent retrieves components that belong to a model-driven app. Components include entities, dashboards, business process flows, and other elements.

Typical use: Get-DataverseAppModuleComponent -Connection $c -AppModuleIdValue $appId

## RELATED LINKS

[Set-DataverseAppModuleComponent](Set-DataverseAppModuleComponent.md)

[Remove-DataverseAppModuleComponent](Remove-DataverseAppModuleComponent.md)

[Get-DataverseAppModule](Get-DataverseAppModule.md)
