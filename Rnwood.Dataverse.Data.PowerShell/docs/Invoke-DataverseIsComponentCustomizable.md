---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseIsComponentCustomizable

## SYNOPSIS
Contains the data that is needed to determine whether a solution component is customizable.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.IsComponentCustomizableRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.IsComponentCustomizableRequest)

## SYNTAX

```
Invoke-DataverseIsComponentCustomizable -Connection <ServiceClient> -ComponentId <Guid> -ComponentType <Int32>
```

## DESCRIPTION
Contains the data that is needed to determine whether a solution component is customizable.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseIsComponentCustomizable -Connection <ServiceClient> -ComponentId <Guid> -ComponentType <Int32>
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

### -ComponentId
Gets or sets the ID of the solution component. Required.

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

### -ComponentType
Gets or sets the value that represents the solution component that you are adding. Required.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.IsComponentCustomizableResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.IsComponentCustomizableResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.IsComponentCustomizableResponse)
## NOTES

## RELATED LINKS
