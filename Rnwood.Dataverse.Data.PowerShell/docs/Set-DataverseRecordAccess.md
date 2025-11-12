---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseRecordAccess

## SYNOPSIS
Grants or modifies access rights for a security principal (user or team) on a specific record.

## SYNTAX

```
Set-DataverseRecordAccess [-Target] <EntityReference> [-Principal] <Guid> [-AccessRights] <AccessRights> 
 [-IsTeam] [-Connection <ServiceClient>] [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>] 
 [<CommonParameters>]
```

## DESCRIPTION

This cmdlet grants or modifies access rights for a security principal (user or team) on a specific Dataverse record using the GrantAccessRequest or ModifyAccessRequest messages.

The cmdlet automatically handles both scenarios:
- If the principal doesn't have access, it grants new access
- If the principal already has access, it modifies the existing access

Access rights can be combined using bitwise OR to grant multiple permissions at once.

## EXAMPLES

### Example 1: Grant read and write access to a user
```powershell
PS C:\> $accountRef = New-Object Microsoft.Xrm.Sdk.EntityReference("account", "12345678-1234-1234-1234-123456789012")
PS C:\> $userId = [Guid]"87654321-4321-4321-4321-210987654321"
PS C:\> $accessRights = [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess
PS C:\> Set-DataverseRecordAccess -Target $accountRef -Principal $userId -AccessRights $accessRights
```

Grants read and write access to the specified user for the account record.

### Example 2: Grant full access to a team
```powershell
PS C:\> $contactRef = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", "11111111-1111-1111-1111-111111111111")
PS C:\> $teamId = [Guid]"22222222-2222-2222-2222-222222222222"
PS C:\> $fullAccess = [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::DeleteAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::ShareAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::AssignAccess
PS C:\> Set-DataverseRecordAccess -Target $contactRef -Principal $teamId -AccessRights $fullAccess -IsTeam
```

Grants full access to a team for the contact record.

### Example 3: Modify existing access to add delete rights
```powershell
PS C:\> $opportunityRef = New-Object Microsoft.Xrm.Sdk.EntityReference("opportunity", "33333333-3333-3333-3333-333333333333")
PS C:\> $userId = [Guid]"44444444-4444-4444-4444-444444444444"
PS C:\> # User already has read/write, now add delete
PS C:\> $newAccess = [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::DeleteAccess
PS C:\> Set-DataverseRecordAccess -Target $opportunityRef -Principal $userId -AccessRights $newAccess
```

Modifies existing access to include delete rights.

## PARAMETERS

### -Target
The record for which to set access rights. Must be an EntityReference object with the entity logical name and record ID.

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
The security principal (user or team) for which to set access rights. Must be a Guid representing the systemuser or team record ID.

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

### -AccessRights
The access rights to grant. Can be combined using bitwise OR.

Valid values include:
- ReadAccess - Read access
- WriteAccess - Write access  
- AppendAccess - Append access
- AppendToAccess - Append to access
- CreateAccess - Create access
- DeleteAccess - Delete access
- ShareAccess - Share access
- AssignAccess - Assign access

```yaml
Type: AccessRights
Parameter Sets: (All)
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IsTeam
Specify if the principal is a team (default is systemuser).

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL.

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

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

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

### None
## NOTES
See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.grantaccessrequest?view=dataverse-sdk-latest

## RELATED LINKS

[Get-DataverseRecordAccess](Get-DataverseRecordAccess.md)

[Test-DataverseRecordAccess](Test-DataverseRecordAccess.md)

[Remove-DataverseRecordAccess](Remove-DataverseRecordAccess.md)

[Microsoft Learn: Security concepts in Dataverse](https://learn.microsoft.com/power-platform/admin/wp-security-cds)
