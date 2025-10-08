---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRemoveMembersTeam

## SYNOPSIS
Contains the data that is needed to remove members from a team.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveMembersTeamRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RemoveMembersTeamRequest)

## SYNTAX

```
Invoke-DataverseRemoveMembersTeam -Connection <ServiceClient> -TeamId <Guid> -MemberIds <Guid>
```

## DESCRIPTION
Contains the data that is needed to remove members from a team.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRemoveMembersTeam -Connection <ServiceClient> -TeamId <Guid> -MemberIds <Guid>
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

### -TeamId
Gets or sets the ID of the team.

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

### -MemberIds
Gets or sets an array of IDs of the users to be removed from the team.

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

### Microsoft.Crm.Sdk.Messages.RemoveMembersTeamResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RemoveMembersTeamResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RemoveMembersTeamResponse)
## NOTES

## RELATED LINKS
