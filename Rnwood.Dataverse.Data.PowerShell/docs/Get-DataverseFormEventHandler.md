---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseFormEventHandler

## SYNOPSIS
Retrieves event handlers from a Dataverse form (form-level or control-level events).

## SYNTAX

```
Get-DataverseFormEventHandler -FormId <Guid> [-EventName <String>] [-ControlId <String>] [-TabName <String>]
 [-SectionName <String>] [-HandlerUniqueId <Guid>] [-Published] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseFormEventHandler cmdlet retrieves JavaScript event handlers from a Dataverse form. It can retrieve both form-level events (such as onload, onsave) and control-level events (such as onchange). Handlers can be filtered by event name, handler unique ID, or control location.

## EXAMPLES

### Example 1: Get all form-level event handlers
```powershell
PS C:\> Get-DataverseFormEventHandler -Connection $c -FormId $formId
```

Retrieves all form-level event handlers (onload, onsave, etc.).

### Example 2: Get handlers for a specific event
```powershell
PS C:\> Get-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onload"
```

Retrieves all handlers for the onload event.

### Example 3: Get control-level event handlers
```powershell
PS C:\> Get-DataverseFormEventHandler -Connection $c -FormId $formId -ControlId "firstname" -TabName "general" -SectionName "name"
```

Retrieves event handlers for a specific control.

### Example 4: Get a specific handler by unique ID
```powershell
PS C:\> $handlerId = 'a1b2c3d4-e5f6-4789-abcd-ef0123456789'
PS C:\> Get-DataverseFormEventHandler -Connection $c -FormId $formId -HandlerUniqueId $handlerId
```

Retrieves a specific handler by its unique identifier.

### Example 5: List all event handlers across all controls
```powershell
PS C:\> $controls = Get-DataverseFormControl -Connection $c -FormId $formId
PS C:\> foreach ($control in $controls) {
PS C:\>     $handlers = Get-DataverseFormEventHandler -Connection $c -FormId $formId -ControlId $control.Id -TabName $control.TabName -SectionName $control.SectionName -ErrorAction SilentlyContinue
PS C:\>     if ($handlers) {
PS C:\>         Write-Host "Control: $($control.Id)"
PS C:\>         $handlers | Format-Table EventName, FunctionName, LibraryName
PS C:\>     }
PS C:\> }
```

Lists all event handlers for all controls on a form.

### Example 6: Get event handlers from published form only
```powershell
PS C:\> Get-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onload" -Published
```

Retrieves event handlers from the published version of the form only. By default, the cmdlet retrieves from the unpublished (draft) version which includes all recent changes.

## PARAMETERS

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
The control ID for retrieving control-level events. Requires TabName and SectionName.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EventName
Filter handlers by event name (e.g., onload, onsave, onchange).

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FormId
The ID of the form to retrieve handlers from.

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

### -HandlerUniqueId
Filter by a specific handler's unique ID.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Published
Retrieve only the published version of the form (default is unpublished)

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

### -SectionName
The section name containing the control (required when ControlId is specified).

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TabName
The tab name containing the control (required when ControlId is specified).

```yaml
Type: String
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

### System.Guid
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS

[Set-DataverseFormEventHandler](Set-DataverseFormEventHandler.md)
[Remove-DataverseFormEventHandler](Remove-DataverseFormEventHandler.md)
[Get-DataverseFormLibrary](Get-DataverseFormLibrary.md)
