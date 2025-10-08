---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCheckPromoteEmail

## SYNOPSIS
Contains the data that is needed to check whether the incoming email message should be promoted to Dataverse.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CheckPromoteEmailRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CheckPromoteEmailRequest)

## SYNTAX

```
Invoke-DataverseCheckPromoteEmail -Connection <ServiceClient> -MessageId <String> -Subject <String> -DirectionCode <Int32>
```

## DESCRIPTION
Contains the data that is needed to check whether the incoming email message should be promoted to Dataverse.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCheckPromoteEmail -Connection <ServiceClient> -MessageId <String> -Subject <String> -DirectionCode <Int32>
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

### -MessageId
Gets or sets the message ID that is contained in the email header. Required.

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
Gets or sets the subject of the message. Optional.

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

### -DirectionCode
Gets or sets the direction of a mail checked for promotion for uniqueness.

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

### Microsoft.Crm.Sdk.Messages.CheckPromoteEmailResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CheckPromoteEmailResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CheckPromoteEmailResponse)
## NOTES

## RELATED LINKS
