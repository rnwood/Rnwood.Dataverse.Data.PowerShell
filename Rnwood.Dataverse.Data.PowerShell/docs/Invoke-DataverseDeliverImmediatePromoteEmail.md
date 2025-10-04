---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseDeliverImmediatePromoteEmail

## SYNOPSIS
Contains the data that is needed to deliver an email from an email client.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.DeliverImmediatePromoteEmailRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.DeliverImmediatePromoteEmailRequest)

## SYNTAX

```
Invoke-DataverseDeliverImmediatePromoteEmail -Connection <ServiceClient> [-EmailId <Guid>]
 [-MessageId <String>] [-Subject <String>] [-From <String>] [-To <String>] [-Cc <String>] [-Bcc <String>]
 [-ReceivedOn <DateTime>] [-SubmittedBy <String>] [-Importance <String>] [-Body <String>]
 [-AttachmentIds <String[]>] [-EWSUrl <String>] [-AttachmentToken <String>] [-ExtraProperties <PSObject>]
 [-ExtraPropertiesTableName <String>] [-ExtraPropertiesIgnoreProperties <String[]>]
 [-ExtraPropertiesLookupColumns <Hashtable>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION
Contains the data that is needed to deliver an email from an email client.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseDeliverImmediatePromoteEmail -Connection <ServiceClient> -EmailId <Guid> -MessageId <String> -Subject <String> -From <String> -To <String> -Cc <String> -Bcc <String> -ReceivedOn <DateTime> -SubmittedBy <String> -Importance <String> -Body <String> -AttachmentIds <String[]> -EWSUrl <String> -AttachmentToken <String> -ExtraProperties <PSObject> -ExtraPropertiesTableName <String> -ExtraPropertiesIgnoreProperties <String[]> -ExtraPropertiesLookupColumns <Hashtable>
```

## PARAMETERS

### -AttachmentIds
Gets or sets the AttachmentIds for the request.

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

### -AttachmentToken
Gets or sets the AttachmentToken for the request.

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
Gets or sets the addresses of the blind carbon copy (Bcc) recipients for the email message.

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
Gets or sets the Body for the request.

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
Gets or sets the addresses of the carbon copy (Cc) recipients for the email message.

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

### -EWSUrl
Gets or sets the EWSUrl for the request.

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

### -EmailId
The ID of the Email.

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

### -ExtraProperties
For internal use only. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type.

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

### -ExtraPropertiesIgnoreProperties
For internal use only. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Properties to ignore when converting ExtraProperties PSObject to Entity.

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
For internal use only. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Hashtable specifying lookup columns for entity reference conversions in ExtraProperties.

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

### -ExtraPropertiesTableName
For internal use only. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. Accepts PSObject with properties that will be converted to Entity. Use corresponding TableName parameter to specify the entity type. The logical name of the table/entity type for the ExtraProperties parameter.

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
Gets or sets the from address for the email message.

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
Gets or sets the Importance for the request.

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

### -ReceivedOn
Gets or sets the ReceivedOn for the request.

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

### -SubmittedBy
Gets or sets the SubmittedBy for the request.

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
Gets or sets the addresses of the recipients of the email message.

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

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
