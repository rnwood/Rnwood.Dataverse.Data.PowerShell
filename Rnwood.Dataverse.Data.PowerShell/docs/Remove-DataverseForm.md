---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseForm

## SYNOPSIS
Removes/deletes a form from a Dataverse environment.

## SYNTAX

### ById
```
Remove-DataverseForm -Id <Guid> [-Publish] [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ByName
```
Remove-DataverseForm -Entity <String> -Name <String> [-Publish] [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseForm cmdlet deletes form definitions from a Dataverse environment. Forms can be deleted by ID or by entity name and form name. The cmdlet supports -WhatIf and -Confirm for safe deletion operations. Optionally, the entity can be published after form deletion to apply changes immediately.

## EXAMPLES

### Example 1: Delete a form by ID
```powershell
PS C:\> $conn = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
PS C:\> $formId = 'a1234567-89ab-cdef-0123-456789abcdef'
PS C:\> Remove-DataverseForm -Connection $conn -Id $formId
```

Deletes a form by its ID with confirmation prompt.

### Example 2: Delete a form by entity and name
```powershell
PS C:\> Remove-DataverseForm -Connection $conn -Entity 'contact' -Name 'Old Contact Form' -Confirm:$false
```

Deletes a form by entity name and form name without confirmation.

### Example 3: Delete form and publish
```powershell
PS C:\> Remove-DataverseForm -Connection $conn -Id $formId -Publish
```

Deletes a form and immediately publishes the entity to apply changes.

### Example 4: Delete form if it exists
```powershell
PS C:\> Remove-DataverseForm -Connection $conn -Entity 'account' -Name 'Test Form' -IfExists -Confirm:$false
```

Attempts to delete a form but doesn't raise an error if the form doesn't exist.

### Example 5: WhatIf simulation
```powershell
PS C:\> Remove-DataverseForm -Connection $conn -Id $formId -WhatIf
```

Shows what would happen if the form were deleted without actually deleting it.

## PARAMETERS

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

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g.
http://server.com/MyOrg/).
If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -Entity
Logical name of the entity/table

```yaml
Type: String
Parameter Sets: ByName
Aliases: EntityName, TableName

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the form to delete

```yaml
Type: Guid
Parameter Sets: ById
Aliases: formid

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -IfExists
Don't raise an error if the form doesn't exist

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Name of the form to delete

```yaml
Type: String
Parameter Sets: ByName
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Publish
Publish the entity after deleting the form

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

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

### System.Guid
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
