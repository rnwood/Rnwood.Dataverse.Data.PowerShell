---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveDependentComponents

## SYNOPSIS
Contains the data that is needed to retrieves a list dependencies for solution components that directly depend on a solution component.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveDependentComponentsRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveDependentComponentsRequest)

## SYNTAX

```
Invoke-DataverseRetrieveDependentComponents -Connection <ServiceClient> -ObjectId <Guid> -ComponentType <Int32>
```

## DESCRIPTION
Contains the data that is needed to retrieves a list dependencies for solution components that directly depend on a solution component.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveDependentComponents -Connection <ServiceClient> -ObjectId <Guid> -ComponentType <Int32>
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

### -ObjectId
Gets or sets the ID of the solution component that you want to check. Required.

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

### -ComponentType
Gets or sets the value that represents the solution component. Required.

```yaml
Type: Int32
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

### Microsoft.Crm.Sdk.Messages.RetrieveDependentComponentsResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveDependentComponentsResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveDependentComponentsResponse)
## NOTES

## RELATED LINKS
