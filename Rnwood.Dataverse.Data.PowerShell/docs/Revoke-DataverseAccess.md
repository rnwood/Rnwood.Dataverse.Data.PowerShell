---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Revoke-DataverseAccess

## SYNOPSIS
Revokes access to a record for a user or team.

## SYNTAX

```
Revoke-DataverseAccess -Connection <ServiceClient> -Target <Object> [-TableName <String>] 
 -Revokee <Object> [-RevokeeTableName <String>] [-WhatIf] [-Confirm] 
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet revokes all access rights to a record for a specific user or team using the RevokeAccessRequest message.

## EXAMPLES

### Example 1
```powershell
PS C:\> Revoke-DataverseAccess -Connection $c -Target $accountId -TableName "account" -Revokee $userId -RevokeeTableName "systemuser"
```

Revokes all access to an account for a user.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

### -Target
Reference to the record to revoke access from

### -TableName
Logical name of the table when Target is specified as a Guid

### -Revokee
Reference to the user or team to revoke access from

### -RevokeeTableName
Logical name of the revokee table (systemuser or team) when Revokee is specified as a Guid

### -WhatIf / -Confirm
Standard ShouldProcess parameters.

### CommonParameters
This cmdlet supports the common parameters.

## INPUTS

### System.Object

## OUTPUTS

### Microsoft.Crm.Sdk.Messages.RevokeAccessResponse

## NOTES

See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.revokeaccessrequest?view=dataverse-sdk-latest

## RELATED LINKS
