---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseFormLibrary

## SYNOPSIS
Removes a script library from a Dataverse form.

## SYNTAX

### ByName
```
Remove-DataverseFormLibrary -FormId <Guid> -LibraryName <String> [-SkipPublish]
 [-Connection <ServiceClient>] [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ByUniqueId
```
Remove-DataverseFormLibrary -FormId <Guid> -LibraryUniqueId <Guid> [-SkipPublish]
 [-Connection <ServiceClient>] [-WhatIf] [-Confirm] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseFormLibrary cmdlet removes a JavaScript library reference from a Dataverse form. The library can be identified by its name or unique ID. After removal, the entity is published unless -SkipPublish is specified. Note that this only removes the library reference from the form; it does not delete the web resource itself.

## EXAMPLES

### Example 1: Remove a library by name
```powershell
PS C:\> Remove-DataverseFormLibrary -Connection $c -FormId $formId -LibraryName "new_/scripts/oldcode.js"
```

Removes the specified library from the form and publishes the entity.

### Example 2: Remove a library by unique ID
```powershell
PS C:\> $libraryId = 'a1b2c3d4-e5f6-4789-abcd-ef0123456789'
PS C:\> Remove-DataverseFormLibrary -Connection $c -FormId $formId -LibraryUniqueId $libraryId
```

Removes a library by its unique identifier.

### Example 3: Remove without publishing
```powershell
PS C:\> Remove-DataverseFormLibrary -Connection $c -FormId $formId -LibraryName "new_/scripts/temp.js" -SkipPublish
```

Removes the library without publishing. Useful when making multiple changes.

### Example 4: Remove all unused libraries
```powershell
PS C:\> $libraries = Get-DataverseFormLibrary -Connection $c -FormId $formId
PS C:\> $unusedLibs = $libraries | Where-Object { $_.Name -like "*test*" }
PS C:\> foreach ($lib in $unusedLibs) {
PS C:\>     Remove-DataverseFormLibrary -Connection $c -FormId $formId -LibraryUniqueId $lib.LibraryUniqueId -SkipPublish -Confirm:$false
PS C:\> }
```

Removes multiple libraries matching a pattern.

## PARAMETERS

### -Connection
The Dataverse connection to use.

```yaml
Type: ServiceClient
Required: False
```

### -Confirm
Prompts for confirmation before removing the library.

```yaml
Type: SwitchParameter
Required: False
```

### -FormId
The ID of the form to remove the library from.

```yaml
Type: Guid
Required: True
```

### -LibraryName
The name of the library to remove.

```yaml
Type: String
Parameter Sets: ByName
Required: True
```

### -LibraryUniqueId
The unique ID of the library to remove.

```yaml
Type: Guid
Parameter Sets: ByUniqueId
Required: True
```

### -SkipPublish
If specified, the entity will not be published after removing the library.

```yaml
Type: SwitchParameter
Required: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.

```yaml
Type: SwitchParameter
Required: False
```

## RELATED LINKS
[Get-DataverseFormLibrary](Get-DataverseFormLibrary.md)
[Set-DataverseFormLibrary](Set-DataverseFormLibrary.md)
