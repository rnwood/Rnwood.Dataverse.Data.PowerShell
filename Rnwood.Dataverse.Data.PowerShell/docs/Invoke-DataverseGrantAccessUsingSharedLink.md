---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseGrantAccessUsingSharedLink

## SYNOPSIS
Adds a system user to the shared link access team of the target table row.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GrantAccessUsingSharedLinkRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GrantAccessUsingSharedLinkRequest)

## SYNTAX

```
Invoke-DataverseGrantAccessUsingSharedLink -Connection <ServiceClient> -RecordUrlWithSharedLink <String>
```

## DESCRIPTION
Adds a system user to the shared link access team of the target table row.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseGrantAccessUsingSharedLink -Connection <ServiceClient> -RecordUrlWithSharedLink <String>
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

### -RecordUrlWithSharedLink
Gets or sets the URL of the table row that has a shared link.

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

### Microsoft.Crm.Sdk.Messages.GrantAccessUsingSharedLinkResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.GrantAccessUsingSharedLinkResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.GrantAccessUsingSharedLinkResponse)
## NOTES

## RELATED LINKS
