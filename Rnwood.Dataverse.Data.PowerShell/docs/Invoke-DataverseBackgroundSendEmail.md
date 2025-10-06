---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseBackgroundSendEmail

## SYNOPSIS
Contains the data that is needed to send email messages asynchronously.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.BackgroundSendEmailRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.BackgroundSendEmailRequest)

## SYNTAX

```
Invoke-DataverseBackgroundSendEmail -Connection <ServiceClient> -Query <QueryBase>
```

## DESCRIPTION
Contains the data that is needed to send email messages asynchronously.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseBackgroundSendEmail -Connection <ServiceClient> -Query <QueryBase>
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

### Microsoft.Crm.Sdk.Messages.BackgroundSendEmailResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.BackgroundSendEmailResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.BackgroundSendEmailResponse)
## NOTES

## RELATED LINKS
