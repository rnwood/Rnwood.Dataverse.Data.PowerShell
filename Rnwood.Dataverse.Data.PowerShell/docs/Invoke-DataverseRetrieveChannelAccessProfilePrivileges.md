---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveChannelAccessProfilePrivileges

## SYNOPSIS
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveChannelAccessProfilePrivilegesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveChannelAccessProfilePrivilegesRequest)

## SYNTAX

```
Invoke-DataverseRetrieveChannelAccessProfilePrivileges -Connection <ServiceClient> -ChannelAccessProfileId <Guid>
```

## DESCRIPTION
For internal use only.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveChannelAccessProfilePrivileges -Connection <ServiceClient> -ChannelAccessProfileId <Guid>
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

### -ChannelAccessProfileId
For internal use only.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.RetrieveChannelAccessProfilePrivilegesResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveChannelAccessProfilePrivilegesResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveChannelAccessProfilePrivilegesResponse)
## NOTES

## RELATED LINKS
