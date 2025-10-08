---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseSendTemplate

## SYNOPSIS
Contains the data that is needed to send a bulk email message that is created from a template.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SendTemplateRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SendTemplateRequest)

## SYNTAX

```
Invoke-DataverseSendTemplate -Connection <ServiceClient> -TemplateId <Guid> -Sender <PSObject> -RecipientType <String> -RecipientIds <Guid> -RegardingType <String> -RegardingId <Guid> -DeliveryPriorityCode <OptionSetValue>
```

## DESCRIPTION
Contains the data that is needed to send a bulk email message that is created from a template.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseSendTemplate -Connection <ServiceClient> -TemplateId <Guid> -Sender <PSObject> -RecipientType <String> -RecipientIds <Guid> -RegardingType <String> -RegardingId <Guid> -DeliveryPriorityCode <OptionSetValue>
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

### -TemplateId
Gets or sets the ID of the template to be used for the email. Required.

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

### -Sender
Gets or sets the sender of the email. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

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

### -RecipientType
Gets or sets the type of entity that is represented by the list of recipients. Required.

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

### -RecipientIds
Gets or sets the array that contains the list of recipients for the email. Required.

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
Gets or sets the type of entity that is represented by the regarding ID. Required.

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
Gets or sets the ID of a record that the email is regarding. Required.

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

### -DeliveryPriorityCode
Gets or sets the delivery priority code for the email.

```yaml
Type: OptionSetValue
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

### Microsoft.Crm.Sdk.Messages.SendTemplateResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SendTemplateResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SendTemplateResponse)
## NOTES

## RELATED LINKS
