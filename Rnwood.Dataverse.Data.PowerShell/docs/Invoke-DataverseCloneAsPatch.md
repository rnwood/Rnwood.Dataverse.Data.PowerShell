---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCloneAsPatch

## SYNOPSIS
Contains the data that is needed to create a solution patch from a managed or unmanaged solution.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CloneAsPatchRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CloneAsPatchRequest)

## SYNTAX

```
Invoke-DataverseCloneAsPatch -Connection <ServiceClient> -ParentSolutionUniqueName <String> -DisplayName <String> -VersionNumber <String>
```

## DESCRIPTION
Contains the data that is needed to create a solution patch from a managed or unmanaged solution.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCloneAsPatch -Connection <ServiceClient> -ParentSolutionUniqueName <String> -DisplayName <String> -VersionNumber <String>
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

### -ParentSolutionUniqueName
Gets or sets the unique name of the parent solution.

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

### -DisplayName
Gets or sets the display name for the attribute.

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

### -VersionNumber
Specifies to display the string as a version number. Value = 6.

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

### Microsoft.Crm.Sdk.Messages.CloneAsPatchResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CloneAsPatchResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CloneAsPatchResponse)
## NOTES

## RELATED LINKS
