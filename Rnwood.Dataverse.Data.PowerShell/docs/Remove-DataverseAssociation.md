---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseAssociation

## SYNOPSIS
Removes a many-to-many relationship between records.

## SYNTAX

```
Remove-DataverseAssociation -Connection <ServiceClient> -Target <Object> [-TableName <String>] 
 -RelationshipName <String> -RelatedRecords <Object[]> [-RelatedTableName <String>] 
 [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet removes many-to-many relationships between records using the DisassociateRequest message.

Common uses include:
- Removing users from teams
- Disassociating roles from users or teams
- Unlinking records via custom many-to-many relationships

## EXAMPLES

### Example 1
```powershell
PS C:\> Remove-DataverseAssociation -Connection $c -Target $userId -TableName "systemuser" -RelationshipName "systemuserroles_association" -RelatedRecords $roleId -RelatedTableName "role"
```

Removes a security role from a user.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

### -Target
Reference to the primary record

### -TableName
Logical name of the table when Target is specified as a Guid

### -RelationshipName
Name of the many-to-many relationship schema name

### -RelatedRecords
References to the related records to disassociate

### -RelatedTableName
Logical name of the related table when RelatedRecords are specified as Guids

### -WhatIf / -Confirm
Standard ShouldProcess parameters.

### CommonParameters
This cmdlet supports the common parameters.

## INPUTS

### System.Object

## OUTPUTS

### Microsoft.Xrm.Sdk.Messages.DisassociateResponse

## NOTES

See https://learn.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.messages.disassociaterequest?view=dataverse-sdk-latest

## RELATED LINKS
