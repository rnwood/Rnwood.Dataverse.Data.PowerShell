---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseFormatAddress

## SYNOPSIS
Contains the data to compute an address based on country and format parameters.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.FormatAddressRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.FormatAddressRequest)

## SYNTAX

```
Invoke-DataverseFormatAddress -Connection <ServiceClient> -Line1 <String> -City <String> -StateOrProvince <String> -PostalCode <String> -Country <String>
```

## DESCRIPTION
Contains the data to compute an address based on country and format parameters.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseFormatAddress -Connection <ServiceClient> -Line1 <String> -City <String> -StateOrProvince <String> -PostalCode <String> -Country <String>
```

## PARAMETERS

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

### -Line1
Gets or sets the Line1 for the request.

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

### -City
Gets or sets the City for the request.

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

### -StateOrProvince
Gets or sets the StateOrProvince for the request.

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

### -PostalCode
Gets or sets the PostalCode for the request.

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

### -Country
Specifies the Country for paging or sizing results.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.FormatAddressResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.FormatAddressResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.FormatAddressResponse)
## NOTES

## RELATED LINKS
