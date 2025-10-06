---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseImportCardTypeSchema

## SYNOPSIS
Contains the data to import and create a new cardtype required by the installed solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportCardTypeSchemaRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ImportCardTypeSchemaRequest)

## SYNTAX

```
Invoke-DataverseImportCardTypeSchema -Connection <ServiceClient> -SolutionUniqueName <String>
```

## DESCRIPTION
Contains the data to import and create a new cardtype required by the installed solution.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseImportCardTypeSchema -Connection <ServiceClient> -SolutionUniqueName <String>
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

### Microsoft.Crm.Sdk.Messages.ImportCardTypeSchemaResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.ImportCardTypeSchemaResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.ImportCardTypeSchemaResponse)
## NOTES

## RELATED LINKS
