# Get-DataverseFileData

## SYNOPSIS
Downloads file data from a Dataverse file column.

## SYNTAX

### FilePath
```powershell
Get-DataverseFileData -Connection <IOrganizationService> -TableName <string> -Id <guid> -ColumnName <string> -FilePath <string> [<CommonParameters>]
```

### Folder
```powershell
Get-DataverseFileData -Connection <IOrganizationService> -TableName <string> -Id <guid> -ColumnName <string> -FolderPath <string> [<CommonParameters>]
```

### Bytes
```powershell
Get-DataverseFileData -Connection <IOrganizationService> -TableName <string> -Id <guid> -ColumnName <string> -AsBytes [<CommonParameters>]
```

## DESCRIPTION
The `Get-DataverseFileData` cmdlet downloads file data from a Dataverse file column. You can save the file to a specific path, to a folder (with auto-generated filename), or retrieve it as a byte array for pipeline processing.

## EXAMPLES

### Example 1: Download file to a specific path
```powershell
Get-DataverseFileData -Connection $connection -TableName "account" -Id $accountId -ColumnName "documentfile" -FilePath "C:\Downloads\document.pdf"
```

Downloads the file from the documentfile column and saves it to the specified path.

### Example 2: Download file to a folder with automatic filename
```powershell
Get-DataverseFileData -Connection $connection -TableName "account" -Id $accountId -ColumnName "documentfile" -FolderPath "C:\Downloads"
```

Downloads the file and saves it to the Downloads folder using the original filename from Dataverse.

### Example 3: Get file as byte array
```powershell
$bytes = Get-DataverseFileData -Connection $connection -TableName "account" -Id $accountId -ColumnName "documentfile" -AsBytes
```

Retrieves the file content as a byte array for processing.

### Example 4: Pipe from Get-DataverseRecord
```powershell
Get-DataverseRecord -Connection $connection -TableName "account" -Id $accountId | Get-DataverseFileData -Connection $connection -ColumnName "documentfile" -FolderPath "C:\Downloads"
```

Pipes a record from Get-DataverseRecord and downloads its file attachment.

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
The ID of the record containing the file.

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
The file path where the file will be saved.

```yaml
Type: String
Parameter Sets: FilePath
Required: True
Position: Named
Accept pipeline input: False
```

### -FolderPath
The folder path where the file will be saved. The filename will be taken from the file metadata.

```yaml
Type: String
Parameter Sets: Folder
Required: True
Position: Named
Accept pipeline input: False
```

### -AsBytes
Return the file content as a byte array.

```yaml
Type: SwitchParameter
Parameter Sets: Bytes
Required: True
Position: Named
Accept pipeline input: False
```

## OUTPUTS

### System.IO.FileInfo
When using -FilePath or -FolderPath parameter sets, returns a FileInfo object representing the saved file.

### System.Byte[]
When using -AsBytes parameter set, returns the file content as a byte array.

## NOTES
- File columns were introduced in Dataverse and are different from traditional Notes/Annotations attachments
- Files are downloaded in 4MB blocks for efficient transfer
- Empty file columns will generate a warning and not create an output file

## RELATED LINKS
- [Set-DataverseFileData](Set-DataverseFileData.md)
- [Remove-DataverseFileData](Remove-DataverseFileData.md)
- [Get-DataverseRecord](Get-DataverseRecord.md)
- [Microsoft Docs: File columns](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/file-column-data)
