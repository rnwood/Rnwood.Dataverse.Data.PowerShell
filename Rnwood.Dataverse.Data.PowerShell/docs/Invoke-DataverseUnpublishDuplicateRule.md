---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseUnpublishDuplicateRule

## SYNOPSIS
Contains the data that is needed to submit an asynchronous job to unpublish a duplicate rule.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UnpublishDuplicateRuleRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.UnpublishDuplicateRuleRequest)

## SYNTAX

```
Invoke-DataverseUnpublishDuplicateRule -Connection <ServiceClient> -DuplicateRuleId <Guid>
```

## DESCRIPTION
Contains the data that is needed to submit an asynchronous job to unpublish a duplicate rule.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseUnpublishDuplicateRule -Connection <ServiceClient> -DuplicateRuleId <Guid>
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

### -DuplicateRuleId
Gets or sets the ID of the duplicate rule to be unpublished. Required.

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

### Microsoft.Crm.Sdk.Messages.UnpublishDuplicateRuleResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.UnpublishDuplicateRuleResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.UnpublishDuplicateRuleResponse)
## NOTES

## RELATED LINKS
