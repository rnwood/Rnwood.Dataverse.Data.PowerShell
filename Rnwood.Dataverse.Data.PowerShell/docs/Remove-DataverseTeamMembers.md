---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseTeamMembers

## SYNOPSIS
Removes members from a team.

## SYNTAX

```
Remove-DataverseTeamMembers -Connection <ServiceClient> -TeamId <Guid> -MemberIds <Guid[]> 
 [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet removes one or more users from a Dataverse team using the RemoveMembersTeamRequest message.

## EXAMPLES

### Example 1
```powershell
PS C:\> Remove-DataverseTeamMembers -Connection $c -TeamId $teamId -MemberIds $userId1,$userId2
```

Removes two users from a team.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

### -TeamId
ID of the team to remove members from

### -MemberIds
Array of user IDs to remove from team members

### -WhatIf / -Confirm
Standard ShouldProcess parameters.

### CommonParameters
This cmdlet supports the common parameters.

## INPUTS

### System.Guid

## OUTPUTS

### Microsoft.Crm.Sdk.Messages.RemoveMembersTeamResponse

## NOTES

See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.removemembersteamrequest?view=dataverse-sdk-latest

## RELATED LINKS
