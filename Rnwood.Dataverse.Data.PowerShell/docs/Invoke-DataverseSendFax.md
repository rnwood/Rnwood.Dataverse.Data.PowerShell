---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseSendFax

## SYNOPSIS
Contains the data that is needed to send a fax.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SendFaxRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SendFaxRequest)

## SYNTAX

```
Invoke-DataverseSendFax -Connection <ServiceClient> -FaxId <Guid> -IssueSend <Boolean>
```

## DESCRIPTION
Contains the data that is needed to send a fax.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseSendFax -Connection <ServiceClient> -FaxId <Guid> -IssueSend <Boolean>
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

### -FaxId
Gets or sets the ID of the fax to send.

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
Gets or sets whether to send the e-mail, or to just record it as sent.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.SendFaxResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.SendFaxResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.SendFaxResponse)
## NOTES

## RELATED LINKS
