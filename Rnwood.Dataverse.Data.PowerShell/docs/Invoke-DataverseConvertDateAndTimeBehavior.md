---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseConvertDateAndTimeBehavior

## SYNOPSIS
Contains the data to convert existing UTC date and time values in the database to DateOnly values.

[Microsoft Learn: Microsoft.Xrm.Sdk.Messages.ConvertDateAndTimeBehaviorRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Xrm.Sdk.Messages.ConvertDateAndTimeBehaviorRequest)

## SYNTAX

```
Invoke-DataverseConvertDateAndTimeBehavior -Connection <ServiceClient>
 [-Attributes <EntityAttributeCollection>] [-ConversionRule <String>] [-TimeZoneCode <Int32>]
 [-AutoConvert <Boolean>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Contains the data to convert existing UTC date and time values in the database to DateOnly values.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseConvertDateAndTimeBehavior -Connection <ServiceClient> -Attributes <EntityAttributeCollection> -ConversionRule <String> -TimeZoneCode <Int32> -AutoConvert <Boolean>
```

## PARAMETERS

### -Attributes
Gets or sets a collection of entity and attributes to apply the behavior conversion on. Required

```yaml
Type: EntityAttributeCollection
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AutoConvert
Gets or sets whether to automatically convert to DateOnly value, if possible. Optional.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConversionRule
Gets or sets the conversion rule to apply. Required.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TimeZoneCode
Gets or sets the Dataverse time zone code to be used for the conversion. Required only if is specified in parameter.

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

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
