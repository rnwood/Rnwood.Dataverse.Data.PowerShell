---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseAppModuleComponent

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

```
Set-DataverseAppModuleComponent [-Id <Guid>] [-AppModuleIdValue <Guid>] [-ObjectId <Guid>]
 [-ComponentType <Int32>] [-RootComponentBehavior <Int32>] [-IsDefault <Boolean>] [-IsMetadata <Boolean>]
 [-NoUpdate] [-NoCreate] [-PassThru] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
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
App module ID that this component belongs to.
Required when creating a new component.

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
Component type (1=Entity, 29=Business Process Flow, 60=Chart, 62=Sitemap, etc.).
Required when creating a new component.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

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
ID of the app module component to update.
If not specified or if the component doesn't exist, a new component is created.

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

### -IsDefault
Whether this is the default component

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IsMetadata
Whether this is a metadata component

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -NoCreate
If specified, then no component will be created even if no existing component matching the ID is found

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

### -NoUpdate
If specified, existing components matching the ID will not be updated

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

### -ObjectId
Object ID (the ID of the component entity record).
Required when creating a new component.

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

### -PassThru
If specified, returns the ID of the created or updated component

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

### -RootComponentBehavior
Root component behavior (0=IncludeSubcomponents, 1=DoNotIncludeSubcomponents, 2=IncludeAsShellOnly)

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

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
### System.Nullable`1[[System.Int32, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
### System.Nullable`1[[System.Boolean, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### System.Guid
## NOTES

Set-DataverseAppModuleComponent adds or updates components in a model-driven app.

Required for creation: AppModuleIdValue, ObjectId, ComponentType
ComponentType values: 1=Entity, 29=Business Process Flow, 60=Chart, 62=Sitemap, etc.

## RELATED LINKS

[Get-DataverseAppModuleComponent](Get-DataverseAppModuleComponent.md)

[Remove-DataverseAppModuleComponent](Remove-DataverseAppModuleComponent.md)

[Set-DataverseAppModule](Set-DataverseAppModule.md)
