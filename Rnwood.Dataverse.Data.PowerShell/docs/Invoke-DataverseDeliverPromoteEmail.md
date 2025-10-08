---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseDeliverPromoteEmail

## SYNOPSIS
Contains the data that is needed to create an email activity record from the specified email message (Track in CRM).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeliverPromoteEmailRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.DeliverPromoteEmailRequest)

## SYNTAX

```
Invoke-DataverseDeliverPromoteEmail -Connection <ServiceClient> -EmailId <Guid> -MessageId <String> -Subject <String> -From <String> -To <String> -Cc <String> -Bcc <String> -ReceivedOn <DateTime> -SubmittedBy <String> -Importance <String> -Body <String> -Attachments <EntityCollection> -ExtraProperties <PSObject> -ExtraPropertiesTableName <String> -ExtraPropertiesIgnoreProperties <String[]> -ExtraPropertiesLookupColumns <Hashtable>
```

## DESCRIPTION
Contains the data that is needed to create an email activity record from the specified email message (Track in CRM).

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseDeliverPromoteEmail -Connection <ServiceClient> -EmailId <Guid> -MessageId <String> -Subject <String> -From <String> -To <String> -Cc <String> -Bcc <String> -ReceivedOn <DateTime> -SubmittedBy <String> -Importance <String> -Body <String> -Attachments <EntityCollection> -ExtraProperties <PSObject> -ExtraPropertiesTableName <String> -ExtraPropertiesIgnoreProperties <String[]> -ExtraPropertiesLookupColumns <Hashtable>
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

### -EmailId
Gets or sets the ID of the email from which to create the email. Required.

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

### -MessageId
Gets or sets the ID of the email message stored in the email header. Required.

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

### -Subject
Gets or sets the subject line for the email message. Optional.

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

### -From
Gets or sets the from address for the email message. Required.

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

### -To
Gets or sets the addresses of the recipients of the email message. Required.

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

### -Cc
Gets or sets the addresses of the carbon copy (Cc) recipients for the email message. Required.

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

### -Bcc
Gets or sets the addresses of the blind carbon copy (Bcc) recipients for the email message. Required.

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

### -ReceivedOn
Gets or sets the time the message was received on. Required.

```yaml
Type: DateTime
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SubmittedBy
Gets or sets the email address of the account that is creating the email activity instance. Required.

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

### -Importance
Gets or sets the level of importance for the email message. Required.

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

### -Body
Gets or sets the message body for the email. Required.

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

### -Attachments
Gets or sets a collection of activity mime attachment (email attachment) records to attach to the email message. Required.

```yaml
Type: EntityCollection
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExtraProperties
Gets or sets the extra properties for the email. Optional. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

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

### -ExtraPropertiesTableName
Gets or sets the extra properties for the email. Optional. The logical name of the table/entity type for the ExtraProperties parameter.

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

### -ExtraPropertiesIgnoreProperties
Gets or sets the extra properties for the email. Optional. Properties to ignore when converting ExtraProperties PSObject to Entity.

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

### -ExtraPropertiesLookupColumns
Gets or sets the extra properties for the email. Optional. Hashtable specifying lookup columns for entity reference conversions in ExtraProperties.

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

### Microsoft.Crm.Sdk.Messages.DeliverPromoteEmailResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeliverPromoteEmailResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.DeliverPromoteEmailResponse)
## NOTES

## RELATED LINKS
