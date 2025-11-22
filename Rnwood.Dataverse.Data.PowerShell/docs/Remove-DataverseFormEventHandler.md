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

### FormEventByUniqueId
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -HandlerUniqueId <Guid> [-SkipPublish]
 [-Connection <ServiceClient>] [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### FormEventByFunction
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String> -LibraryName <String>
 [-SkipPublish] [-Connection <ServiceClient>] [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### ControlEventByUniqueId
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -HandlerUniqueId <Guid> -ControlId <String>
 -TabName <String> -SectionName <String> [-SkipPublish] [-Connection <ServiceClient>] [-WhatIf] [-Confirm]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ControlEventByFunction
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String> -LibraryName <String>
 -ControlId <String> -TabName <String> -SectionName <String> [-SkipPublish] [-Connection <ServiceClient>]
 [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>] [<CommonParameters>]
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
PS C:\> Remove-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onload" -HandlerUniqueId $handlerId -SkipPublish
```

Removes the handler without publishing the entity.

### Example 5: Remove all handlers for an event
```powershell
PS C:\> $handlers = Get-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onload"
PS C:\> foreach ($handler in $handlers) {
PS C:\>     Remove-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onload" -HandlerUniqueId $handler.HandlerUniqueId -SkipPublish -Confirm:$false
PS C:\> }
```

Removes all handlers for a specific event.

### Example 6: Clean up deprecated handlers
```powershell
PS C:\> $allHandlers = Get-DataverseFormEventHandler -Connection $c -FormId $formId
PS C:\> $deprecated = $allHandlers | Where-Object { $_.LibraryName -like "*deprecated*" }
PS C:\> foreach ($handler in $deprecated) {
PS C:\>     Remove-DataverseFormEventHandler -Connection $c -FormId $formId -EventName $handler.EventName -HandlerUniqueId $handler.HandlerUniqueId -SkipPublish -Confirm:$false
PS C:\> }
```

Removes all handlers using deprecated libraries.

## PARAMETERS

### -Connection
The Dataverse connection to use.

```yaml
Type: ServiceClient
Required: False
```

### -ControlId
The control ID for control-level events.

```yaml
Type: String
Parameter Sets: ControlEventByUniqueId, ControlEventByFunction
Required: True
```

### -EventName
The name of the event containing the handler to remove.

```yaml
Type: String
Required: True
```

### -FormId
The ID of the form.

```yaml
Type: Guid
Required: True
```

### -FunctionName
The function name to identify the handler to remove.

```yaml
Type: String
Parameter Sets: FormEventByFunction, ControlEventByFunction
Required: True
```

### -HandlerUniqueId
The unique ID of the handler to remove.

```yaml
Type: Guid
Parameter Sets: FormEventByUniqueId, ControlEventByUniqueId
Required: True
```

### -LibraryName
The library name to identify the handler to remove (required with FunctionName).

```yaml
Type: String
Parameter Sets: FormEventByFunction, ControlEventByFunction
Required: True
```

### -SectionName
The section name containing the control.

```yaml
Type: String
Parameter Sets: ControlEventByUniqueId, ControlEventByFunction
Required: True
```

### -SkipPublish
If specified, the entity will not be published after removing the handler.

```yaml
Type: SwitchParameter
Required: False
```

### -TabName
The tab name containing the control.

```yaml
Type: String
Parameter Sets: ControlEventByUniqueId, ControlEventByFunction
Required: True
```

## NOTES
- After removing the last handler from an event, the empty event element is automatically cleaned up.
- Use -SkipPublish when removing multiple handlers to avoid multiple publish operations.
- The entity is automatically published after removal unless -SkipPublish is used.

## RELATED LINKS
[Get-DataverseFormEventHandler](Get-DataverseFormEventHandler.md)
[Set-DataverseFormEventHandler](Set-DataverseFormEventHandler.md)
[Remove-DataverseFormLibrary](Remove-DataverseFormLibrary.md)
