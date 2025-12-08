# Set-DataverseFileData

## SYNOPSIS
Uploads file data to a Dataverse file column.

## SYNTAX

### FilePath
```powershell
Set-DataverseFileData -Connection <IOrganizationService> -TableName <string> -Id <guid> -ColumnName <string> -FilePath <string> [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Bytes
```powershell
Set-DataverseFileData -Connection <IOrganizationService> -TableName <string> -Id <guid> -ColumnName <string> -FileContent <byte[]> [-FileName <string>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Set-DataverseFileData` cmdlet uploads file data to a Dataverse file column. You can upload from a file path or from a byte array in memory. The cmdlet uses block-based uploading for efficient transfer of large files.

## EXAMPLES

### Example 1: Upload file from path
```powershell
Set-DataverseFileData -Connection $connection -TableName "account" -Id $accountId -ColumnName "documentfile" -FilePath "C:\Documents\contract.pdf"
```

Uploads a file from the specified path to the file column.

### Example 2: Upload from byte array
```powershell
$bytes = [System.IO.File]::ReadAllBytes("C:\Documents\contract.pdf")
Set-DataverseFileData -Connection $connection -TableName "account" -Id $accountId -ColumnName "documentfile" -FileContent $bytes -FileName "contract.pdf"
```

Uploads file content from a byte array with a specified filename.

### Example 3: Pipe from Get-DataverseRecord
```powershell
Get-DataverseRecord -Connection $connection -TableName "account" -Id $accountId | 
    Set-DataverseFileData -Connection $connection -ColumnName "documentfile" -FilePath "C:\Documents\contract.pdf"
```

Pipes a record and uploads a file to it.

### Example 4: Use WhatIf to preview changes
```powershell
Set-DataverseFileData -Connection $connection -TableName "account" -Id $accountId -ColumnName "documentfile" -FilePath "C:\Documents\contract.pdf" -WhatIf
```

Shows what would happen if the cmdlet runs without actually uploading the file.

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
The ID of the record to update with the file.

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

### -FilePath
The file path of the file to upload.

```yaml
Type: String
Parameter Sets: FilePath
Required: True
Position: Named
Accept pipeline input: False
```

### -FileContent
The file content as a byte array.

```yaml
Type: Byte[]
Parameter Sets: Bytes
Required: True
Position: Named
Accept pipeline input: True
```

### -FileName
The filename to use when uploading from byte array. If not specified, defaults to "file.bin".

```yaml
Type: String
Parameter Sets: Bytes
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
- Files are uploaded in 4MB blocks for efficient transfer
- The cmdlet supports -WhatIf and -Confirm for safe operations
- Uploading a file will replace any existing file in the column
- Maximum file size depends on your Dataverse environment settings

## RELATED LINKS
- [Get-DataverseFileData](Get-DataverseFileData.md)
- [Remove-DataverseFileData](Remove-DataverseFileData.md)
- [Set-DataverseRecord](Set-DataverseRecord.md)
- [Microsoft Docs: File columns](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/file-column-data)
