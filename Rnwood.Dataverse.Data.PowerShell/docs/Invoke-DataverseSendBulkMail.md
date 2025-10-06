---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseSendBulkMail

## SYNOPSIS
Contains the data that is needed to send bulk email messages.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SendBulkMailRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SendBulkMailRequest)

## SYNTAX

```
Invoke-DataverseSendBulkMail -Connection <ServiceClient> -Sender <PSObject> -TemplateId <Guid> -RegardingType <String> -RegardingId <Guid> -Query <QueryBase>
```

## DESCRIPTION
Contains the data that is needed to send bulk email messages.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseSendBulkMail -Connection <ServiceClient> -Sender <PSObject> -TemplateId <Guid> -RegardingType <String> -RegardingId <Guid> -Query <QueryBase>
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

### -Sender
Gets or sets the sender of the email messages. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### -TemplateId
Sets the ID of the template (email template) that is used for the email notification.

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

### -RegardingType
Gets or sets the type of the record with which the email messages are associated.

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

### -RegardingId
Gets or sets the ID of the record with which the email messages are associated.

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

### Microsoft.Crm.Sdk.Messages.SendBulkMailResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SendBulkMailResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SendBulkMailResponse)
## NOTES

## RELATED LINKS
