---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveMissingComponents

## SYNOPSIS
Contains the data that is needed to retrieve a list of missing components in the target organization.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveMissingComponentsRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveMissingComponentsRequest)

## SYNTAX

```
Invoke-DataverseRetrieveMissingComponents -Connection <ServiceClient> -CustomizationFile <Byte[]> -InFile <String>
```

## DESCRIPTION
Contains the data that is needed to retrieve a list of missing components in the target organization.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveMissingComponents -Connection <ServiceClient> -CustomizationFile <Byte[]> -InFile <String>
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

### -CustomizationFile
Gets or sets a file for a solution. Required.

```yaml
Type: Byte[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InFile
Gets or sets the path to a file containing the data to upload.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
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

### Microsoft.Crm.Sdk.Messages.RetrieveMissingComponentsResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveMissingComponentsResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveMissingComponentsResponse)
## NOTES

## RELATED LINKS
