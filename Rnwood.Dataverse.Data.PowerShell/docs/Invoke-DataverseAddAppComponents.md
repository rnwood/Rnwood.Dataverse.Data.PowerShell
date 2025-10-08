---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseAddAppComponents

## SYNOPSIS
Contains the data that is needed to add app components to a business app.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddAppComponentsRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.AddAppComponentsRequest)

## SYNTAX

```
Invoke-DataverseAddAppComponents -Connection <ServiceClient> -AppId <Guid> -Components <EntityReferenceCollection>
```

## DESCRIPTION
Contains the data that is needed to add app components to a business app.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseAddAppComponents -Connection <ServiceClient> -AppId <Guid> -Components <EntityReferenceCollection>
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
Gets or sets the ID of the business app to add the components to.

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
Gets or sets a collection of components, such as sitemap, entity, dashboard, business process flows, views, and forms, to be added to the business app.

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

### Microsoft.Crm.Sdk.Messages.AddAppComponentsResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddAppComponentsResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.AddAppComponentsResponse)
## NOTES

## RELATED LINKS
