---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseQualifyMemberList

## SYNOPSIS
Contains the data that is needed to qualify the specified list and either override the list members or remove them according to the specified option.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.QualifyMemberListRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.QualifyMemberListRequest)

## SYNTAX

```
Invoke-DataverseQualifyMemberList -Connection <ServiceClient> -ListId <Guid> -MembersId <Guid> -OverrideorRemove <Boolean>
```

## DESCRIPTION
Contains the data that is needed to qualify the specified list and either override the list members or remove them according to the specified option.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseQualifyMemberList -Connection <ServiceClient> -ListId <Guid> -MembersId <Guid> -OverrideorRemove <Boolean>
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

### -ListId
Gets or sets the ID of the list to qualify. Required.

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

### -MembersId
Gets or sets an array of IDs of the members to qualify. Required.

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

### -OverrideorRemove
Gets or sets a value that indicates whether to override or remove the members from the list. Required.

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

### Microsoft.Crm.Sdk.Messages.QualifyMemberListResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.QualifyMemberListResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.QualifyMemberListResponse)
## NOTES

## RELATED LINKS
