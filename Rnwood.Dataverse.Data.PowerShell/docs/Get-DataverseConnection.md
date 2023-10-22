---
external help file: Rnwood.Dataverse.Data.PowerShell.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseConnection

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

### Authenticate with client secret
```
Get-DataverseConnection [-ClientId <Guid>] -Url <Uri> -ClientSecret <String> [<CommonParameters>]
```

### Authenticate interactively
```
Get-DataverseConnection [-ClientId <Guid>] -Url <Uri> [-Username <String>] [-Interactive] [<CommonParameters>]
```

### Authenticate using the device code flow
```
Get-DataverseConnection [-ClientId <Guid>] -Url <Uri> [-Username <String>] [-DeviceCode] [<CommonParameters>]
```

### Authenticate with username and password
```
Get-DataverseConnection [-ClientId <Guid>] -Url <Uri> -Username <String> -Password <String>
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

### -Url
{{ Fill Url Description }}

```yaml
Type: Uri
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Username
{{ Fill Username Description }}

```yaml
Type: String
Parameter Sets: Authenticate interactively, Authenticate using the device code flow
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: Authenticate with username and password
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClientId
{{ Fill ClientId Description }}

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClientSecret
{{ Fill ClientSecret Description }}

```yaml
Type: String
Parameter Sets: Authenticate with client secret
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DeviceCode
{{ Fill DeviceCode Description }}

```yaml
Type: SwitchParameter
Parameter Sets: Authenticate using the device code flow
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Interactive
{{ Fill Interactive Description }}

```yaml
Type: SwitchParameter
Parameter Sets: Authenticate interactively
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Password
{{ Fill Password Description }}

```yaml
Type: String
Parameter Sets: Authenticate with username and password
Aliases:

Required: True
Position: Named
Default value: None
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
