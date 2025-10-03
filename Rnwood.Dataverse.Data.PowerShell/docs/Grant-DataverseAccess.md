---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Grant-DataverseAccess

## SYNOPSIS
Grants access to a record for a user or team.

## SYNTAX

```
Grant-DataverseAccess -Connection <ServiceClient> -Target <Object> [-TableName <String>] 
 -Principal <Object> [-PrincipalTableName <String>] -AccessRights <AccessRights> 
 [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet grants access rights to a record for a specific user or team using the GrantAccessRequest message.

Access rights can include: ReadAccess, WriteAccess, AppendAccess, AppendToAccess, CreateAccess, DeleteAccess, ShareAccess, AssignAccess

## EXAMPLES

### Example 1
```powershell
PS C:\> Grant-DataverseAccess -Connection $c -Target $accountId -TableName "account" -Principal $userId -PrincipalTableName "systemuser" -AccessRights ReadAccess,WriteAccess
```

Grants read and write access to an account for a user.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

### -Target
Reference to the record to grant access to

### -TableName
Logical name of the table when Target is specified as a Guid

### -Principal
Reference to the user or team to grant access to

### -PrincipalTableName
Logical name of the principal table (systemuser or team) when Principal is specified as a Guid

### -AccessRights
Access rights to grant

### -WhatIf / -Confirm
Standard ShouldProcess parameters.

### CommonParameters
This cmdlet supports the common parameters.

## INPUTS

### System.Object

## OUTPUTS

### Microsoft.Crm.Sdk.Messages.GrantAccessResponse

## NOTES

See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.grantaccessrequest?view=dataverse-sdk-latest

## RELATED LINKS
