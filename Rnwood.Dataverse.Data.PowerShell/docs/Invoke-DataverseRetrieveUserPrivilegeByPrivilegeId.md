---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveUserPrivilegeByPrivilegeId

## SYNOPSIS
Contains the data to retrieve a list of privileges a system user (user) has through their roles, and inherited privileges from their team membership, based on the specified privilege ID.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegeByPrivilegeIdRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegeByPrivilegeIdRequest)

## SYNTAX

```
Invoke-DataverseRetrieveUserPrivilegeByPrivilegeId -Connection <ServiceClient> -UserId <Guid> -PrivilegeId <Guid>
```

## DESCRIPTION
Contains the data to retrieve a list of privileges a system user (user) has through their roles, and inherited privileges from their team membership, based on the specified privilege ID.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveUserPrivilegeByPrivilegeId -Connection <ServiceClient> -UserId <Guid> -PrivilegeId <Guid>
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

### -PrivilegeId
Gets or sets the ID of the privilege that is to be removed from the existing role.

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

### Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegeByPrivilegeIdResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegeByPrivilegeIdResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegeByPrivilegeIdResponse)
## NOTES

## RELATED LINKS
