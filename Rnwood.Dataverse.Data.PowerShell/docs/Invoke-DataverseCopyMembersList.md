---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseCopyMembersList

## SYNOPSIS
Contains the data that is needed to copy the members from the source list to the target list without creating duplicates.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CopyMembersListRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CopyMembersListRequest)

## SYNTAX

```
Invoke-DataverseCopyMembersList -Connection <ServiceClient> -SourceListId <Guid> -TargetListId <Guid>
```

## DESCRIPTION
Contains the data that is needed to copy the members from the source list to the target list without creating duplicates.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseCopyMembersList -Connection <ServiceClient> -SourceListId <Guid> -TargetListId <Guid>
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

### -SourceListId
Gets or sets the ID of the source list. Required.

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

### -TargetListId
Gets or sets the ID of the target list. Required.

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

### Microsoft.Crm.Sdk.Messages.CopyMembersListResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.CopyMembersListResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.CopyMembersListResponse)
## NOTES

## RELATED LINKS
