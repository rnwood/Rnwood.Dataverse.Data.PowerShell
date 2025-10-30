---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseConnectionReference

## SYNOPSIS
Sets connection reference values in Dataverse.

## SYNTAX

### Single (Default)
```
Set-DataverseConnectionReference [-ConnectionReferenceLogicalName] <String> [-ConnectionId] <String>
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Multiple
```
Set-DataverseConnectionReference -ConnectionReferences <Hashtable> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Sets connection reference values in Dataverse. Can set a single connection reference or multiple connection references at once. The cmdlet updates existing connection reference records with the specified connection IDs.

This cmdlet uses the same table and column names as the Import-DataverseSolution cmdlet for consistency:
- Table: `connectionreference`
- Columns: `connectionreferencelogicalname`, `connectionid`

## EXAMPLES

### Example 1: Set a single connection reference
```powershell
PS C:\> Set-DataverseConnectionReference -ConnectionReferenceLogicalName "new_sharedconnectionref" -ConnectionId "12345678-1234-1234-1234-123456789012"
```

Sets the connection reference 'new_sharedconnectionref' to the specified connection ID.

### Example 2: Set multiple connection references
```powershell
PS C:\> Set-DataverseConnectionReference -ConnectionReferences @{
    'new_sharepoint' = '12345678-1234-1234-1234-123456789012'
    'new_sql' = '87654321-4321-4321-4321-210987654321'
}
```

Sets multiple connection references at once using a hashtable.

### Example 3: Set connection references with stored connection IDs
```powershell
PS C:\> $connectionIds = @{
    'new_dataverse' = (Get-DataverseRecord -TableName connection -Filter "name eq 'Production Dataverse'").connectionid
    'new_sharepoint' = (Get-DataverseRecord -TableName connection -Filter "name eq 'Production SharePoint'").connectionid
}
PS C:\> Set-DataverseConnectionReference -ConnectionReferences $connectionIds
```

Retrieves connection IDs by name and sets them for the connection references.

### Example 4: Use with solution import workflow
```powershell
PS C:\> # Set connection references before importing solution
PS C:\> Set-DataverseConnectionReference -ConnectionReferences @{
    'new_sharepoint' = '12345678-1234-1234-1234-123456789012'
    'new_sql' = '87654321-4321-4321-4321-210987654321'
}
PS C:\> Import-DataverseSolution -InFile "solution.zip"
```

Sets connection references before importing a solution, ensuring they are configured correctly.

### Example 5: View previous connection ID
```powershell
PS C:\> $result = Set-DataverseConnectionReference -ConnectionReferenceLogicalName "new_sharedconnectionref" -ConnectionId "12345678-1234-1234-1234-123456789abc"
PS C:\> Write-Host "Changed from $($result.PreviousConnectionId) to $($result.ConnectionId)"
```

Sets a connection reference and displays the previous and new connection IDs.

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

### -ConnectionId
Connection ID (GUID) to set for the connection reference (for single parameter set).

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

### -ConnectionReferenceLogicalName
Logical name of the connection reference to set (for single parameter set).

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

### -ConnectionReferences
Hashtable of connection reference logical names to connection IDs (e.g., @{'new_sharedconnectionref' = '00000000-0000-0000-0000-000000000000'}).

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
- The connection reference must already exist in Dataverse before you can set its value.
- Connection IDs must be valid GUIDs that correspond to connection records in Dataverse.
- This cmdlet follows the same conventions as the Import-DataverseSolution cmdlet's ConnectionReferences parameter.

## RELATED LINKS

[Import-DataverseSolution](Import-DataverseSolution.md)
[Set-DataverseEnvironmentVariable](Set-DataverseEnvironmentVariable.md)
