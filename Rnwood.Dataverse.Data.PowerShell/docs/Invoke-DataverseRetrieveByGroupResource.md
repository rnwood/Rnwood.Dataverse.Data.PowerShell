---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveByGroupResource

## SYNOPSIS
Contains the data that is needed to retrieve all resources that are related to the specified resource group (scheduling group).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveByGroupResourceRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveByGroupResourceRequest)

## SYNTAX

```
Invoke-DataverseRetrieveByGroupResource -Connection <ServiceClient> -ResourceGroupId <Guid> -Query <QueryBase>
```

## DESCRIPTION
Contains the data that is needed to retrieve all resources that are related to the specified resource group (scheduling group).

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveByGroupResource -Connection <ServiceClient> -ResourceGroupId <Guid> -Query <QueryBase>
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

### -ResourceGroupId
Gets or sets the ID of the resource group.

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

### -Query
Gets or sets the query representing the metadata to return.

```yaml
Type: QueryBase
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

### Microsoft.Crm.Sdk.Messages.RetrieveByGroupResourceResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveByGroupResourceResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveByGroupResourceResponse)
## NOTES

## RELATED LINKS
