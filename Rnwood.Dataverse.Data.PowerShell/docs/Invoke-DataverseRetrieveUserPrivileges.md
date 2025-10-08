---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveUserPrivileges

## SYNOPSIS
Contains the data to retrieve the privileges a system user (user) has through their roles, and inherited privileges from their team membership.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegesRequest)

## SYNTAX

```
Invoke-DataverseRetrieveUserPrivileges -Connection <ServiceClient> -UserId <Guid>
```

## DESCRIPTION
Contains the data to retrieve the privileges a system user (user) has through their roles, and inherited privileges from their team membership.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveUserPrivileges -Connection <ServiceClient> -UserId <Guid>
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

### -UserId
Gets or sets the Microsoft Dynamics 365 system user ID of the client.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegesResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegesResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegesResponse)
## NOTES

## RELATED LINKS
