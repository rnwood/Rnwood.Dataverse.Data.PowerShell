---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveAadUserRoles

## SYNOPSIS
Executes a RetrieveAadUserRolesRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAadUserRolesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveAadUserRolesRequest)

## SYNTAX

```
Invoke-DataverseRetrieveAadUserRoles -Connection <ServiceClient> -DirectoryObjectId <Guid> -ColumnSet <ColumnSet>
```

## DESCRIPTION
Executes a RetrieveAadUserRolesRequest against the Dataverse organization service.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveAadUserRoles -Connection <ServiceClient> -DirectoryObjectId <Guid> -ColumnSet <ColumnSet>
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

### -DirectoryObjectId
Gets or sets the ID of the Directory Object.

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

### -ColumnSet
Gets or sets the collection of columns for which non-null values are returned from a query. Required.

```yaml
Type: ColumnSet
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

### Microsoft.Crm.Sdk.Messages.RetrieveAadUserRolesResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAadUserRolesResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveAadUserRolesResponse)
## NOTES

## RELATED LINKS
