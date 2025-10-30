---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseEnvironmentVariable

## SYNOPSIS
Sets environment variable values in Dataverse.

## SYNTAX

### Single (Default)
```
Set-DataverseEnvironmentVariable [-SchemaName] <String> [-Value] <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Multiple
```
Set-DataverseEnvironmentVariable -EnvironmentVariables <Hashtable> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Sets environment variable values in Dataverse. Can set a single environment variable or multiple environment variables at once. The cmdlet creates new environment variable value records if they don't exist, or updates existing ones.

This cmdlet uses the same table and column names as the Import-DataverseSolution cmdlet for consistency:
- Table: `environmentvariablevalue`
- Columns: `schemaname`, `value`, `environmentvariablevalueid`

## EXAMPLES

### Example 1: Set a single environment variable
```powershell
PS C:\> Set-DataverseEnvironmentVariable -SchemaName "new_apiurl" -Value "https://api.production.example.com"
```

Sets the environment variable 'new_apiurl' to the specified value.

### Example 2: Set multiple environment variables
```powershell
PS C:\> Set-DataverseEnvironmentVariable -EnvironmentVariables @{
    'new_apiurl' = 'https://api.production.example.com'
    'new_apikey' = 'prod-key-12345'
    'new_timeout' = '30'
}
```

Sets multiple environment variables at once using a hashtable.

### Example 3: Set environment variable with pipeline
```powershell
PS C:\> @{
    'new_apiurl' = 'https://api.staging.example.com'
    'new_environment' = 'staging'
} | ForEach-Object { Set-DataverseEnvironmentVariable -EnvironmentVariables $_ }
```

Sets multiple environment variables by piping a hashtable.

### Example 4: Use with solution import workflow
```powershell
PS C:\> # Set environment variables before importing solution
PS C:\> Set-DataverseEnvironmentVariable -EnvironmentVariables @{
    'new_apiurl' = 'https://api.example.com'
    'new_feature_enabled' = 'true'
}
PS C:\> Import-DataverseSolution -InFile "solution.zip"
```

Sets environment variables before importing a solution, ensuring they are configured correctly.

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
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g. http://server.com/MyOrg/). If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -EnvironmentVariables
Hashtable of environment variable schema names to values (e.g., @{'new_apiurl' = 'https://api.example.com'}).

```yaml
Type: Hashtable
Parameter Sets: Multiple
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SchemaName
Schema name of the environment variable to set (for single parameter set).

```yaml
Type: String
Parameter Sets: Single
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Value
Value to set for the environment variable (for single parameter set). Can be an empty string.

```yaml
Type: String
Parameter Sets: Single
Aliases:

Required: True
Position: 1
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

### None
## OUTPUTS

### System.Object
## NOTES
- The environment variable definition must already exist in Dataverse before you can set its value.
- If the environment variable value record doesn't exist, it will be created.
- If the environment variable value record already exists, it will be updated.
- This cmdlet follows the same conventions as the Import-DataverseSolution cmdlet's EnvironmentVariables parameter.

## RELATED LINKS

[Import-DataverseSolution](Import-DataverseSolution.md)
[Set-DataverseConnectionReference](Set-DataverseConnectionReference.md)
