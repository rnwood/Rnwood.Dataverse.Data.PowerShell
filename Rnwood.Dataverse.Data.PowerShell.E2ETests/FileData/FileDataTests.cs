using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.FileData
{
    /// <summary>
    /// File data manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/FileData.Tests.ps1
    /// </summary>
    public class FileDataTests : E2ETestBase
    {
        [Fact]
        public void CanUploadDownloadAndDeleteFileData()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmmss')
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $entityName = ""new_e2efile_${timestamp}_$testRunId""
    $schemaName = ""new_E2EFile_${timestamp}_$testRunId""
    
    Write-Host ""Creating test entity: $entityName""
    $entityMetadata = New-Object Microsoft.Xrm.Sdk.Metadata.EntityMetadata
    $entityMetadata.LogicalName = $entityName
    $entityMetadata.SchemaName = $schemaName
    $entityMetadata.DisplayName = New-Object Microsoft.Xrm.Sdk.Label(""E2E File Test Entity"", 1033)
    $entityMetadata.DisplayCollectionName = New-Object Microsoft.Xrm.Sdk.Label(""E2E File Test Entities"", 1033)
    $entityMetadata.OwnershipType = [Microsoft.Xrm.Sdk.Metadata.OwnershipTypes]::UserOwned
    $entityMetadata.IsActivity = $false
    
    $primaryAttribute = New-Object Microsoft.Xrm.Sdk.Metadata.StringAttributeMetadata
    $primaryAttribute.SchemaName = ""new_name""
    $primaryAttribute.DisplayName = New-Object Microsoft.Xrm.Sdk.Label(""Name"", 1033)
    $primaryAttribute.MaxLength = 100
    $primaryAttribute.RequiredLevel = New-Object Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevelManagedProperty([Microsoft.Xrm.Sdk.Metadata.AttributeRequiredLevel]::None)
    
    Set-DataverseEntityMetadata -EntityMetadata $entityMetadata -PrimaryAttribute $primaryAttribute -Confirm:`$false | Out-Null
    Wait-DataversePublish -Verbose
    
    Write-Host ""Adding file column to entity""
    $fileAttr = New-Object Microsoft.Xrm.Sdk.Metadata.FileAttributeMetadata
    $fileAttr.SchemaName = ""new_document""
    $fileAttr.DisplayName = New-Object Microsoft.Xrm.Sdk.Label(""Document"", 1033)
    $fileAttr.MaxSizeInKB = 32768
    
    Set-DataverseAttributeMetadata -EntityName $entityName -AttributeMetadata $fileAttr -Confirm:`$false | Out-Null
    Wait-DataversePublish -Verbose
    
    Write-Host ""Creating test record""
    $recordId = New-Guid
    $record = New-Object Microsoft.Xrm.Sdk.Entity($entityName)
    $record.Id = $recordId
    $record[""new_name""] = ""E2E Test Record $testRunId""
    Set-DataverseRecord -Record $record -Confirm:`$false | Out-Null
    
    Write-Host ""Test 1: Upload file via FilePath""
    $tempFile = [System.IO.Path]::GetTempFileName()
    'Test file content for E2E testing' | Out-File -FilePath $tempFile -Encoding utf8
    Set-DataverseFileData -TableName $entityName -Id $recordId -ColumnName 'new_document' -FilePath $tempFile -Confirm:`$false
    
    Write-Host ""Test 2: Download file via AsBytes""
    $downloadedBytes = Get-DataverseFileData -TableName $entityName -Id $recordId -ColumnName 'new_document' -AsBytes
    if ($null -eq $downloadedBytes -or $downloadedBytes.Length -eq 0) {
        throw 'Downloaded bytes are empty'
    }
    Write-Host ""✓ Downloaded $($downloadedBytes.Length) bytes""
    
    Write-Host ""Test 3: Upload via FileContent (byte array)""
    $uploadContent = 'Byte array upload test content'
    $uploadBytes = [System.Text.Encoding]::UTF8.GetBytes($uploadContent)
    Set-DataverseFileData -TableName $entityName -Id $recordId -ColumnName 'new_document' -FileContent $uploadBytes -FileName 'test.txt' -Confirm:`$false
    
    Write-Host ""Test 4: Download via AsByteStream""
    $streamBytes = @(Get-DataverseFileData -TableName $entityName -Id $recordId -ColumnName 'new_document' -AsByteStream)
    if ($null -eq $streamBytes -or $streamBytes.Count -eq 0) {
        throw 'Byte stream is empty'
    }
    Write-Host ""✓ Downloaded $($streamBytes.Count) bytes via stream""
    
    $streamContent = [System.Text.Encoding]::UTF8.GetString([byte[]]$streamBytes)
    if ($streamContent -ne $uploadContent) {
        throw ""Content mismatch. Expected: '$uploadContent', Got: '$streamContent'""
    }
    Write-Host ""✓ Byte stream content verified""
    
    Write-Host ""Test 5: Remove file data""
    Remove-DataverseFileData -TableName $entityName -Id $recordId -ColumnName 'new_document' -Confirm:`$false
    Write-Host ""✓ File data removed""
    
    Write-Host ""Cleanup: Deleting test record and entity""
    Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
    Remove-DataverseRecord -TableName $entityName -Id $recordId -Confirm:`$false
    Wait-DataversePublish -Verbose
    Remove-DataverseEntityMetadata -EntityName $entityName -Confirm:`$false
    
    Write-Host 'SUCCESS: All file data operations completed'
}
catch {
    Write-Host ""ERROR: $($_ | Out-String)""
    throw
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }
    }
}
