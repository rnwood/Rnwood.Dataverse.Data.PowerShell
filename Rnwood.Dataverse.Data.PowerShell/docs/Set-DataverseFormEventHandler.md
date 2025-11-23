---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseFormEventHandler

## SYNOPSIS
Adds or updates an event handler in a Dataverse form (form-level or control-level).

## SYNTAX

### FormEvent (Default)
```
Set-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String> -LibraryName <String>
 [-HandlerUniqueId <Guid>] [-Enabled <Boolean>] [-Parameters <String>] [-PassExecutionContext <Boolean>]
 [-Application <Boolean>] [-Active <Boolean>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ControlEvent
```
Set-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String> -LibraryName <String>
 -ControlId <String> -TabName <String> -SectionName <String> [-HandlerUniqueId <Guid>] [-Enabled <Boolean>]
 [-Parameters <String>] [-PassExecutionContext <Boolean>] [-Application <Boolean>] [-Active <Boolean>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseFormEventHandler cmdlet adds or updates JavaScript event handlers on Dataverse forms. It supports both form-level events (onload, onsave) and control-level events (onchange, etc.). The cmdlet validates that the referenced web resource exists before adding the handler.

## EXAMPLES

### Example 1: Add a form onload handler
```powershell
PS C:\> Set-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onload" -FunctionName "OnFormLoad" -LibraryName "new_/scripts/main.js"
```

Adds an onload event handler to the form.

### Example 2: Add a control onchange handler
```powershell
PS C:\> Set-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onchange" -FunctionName "OnFieldChange" -LibraryName "new_/scripts/validation.js" -ControlId "emailaddress1" -TabName "general" -SectionName "contact_info"
```

Adds an onchange event handler to a specific control.

### Example 3: Add a handler with parameters
```powershell
PS C:\> Set-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onload" -FunctionName "InitializeForm" -LibraryName "new_/scripts/init.js" -Parameters "{'mode':'create','showWarnings':true}"
```

Adds a handler with JSON parameters that will be passed to the function.

### Example 4: Add a disabled handler
```powershell
PS C:\> Set-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onsave" -FunctionName "ValidateData" -LibraryName "new_/scripts/validation.js" -Enabled $false
```

Adds a handler but leaves it disabled for later activation.

### Example 5: Update an existing handler
```powershell
PS C:\> Set-DataverseFormEventHandler -Connection $c -FormId $formId -EventName "onload" -FunctionName "ExistingFunction" -LibraryName "new_/scripts/main.js" -PassExecutionContext $false
```

Updates an existing handler's PassExecutionContext setting.

### Example 6: Add multiple handlers without publishing
```powershell
PS C:\> $handlers = @(
PS C:\>     @{ EventName="onload"; FunctionName="Init"; LibraryName="new_/scripts/init.js" },
PS C:\>     @{ EventName="onsave"; FunctionName="Validate"; LibraryName="new_/scripts/validate.js" }
PS C:\> )
PS C:\> foreach ($handler in $handlers) {
PS C:\>     Set-DataverseFormEventHandler -Connection $c -FormId $formId @handler
PS C:\> }
```

Adds multiple handlers efficiently by skipping publish until all are added.

## PARAMETERS

### -Active
Whether the event is active. Default is true.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: True
Accept pipeline input: False
Accept wildcard characters: False
```

### -Application
Whether the event is application-managed. Default is false.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
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
Parameter Sets: ControlEvent
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Enabled
Whether the handler is enabled. Default is true.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: True
Accept pipeline input: False
Accept wildcard characters: False
```

### -EventName
The name of the event (e.g., onload, onsave, onchange).

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
The name of the JavaScript function to call.

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

### -HandlerUniqueId
Optional unique ID for the handler. If not specified, a new GUID is generated.

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

### -LibraryName
The name of the web resource library containing the function.

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

### -Parameters
Parameters to pass to the function (as a string, typically JSON).

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

### -PassExecutionContext
Whether to pass the execution context to the function. Default is true.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: True
Accept pipeline input: False
Accept wildcard characters: False
```

### -SectionName
The section name containing the control (required for control events).

```yaml
Type: String
Parameter Sets: ControlEvent
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TabName
The tab name containing the control (required for control events).

```yaml
Type: String
Parameter Sets: ControlEvent
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

### System.Management.Automation.PSObject
## NOTES
- The web resource must exist (published or unpublished) before adding a handler.
- If a handler with the same function name and library already exists, it will be updated.
- Use when adding multiple handlers to avoid multiple publish operations.

## RELATED LINKS

[Get-DataverseFormEventHandler](Get-DataverseFormEventHandler.md)
[Remove-DataverseFormEventHandler](Remove-DataverseFormEventHandler.md)
[Set-DataverseFormLibrary](Set-DataverseFormLibrary.md)
