---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseAddPrincipalToQueue

## SYNOPSIS
Contains the data to add the specified principal to the list of queue members. If the principal is a team, add each team member to the queue.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddPrincipalToQueueRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.AddPrincipalToQueueRequest)

## SYNTAX

```
Invoke-DataverseAddPrincipalToQueue -Connection <ServiceClient> -QueueId <Guid> -Principal <PSObject> -PrincipalTableName <String> -PrincipalIgnoreProperties <String[]> -PrincipalLookupColumns <Hashtable>
```

## DESCRIPTION
Contains the data to add the specified principal to the list of queue members. If the principal is a team, add each team member to the queue.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseAddPrincipalToQueue -Connection <ServiceClient> -QueueId <Guid> -Principal <PSObject> -PrincipalTableName <String> -PrincipalIgnoreProperties <String[]> -PrincipalLookupColumns <Hashtable>
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

### -QueueId
Gets or sets the ID of the queue. Required

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

### -Principal
Gets or sets the principal to add to the queue. Required. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrincipalTableName
Gets or sets the principal to add to the queue. Required. The logical name of the table/entity type for the Principal parameter.

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

### -PrincipalIgnoreProperties
Gets or sets the principal to add to the queue. Required. Properties to ignore when converting Principal PSObject to Entity.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrincipalLookupColumns
Gets or sets the principal to add to the queue. Required. Hashtable specifying lookup columns for entity reference conversions in Principal.

```yaml
Type: Hashtable
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

### Microsoft.Crm.Sdk.Messages.AddPrincipalToQueueResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddPrincipalToQueueResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.AddPrincipalToQueueResponse)
## NOTES

## RELATED LINKS
