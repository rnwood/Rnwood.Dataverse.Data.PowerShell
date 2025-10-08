---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseSendEmail

## SYNOPSIS
Contains the data that is needed to send an email message.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SendEmailRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SendEmailRequest)

## SYNTAX

```
Invoke-DataverseSendEmail -Connection <ServiceClient> -EmailId <Guid> -IssueSend <Boolean> -TrackingToken <String>
```

## DESCRIPTION
Contains the data that is needed to send an email message.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseSendEmail -Connection <ServiceClient> -EmailId <Guid> -IssueSend <Boolean> -TrackingToken <String>
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
Gets or sets the ID of the email to send.

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

### -IssueSend
Gets or sets whether to send the email, or to just record it as sent.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TrackingToken
Gets or sets the tracking token.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.SendEmailResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SendEmailResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SendEmailResponse)
## NOTES

## RELATED LINKS
