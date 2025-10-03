---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Add-DataverseAssociation

## SYNOPSIS
Creates a many-to-many relationship between records.

## SYNTAX

```
Add-DataverseAssociation -Connection <ServiceClient> -Target <Object> [-TableName <String>] 
 -RelationshipName <String> -RelatedRecords <Object[]> [-RelatedTableName <String>] 
 [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet creates many-to-many relationships between records using the AssociateRequest message.

Common uses include:
- Adding users to teams
- Associating roles with users or teams
- Linking records via custom many-to-many relationships

## EXAMPLES

### Example 1
```powershell
PS C:\> Add-DataverseAssociation -Connection $c -Target $userId -TableName "systemuser" -RelationshipName "systemuserroles_association" -RelatedRecords $roleId1,$roleId2 -RelatedTableName "role"
```

Associates security roles with a user.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

### -Target
Reference to the primary record

### -TableName
Logical name of the table when Target is specified as a Guid

### -RelationshipName
Name of the many-to-many relationship schema name (e.g., 'systemuserroles_association')

### -RelatedRecords
References to the related records to associate

### -RelatedTableName
Logical name of the related table when RelatedRecords are specified as Guids

### -WhatIf / -Confirm
Standard ShouldProcess parameters.

### CommonParameters
This cmdlet supports the common parameters.

## INPUTS

### System.Object

## OUTPUTS

### Microsoft.Xrm.Sdk.Messages.AssociateResponse

## NOTES

See https://learn.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.messages.associaterequest?view=dataverse-sdk-latest

## RELATED LINKS
