---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseView

## SYNOPSIS
Removes Dataverse views (savedquery and userquery entities).

## SYNTAX

```
Remove-DataverseView -Id <Guid> [-ViewType <String>] [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseView cmdlet deletes Dataverse views from the savedquery (system views) and userquery (personal views) entities. Views define how records are displayed in model-driven apps and other Dataverse interfaces.

The cmdlet supports safe deletion with confirmation prompts and WhatIf support. Use the -IfExists parameter to suppress errors when the view doesn't exist.

## EXAMPLES

### Example 1: Remove a personal view
```powershell
PS C:\> Remove-DataverseView -Connection $c -Id "12345678-1234-1234-1234-123456789012"
```

Removes a personal view by its ID.

### Example 2: Remove a system view
```powershell
PS C:\> Remove-DataverseView -Connection $c -Id "12345678-1234-1234-1234-123456789012" -SystemView
```

Removes a system view by its ID.

### Example 3: Remove view if it exists
```powershell
PS C:\> Remove-DataverseView -Connection $c -Id "12345678-1234-1234-1234-123456789012" -IfExists
```

Removes a view if it exists, without raising an error if it doesn't.

### Example 4: Remove multiple views
```powershell
PS C:\> Get-DataverseView -Connection $c -Name "Test*" | Remove-DataverseView -Connection $c
```

Finds all views whose names start with "Test" and removes them.

### Example 5: Remove with confirmation suppressed
```powershell
PS C:\> Remove-DataverseView -Connection $c -Id "12345678-1234-1234-1234-123456789012" -Confirm:$false
```

Removes a view without prompting for confirmation.

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

### -Id
ID of the view to remove

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IfExists
If specified, the cmdlet will not raise an error if the view does not exist

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

### -ViewType
Remove a system view (savedquery) instead of a personal view (userquery)

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
## OUTPUTS

### System.Object
## NOTES

- Prompts for confirmation by default; use -Confirm:$false to suppress.
- System views (savedquery) are accessible to all users; personal views (userquery) are user-specific.
- Use -IfExists to avoid errors when the view may have already been deleted.
- Supports WhatIf to preview the operation without executing it.
- Cannot be used to delete default system views in some cases.

## RELATED LINKS

[View Management Documentation](../../docs/core-concepts/view-management.md)

[Get-DataverseView](Get-DataverseView.md)

[Set-DataverseView](Set-DataverseView.md)
