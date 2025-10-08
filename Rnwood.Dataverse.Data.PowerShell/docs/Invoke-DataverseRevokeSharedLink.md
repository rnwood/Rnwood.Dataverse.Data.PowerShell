---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRevokeSharedLink

## SYNOPSIS
Revokes user access rights from a shared link.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RevokeSharedLinkRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RevokeSharedLinkRequest)

## SYNTAX

```
Invoke-DataverseRevokeSharedLink -Connection <ServiceClient> -Target <PSObject> -SharedRights <AccessRights>
```

## DESCRIPTION
Revokes user access rights from a shared link.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRevokeSharedLink -Connection <ServiceClient> -Target <PSObject> -SharedRights <AccessRights>
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

### -Target
Gets or sets the target table row. Accepts PSObject with Id and TableName/EntityName/LogicalName properties, or a string containing the entity name for lookup by name.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True
Accept wildcard characters: False
```

### -SharedRights
Gets or sets the access rights that will be revoked.

```yaml
Type: AccessRights
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

### Microsoft.Crm.Sdk.Messages.RevokeSharedLinkResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RevokeSharedLinkResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RevokeSharedLinkResponse)
## NOTES

## RELATED LINKS
