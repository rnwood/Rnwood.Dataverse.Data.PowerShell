---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Test-DataverseRecordAccess

## SYNOPSIS
Tests the access rights a security principal (user or team) has for a specific record.

## SYNTAX

```
Test-DataverseRecordAccess [-Target] <EntityReference> [-Principal] <Guid> [-Connection <ServiceClient>] 
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet executes the Dataverse RetrievePrincipalAccess message and returns the access rights that a security principal (user or team) has for a specific record.

The response includes the access rights as an AccessRights enumeration value, which can include:
- None - No access
- ReadAccess - Read access
- WriteAccess - Write access
- AppendAccess - Append access
- AppendToAccess - Append to access
- CreateAccess - Create access
- DeleteAccess - Delete access
- ShareAccess - Share access
- AssignAccess - Assign access

This is useful for verifying permissions, debugging security issues, and implementing custom security logic.

## EXAMPLES

### Example 1: Check user access to a specific account record
```powershell
PS C:\> $accountRef = New-Object Microsoft.Xrm.Sdk.EntityReference("account", "12345678-1234-1234-1234-123456789012")
PS C:\> $userId = [Guid]"87654321-4321-4321-4321-210987654321"
PS C:\> $access = Test-DataverseRecordAccess -Target $accountRef -Principal $userId
PS C:\> $access
ReadAccess, WriteAccess, DeleteAccess, ShareAccess, AssignAccess
```

Tests what access rights the specified user has for the account record.

### Example 2: Check if a user has write access
```powershell
PS C:\> $contactRef = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", "11111111-1111-1111-1111-111111111111")
PS C:\> $userId = [Guid]"22222222-2222-2222-2222-222222222222"
PS C:\> $access = Test-DataverseRecordAccess -Target $contactRef -Principal $userId
PS C:\> if ($access -band [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) {
>>     Write-Host "User has write access"
>> } else {
>>     Write-Host "User does not have write access"
>> }
```

Checks if a user has write access to a contact record.

### Example 3: Check access for current user
```powershell
PS C:\> $whoAmI = Get-DataverseWhoAmI -Connection $c
PS C:\> $recordRef = New-Object Microsoft.Xrm.Sdk.EntityReference("opportunity", "33333333-3333-3333-3333-333333333333")
PS C:\> $access = Test-DataverseRecordAccess -Target $recordRef -Principal $whoAmI.UserId -Connection $c
PS C:\> $access
ReadAccess, WriteAccess, AppendAccess, AppendToAccess, DeleteAccess, ShareAccess, AssignAccess
```

Checks what access the current authenticated user has to an opportunity record.

## PARAMETERS

### -Target
The record for which to check access rights. Must be an EntityReference object with the entity logical name and record ID.

```yaml
Type: EntityReference
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Principal
The security principal (user or team) for which to check access rights. Must be a Guid representing the systemuser or team record ID.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g. http://server.com/MyOrg/).

If not provided, uses the default connection set via `Get-DataverseConnection -SetAsDefault`.

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.AccessRights
## NOTES
See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.retrieveprincipalaccessrequest?view=dataverse-sdk-latest

## RELATED LINKS

[Get-DataverseWhoAmI](Get-DataverseWhoAmI.md)

[Microsoft Learn: Security concepts in Dataverse](https://learn.microsoft.com/power-platform/admin/wp-security-cds)
