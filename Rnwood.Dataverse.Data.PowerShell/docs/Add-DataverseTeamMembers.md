---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Add-DataverseTeamMembers

## SYNOPSIS
Adds members to a team.

## SYNTAX

```
Add-DataverseTeamMembers -Connection <ServiceClient> -TeamId <Guid> -MemberIds <Guid[]> 
 [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet adds one or more users as members of a Dataverse team using the AddMembersTeamRequest message.

## EXAMPLES

### Example 1
```powershell
PS C:\> Add-DataverseTeamMembers -Connection $c -TeamId $teamId -MemberIds $userId1,$userId2
```

Adds two users to a team.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

### -TeamId
ID of the team to add members to

### -MemberIds
Array of user IDs to add as team members

### -WhatIf / -Confirm
Standard ShouldProcess parameters.

### CommonParameters
This cmdlet supports the common parameters.

## INPUTS

### System.Guid

## OUTPUTS

### Microsoft.Crm.Sdk.Messages.AddMembersTeamResponse

## NOTES

See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.addmembersteamrequest?view=dataverse-sdk-latest

## RELATED LINKS
