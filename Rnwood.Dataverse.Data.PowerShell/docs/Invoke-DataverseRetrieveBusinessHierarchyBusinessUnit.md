---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveBusinessHierarchyBusinessUnit

## SYNOPSIS
Contains the data that is needed to retrieve all business units (including the specified business unit) from the business unit hierarchy.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveBusinessHierarchyBusinessUnitRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveBusinessHierarchyBusinessUnitRequest)

## SYNTAX

```
Invoke-DataverseRetrieveBusinessHierarchyBusinessUnit -Connection <ServiceClient> -EntityId <Guid> -ColumnSet <ColumnSet>
```

## DESCRIPTION
Contains the data that is needed to retrieve all business units (including the specified business unit) from the business unit hierarchy.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveBusinessHierarchyBusinessUnit -Connection <ServiceClient> -EntityId <Guid> -ColumnSet <ColumnSet>
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

### -EntityId
Gets the of the newly created table.

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

### Microsoft.Crm.Sdk.Messages.RetrieveBusinessHierarchyBusinessUnitResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveBusinessHierarchyBusinessUnitResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveBusinessHierarchyBusinessUnitResponse)
## NOTES

## RELATED LINKS
