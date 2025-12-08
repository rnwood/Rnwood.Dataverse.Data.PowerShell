# Remove-DataverseFileData

## SYNOPSIS
Deletes file data from a Dataverse file column.

## SYNTAX

```powershell
Remove-DataverseFileData -Connection <IOrganizationService> -TableName <string> -Id <guid> -ColumnName <string> [-IfExists] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Remove-DataverseFileData` cmdlet deletes file data from a Dataverse file column. The cmdlet removes the file content but does not delete the record itself.

## EXAMPLES

### Example 1: Delete file data
```powershell
Remove-DataverseFileData -Connection $connection -TableName "account" -Id $accountId -ColumnName "documentfile"
```

Deletes the file from the documentfile column on the specified account record.

### Example 2: Use IfExists to suppress errors
```powershell
Remove-DataverseFileData -Connection $connection -TableName "account" -Id $accountId -ColumnName "documentfile" -IfExists
```

Deletes the file if it exists, but does not raise an error if the file column is empty.

### Example 3: Pipe from Get-DataverseRecord
```powershell
Get-DataverseRecord -Connection $connection -TableName "account" -FilterValues @{name="Contoso"} | 
    Remove-DataverseFileData -Connection $connection -ColumnName "documentfile"
```

Pipes records and deletes files from each one.

### Example 4: Use WhatIf to preview changes
```powershell
Remove-DataverseFileData -Connection $connection -TableName "account" -Id $accountId -ColumnName "documentfile" -WhatIf
```

Shows what would happen if the cmdlet runs without actually deleting the file.

## PARAMETERS

### -Connection
The Dataverse connection to use. This can be created using Get-DataverseConnection.

```yaml
Type: IOrganizationService
Required: True
Position: Named
Accept pipeline input: False
```

### -TableName
The logical name of the table containing the file column.

```yaml
Type: String
Required: True
Position: Named
Accept pipeline input: True (ByPropertyName)
Aliases: EntityName, LogicalName
```

### -Id
The ID of the record containing the file to delete.

```yaml
Type: Guid
Required: True
Position: Named
Accept pipeline input: True (ByPropertyName)
```

### -ColumnName
The logical name of the file column.

```yaml
Type: String
Required: True
Position: Named
Accept pipeline input: True (ByPropertyName)
```

### -IfExists
If specified, the cmdlet will not raise an error if the file does not exist.

```yaml
Type: SwitchParameter
Required: False
Position: Named
Accept pipeline input: False
```

### -WhatIf
Shows what would happen if the cmdlet runs without actually performing the operation.

```yaml
Type: SwitchParameter
Required: False
Position: Named
Accept pipeline input: False
```

### -Confirm
Prompts for confirmation before performing the operation.

```yaml
Type: SwitchParameter
Required: False
Position: Named
Accept pipeline input: False
```

## OUTPUTS

None. This cmdlet does not generate any output.

## NOTES
- The cmdlet deletes the file content but does not delete the record
- Use -IfExists to avoid errors when the file column is already empty
- The cmdlet supports -WhatIf and -Confirm for safe operations
- The operation requires the DeleteFile privilege on the entity

## RELATED LINKS
- [Get-DataverseFileData](Get-DataverseFileData.md)
- [Set-DataverseFileData](Set-DataverseFileData.md)
- [Remove-DataverseRecord](Remove-DataverseRecord.md)
- [Microsoft Docs: File columns](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/file-column-data)
