---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseAddChannelAccessProfilePrivileges

## SYNOPSIS
For internal use only.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddChannelAccessProfilePrivilegesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.AddChannelAccessProfilePrivilegesRequest)

## SYNTAX

```
Invoke-DataverseAddChannelAccessProfilePrivileges -Connection <ServiceClient> -ChannelAccessProfileId <Guid> -Privileges <ChannelAccessProfilePrivilege[]>
```

## DESCRIPTION
For internal use only.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseAddChannelAccessProfilePrivileges -Connection <ServiceClient> -ChannelAccessProfileId <Guid> -Privileges <ChannelAccessProfilePrivilege[]>
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

### -Privileges
Use this to retrieve entity information plus privileges for the entity. Value = 4.

```yaml
Type: ChannelAccessProfilePrivilege[]
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

### Microsoft.Crm.Sdk.Messages.AddChannelAccessProfilePrivilegesResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.AddChannelAccessProfilePrivilegesResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.AddChannelAccessProfilePrivilegesResponse)
## NOTES

## RELATED LINKS
