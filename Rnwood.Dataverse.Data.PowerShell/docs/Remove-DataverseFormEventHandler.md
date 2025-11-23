---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseFormEventHandler

## SYNOPSIS
Removes an event handler from a Dataverse form (form-level or control-level).

## SYNTAX

### FormEventByUniqueId (Default)
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -HandlerUniqueId <Guid>
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ControlEventByUniqueId
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -HandlerUniqueId <Guid> -ControlId <String>
 -TabName <String> -SectionName <String> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

### FormEventByFunction
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String>
 -LibraryName <String> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### ControlEventByFunction
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String>
 -LibraryName <String> -ControlId <String> -TabName <String> -SectionName <String>
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseFormEventHandler cmdlet removes JavaScript event handlers from Dataverse forms. It supports both form-level and control-level events. Handlers can be identified by their unique ID or by function name and library name combination.

## EXAMPLES

### Example 1: Remove a form-level handler by unique ID
```powershell
PS C:\> $handlerId = 'a1b2c3d4-e5f6-4789-abcd-ef0123456789'
PS C:\> Remove-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onload" -HandlerUniqueId $handlerId
```

Removes a specific onload handler from the form.

### Example 2: Remove a handler by function and library name
```powershell
PS C:\> Remove-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onsave" -FunctionName "ValidateData" -LibraryName "new_/scripts/validation.js"
```

Removes a handler identified by its function and library.

### Example 3: Remove a control-level handler
```powershell
PS C:\> Remove-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onchange" -FunctionName "OnFieldChange" -LibraryName "new_/scripts/main.js" -ControlId "emailaddress1" -TabName "general" -SectionName "contact_info"
```

Removes an onchange handler from a specific control.

### Example 4: Remove without publishing
```powershell
PS C:\> Remove-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onload" -HandlerUniqueId $handlerId
```

Removes the handler without publishing the entity.

### Example 5: Remove all handlers for an event
```powershell
PS C:\> $handlers = Get-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onload"
PS C:\> foreach ($handler in $handlers) {
PS C:\>     Remove-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onload" -HandlerUniqueId $handler.HandlerUniqueId -Confirm:$false
PS C:\> }
```

Removes all handlers for a specific event.

### Example 6: Clean up deprecated handlers
```powershell
PS C:\> $allHandlers = Get-DataverseFormEventHandler -Connection $c -FormId $formId
PS C:\> $deprecated = $allHandlers | Where-Object { $_.LibraryName -like "*deprecated*" }
PS C:\> foreach ($handler in $deprecated) {
PS C:\>     Remove-DataverseFormEventHandler -Connection $c -FormId $formId -EventName $handler.EventName -HandlerUniqueId $handler.HandlerUniqueId -Confirm:$false
PS C:\> }
```

Removes all handlers using deprecated libraries.

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
The Dataverse connection to use.

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

### -ControlId
The control ID for control-level events.

```yaml
Type: String
Parameter Sets: ControlEventByUniqueId, ControlEventByFunction
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EventName
The name of the event containing the handler to remove.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FormId
The ID of the form.

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

### -FunctionName
The function name to identify the handler to remove.

```yaml
Type: String
Parameter Sets: FormEventByFunction, ControlEventByFunction
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HandlerUniqueId
The unique ID of the handler to remove.

```yaml
Type: Guid
Parameter Sets: FormEventByUniqueId, ControlEventByUniqueId
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LibraryName
The library name to identify the handler to remove (required with FunctionName).

```yaml
Type: String
Parameter Sets: FormEventByFunction, ControlEventByFunction
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SectionName
The section name containing the control.

```yaml
Type: String
Parameter Sets: ControlEventByUniqueId, ControlEventByFunction
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TabName
The tab name containing the control.

```yaml
Type: String
Parameter Sets: ControlEventByUniqueId, ControlEventByFunction
Aliases:

Required: True
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
- After removing the last handler from an event, the empty event element is automatically cleaned up.
- Use when removing multiple handlers to avoid multiple publish operations.
- The entity is automatically published after removal unless is used.

## RELATED LINKS

[Get-DataverseFormEventHandler](Get-DataverseFormEventHandler.md)
[Set-DataverseFormEventHandler](Set-DataverseFormEventHandler.md)
[Remove-DataverseFormLibrary](Remove-DataverseFormLibrary.md)
