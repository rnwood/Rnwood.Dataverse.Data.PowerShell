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

### Default (Form-level event)
```
Set-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String> -LibraryName <String>
 [-HandlerUniqueId <Guid>] [-Enabled <Boolean>] [-Parameters <String>] [-PassExecutionContext <Boolean>]
 [-Application <Boolean>] [-Active <Boolean>] [-SkipPublish] [-Connection <ServiceClient>] [-WhatIf] [-Confirm]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ControlEvent
```
Set-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String> -LibraryName <String>
 -ControlId <String> -TabName <String> -SectionName <String> [-HandlerUniqueId <Guid>] [-Enabled <Boolean>]
 [-Parameters <String>] [-PassExecutionContext <Boolean>] [-Application <Boolean>] [-Active <Boolean>]
 [-SkipPublish] [-Connection <ServiceClient>] [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
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
PS C:\>     Set-DataverseFormEventHandler -Connection $c -FormId $formId @handler -SkipPublish
PS C:\> }
```

Adds multiple handlers efficiently by skipping publish until all are added.

## PARAMETERS

### -Active
Whether the event is active. Default is true.

```yaml
Type: Boolean
Default value: True
```

### -Application
Whether the event is application-managed. Default is false.

```yaml
Type: Boolean
Default value: False
```

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
Parameter Sets: ControlEvent
Required: True
```

### -Enabled
Whether the handler is enabled. Default is true.

```yaml
Type: Boolean
Default value: True
```

### -EventName
The name of the event (e.g., onload, onsave, onchange).

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
The name of the JavaScript function to call.

```yaml
Type: String
Required: True
```

### -HandlerUniqueId
Optional unique ID for the handler. If not specified, a new GUID is generated.

```yaml
Type: Guid
Required: False
```

### -LibraryName
The name of the web resource library containing the function.

```yaml
Type: String
Required: True
```

### -Parameters
Parameters to pass to the function (as a string, typically JSON).

```yaml
Type: String
Required: False
```

### -PassExecutionContext
Whether to pass the execution context to the function. Default is true.

```yaml
Type: Boolean
Default value: True
```

### -SectionName
The section name containing the control (required for control events).

```yaml
Type: String
Parameter Sets: ControlEvent
Required: True
```

### -SkipPublish
If specified, the entity will not be published after adding the handler.

```yaml
Type: SwitchParameter
Required: False
```

### -TabName
The tab name containing the control (required for control events).

```yaml
Type: String
Parameter Sets: ControlEvent
Required: True
```

## OUTPUTS

### System.Management.Automation.PSObject
Returns an object with FormId, EventName, ControlId, TabName, SectionName, FunctionName, LibraryName, HandlerUniqueId, Enabled, Parameters, and PassExecutionContext properties.

## NOTES
- The web resource must exist (published or unpublished) before adding a handler.
- If a handler with the same function name and library already exists, it will be updated.
- Use -SkipPublish when adding multiple handlers to avoid multiple publish operations.

## RELATED LINKS
[Get-DataverseFormEventHandler](Get-DataverseFormEventHandler.md)
[Remove-DataverseFormEventHandler](Remove-DataverseFormEventHandler.md)
[Set-DataverseFormLibrary](Set-DataverseFormLibrary.md)
