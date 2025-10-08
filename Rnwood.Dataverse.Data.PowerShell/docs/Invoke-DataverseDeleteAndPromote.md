---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseDeleteAndPromote

## SYNOPSIS
Contains the data needed to replace a managed solution plus all of its patches.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeleteAndPromoteRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.DeleteAndPromoteRequest)

## SYNTAX

```
Invoke-DataverseDeleteAndPromote -Connection <ServiceClient> -UniqueName <String>
```

## DESCRIPTION
Contains the data needed to replace a managed solution plus all of its patches.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseDeleteAndPromote -Connection <ServiceClient> -UniqueName <String>
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

### -UniqueName
Gets or sets the unique name of the organization.

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

### Microsoft.Crm.Sdk.Messages.DeleteAndPromoteResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeleteAndPromoteResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.DeleteAndPromoteResponse)
## NOTES

## RELATED LINKS
