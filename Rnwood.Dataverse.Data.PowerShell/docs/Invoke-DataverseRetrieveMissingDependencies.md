---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveMissingDependencies

## SYNOPSIS
Contains the data that is needed to retrieve any required solution components that are not included in the solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveMissingDependenciesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveMissingDependenciesRequest)

## SYNTAX

```
Invoke-DataverseRetrieveMissingDependencies -Connection <ServiceClient> -SolutionUniqueName <String>
```

## DESCRIPTION
Contains the data that is needed to retrieve any required solution components that are not included in the solution.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveMissingDependencies -Connection <ServiceClient> -SolutionUniqueName <String>
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

### -SolutionUniqueName
Gets or sets the name of the unmanaged solution to which you want to add this column. Optional.

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

### Microsoft.Crm.Sdk.Messages.RetrieveMissingDependenciesResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveMissingDependenciesResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveMissingDependenciesResponse)
## NOTES

## RELATED LINKS
