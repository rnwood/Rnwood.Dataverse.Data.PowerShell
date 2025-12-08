# File Attachments

File columns in Dataverse allow you to store files directly in table columns, providing a modern alternative to Notes/Annotations. This document covers how to work with file columns using PowerShell cmdlets.

## Overview

File columns provide:
- Direct file storage in table columns
- Efficient block-based upload/download
- Support for large files
- Integration with Power Apps and Power Automate
- Better performance than traditional attachments

## Available Cmdlets

- **[Get-DataverseFileData](Get-DataverseFileData.md)** - Download file data from a file column
- **[Set-DataverseFileData](Set-DataverseFileData.md)** - Upload file data to a file column
- **[Remove-DataverseFileData](Remove-DataverseFileData.md)** - Delete file data from a file column

## Common Scenarios

### Uploading Files

Upload a file from your local file system:

```powershell
# Upload a single file
Set-DataverseFileData -Connection $conn -TableName "account" -Id $accountId -ColumnName "contractdocument" -FilePath "C:\Contracts\contract.pdf"

# Upload with confirmation
Set-DataverseFileData -Connection $conn -TableName "account" -Id $accountId -ColumnName "contractdocument" -FilePath "C:\Contracts\contract.pdf" -Confirm
```

Upload from memory (byte array):

```powershell
# Load file content
$bytes = [System.IO.File]::ReadAllBytes("C:\Documents\report.xlsx")

# Upload from memory
Set-DataverseFileData -Connection $conn -TableName "account" -Id $accountId -ColumnName "reportfile" -FileContent $bytes -FileName "monthly-report.xlsx"
```

### Downloading Files

Download to a specific path:

```powershell
# Download to specific file
Get-DataverseFileData -Connection $conn -TableName "account" -Id $accountId -ColumnName "contractdocument" -FilePath "C:\Downloads\contract.pdf"
```

Download to a folder with automatic filename:

```powershell
# Download to folder (uses original filename)
Get-DataverseFileData -Connection $conn -TableName "account" -Id $accountId -ColumnName "contractdocument" -FolderPath "C:\Downloads"
```

Get file as byte array for processing:

```powershell
# Get file content as bytes
$bytes = Get-DataverseFileData -Connection $conn -TableName "account" -Id $accountId -ColumnName "contractdocument" -AsBytes

# Process the content
$content = [System.Text.Encoding]::UTF8.GetString($bytes)
```

### Deleting Files

Delete file data from a column:

```powershell
# Delete file
Remove-DataverseFileData -Connection $conn -TableName "account" -Id $accountId -ColumnName "contractdocument"

# Delete with confirmation
Remove-DataverseFileData -Connection $conn -TableName "account" -Id $accountId -ColumnName "contractdocument" -Confirm

# Delete only if exists (no error if empty)
Remove-DataverseFileData -Connection $conn -TableName "account" -Id $accountId -ColumnName "contractdocument" -IfExists
```

## Pipeline Support

All cmdlets support piping from `Get-DataverseRecord`:

```powershell
# Download files from multiple records
Get-DataverseRecord -Connection $conn -TableName "account" -FilterValues @{statecode=0} |
    Get-DataverseFileData -Connection $conn -ColumnName "contractdocument" -FolderPath "C:\Exports"

# Upload files to multiple records
Get-DataverseRecord -Connection $conn -TableName "account" -FilterValues @{contractneeded=$true} |
    Set-DataverseFileData -Connection $conn -ColumnName "contractdocument" -FilePath "C:\Templates\standard-contract.pdf"

# Delete files from multiple records
Get-DataverseRecord -Connection $conn -TableName "account" -FilterValues @{deletefiles=$true} |
    Remove-DataverseFileData -Connection $conn -ColumnName "contractdocument" -IfExists
```

## Batch Operations

Process multiple files efficiently:

```powershell
# Export all contract files
$accounts = Get-DataverseRecord -Connection $conn -TableName "account" -FilterValues @{contractdocument=@{value=$null; operator="NotNull"}}

foreach ($account in $accounts) {
    $folderName = $account.name -replace '[\\/:*?"<>|]', '_'  # Sanitize folder name
    $exportPath = "C:\Exports\$folderName"
    New-Item -ItemType Directory -Path $exportPath -Force | Out-Null
    
    $account | Get-DataverseFileData -Connection $conn -ColumnName "contractdocument" -FolderPath $exportPath
    Write-Host "Exported file for: $($account.name)"
}
```

## Best Practices

1. **Use -WhatIf for Testing**: Always test operations with `-WhatIf` before running them on production data
   ```powershell
   Set-DataverseFileData ... -WhatIf
   ```

2. **Handle Empty Files**: Use `-IfExists` when deleting to avoid errors on empty columns
   ```powershell
   Remove-DataverseFileData ... -IfExists
   ```

3. **Efficient Downloads**: Use `-AsBytes` when processing file content without saving to disk
   ```powershell
   $bytes = Get-DataverseFileData ... -AsBytes
   ```

4. **Error Handling**: Wrap operations in try/catch for robustness
   ```powershell
   try {
       Set-DataverseFileData ... -FilePath $path
   } catch {
       Write-Error "Failed to upload $path: $_"
   }
   ```

5. **Large Files**: File operations use 4MB blocks automatically for efficient transfer

## Differences from Notes/Annotations

File columns differ from traditional Notes/Annotations:

| Feature | File Columns | Notes/Annotations |
|---------|-------------|-------------------|
| Storage | Column data | Related records |
| API | Block-based | Attachment entity |
| Performance | Optimized | Lower |
| Query | Direct column | Join required |
| Cmdlets | Get/Set/Remove-DataverseFileData | Get/Set-DataverseRecord with annotations |

## See Also

- [Get-DataverseFileData](Get-DataverseFileData.md) - Download cmdlet reference
- [Set-DataverseFileData](Set-DataverseFileData.md) - Upload cmdlet reference
- [Remove-DataverseFileData](Remove-DataverseFileData.md) - Delete cmdlet reference
- [Creating and Updating Records](creating-updating.md) - General record operations
- [Microsoft Docs: File columns](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/file-column-data) - Official documentation
