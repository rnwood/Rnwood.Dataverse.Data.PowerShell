---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRemoveAppComponents

## SYNOPSIS
Contains the data that is needed to remove a component from an app.For the Web API, use the RemoveAppComponents Action.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveAppComponentsRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RemoveAppComponentsRequest)

## SYNTAX

```
Invoke-DataverseRemoveAppComponents -Connection <ServiceClient> -AppId <Guid> -Components <EntityReferenceCollection>
```

## DESCRIPTION
Contains the data that is needed to remove a component from an app.For the Web API, use the RemoveAppComponents Action.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRemoveAppComponents -Connection <ServiceClient> -AppId <Guid> -Components <EntityReferenceCollection>
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

### -AppId
ID of the app from where you want to remove components.

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

### -Components
A collection of components to be removed.

```yaml
Type: EntityReferenceCollection
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

### Microsoft.Crm.Sdk.Messages.RemoveAppComponentsResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveAppComponentsResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RemoveAppComponentsResponse)
## NOTES

## RELATED LINKS
