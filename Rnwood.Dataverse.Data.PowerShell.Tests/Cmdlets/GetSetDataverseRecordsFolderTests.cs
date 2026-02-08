using Xunit;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using FluentAssertions;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

/// <summary>
/// Tests for Get-DataverseRecordsFolder and Set-DataverseRecordsFolder script module cmdlets.
/// These are PowerShell script modules (.psm1), so tests must execute in a child PowerShell process.
/// </summary>
public class GetSetDataverseRecordsFolderTests : TestBase
{
    private string? _tempFolder;
    
    private string CreateTempFolder()
    {
        _tempFolder = Path.Combine(Path.GetTempPath(), $"DataverseRecordsFolderTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempFolder);
        return _tempFolder;
    }

    public void Dispose()
    {
        if (_tempFolder != null && Directory.Exists(_tempFolder))
        {
            try { Directory.Delete(_tempFolder, true); } catch { /* Ignore cleanup errors */ }
        }
    }

    [Fact]
    public void DataverseRecordsFolder_RoundTripSerialization_WritesAndReadsCorrectly()
    {
        // Arrange
        var folder = CreateTempFolder();
        var script = $@"
$record = [PSCustomObject]@{{
    Id = [Guid]'00000000-0000-0000-0000-000000000001'
    TableName = 'contact'
    firstname = 'John'
    lastname = 'Doe'
}}

# Write record to folder
$record | Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'

# Read records back
$results = Get-DataverseRecordsFolder -InputPath '{folder.Replace("'", "''")}'
$results | ConvertTo-Json -Depth 10
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
        result.StandardOutput.Should().Contain("John");
        result.StandardOutput.Should().Contain("Doe");
    }

    [Fact]
    public void DataverseRecordsFolder_CreatesOneJsonFilePerRecord()
    {
        // Arrange
        var folder = CreateTempFolder();
        var script = $@"
$records = @(
    [PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000001'; TableName = 'contact'; name = 'Record1' }},
    [PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000002'; TableName = 'contact'; name = 'Record2' }},
    [PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000003'; TableName = 'contact'; name = 'Record3' }}
)

$records | Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'

# Count JSON files
(Get-ChildItem -Path '{folder.Replace("'", "''")}' -Filter '*.json').Count
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
        result.StandardOutput.Trim().Should().Be("3");
    }

    [Fact]
    public void DataverseRecordsFolder_HandlesEmptyFolderGracefully()
    {
        // Arrange
        var folder = CreateTempFolder();
        var script = $@"
$results = @(Get-DataverseRecordsFolder -InputPath '{folder.Replace("'", "''")}')
$results.Count
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
        result.StandardOutput.Trim().Should().Be("0");
    }

    [Fact]
    public void DataverseRecordsFolder_PreservesComplexDataTypes()
    {
        // Arrange
        var folder = CreateTempFolder();
        var script = $@"
$record = [PSCustomObject]@{{
    Id = [Guid]'00000000-0000-0000-0000-000000000001'
    TableName = 'contact'
    birthdate = '1990-05-15T00:00:00'
    revenue = 12345.67
    isactive = $true
    primarycontactid = [PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000099'; LogicalName = 'account' }}
}}

$record | Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'
$results = Get-DataverseRecordsFolder -InputPath '{folder.Replace("'", "''")}'
$results | ConvertTo-Json -Depth 10
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
        result.StandardOutput.Should().Contain("1990");  // Date preserved
        result.StandardOutput.Should().Contain("12345"); // Decimal preserved
        result.StandardOutput.Should().Contain("true");  // Boolean preserved
    }

    [Fact]
    public void DataverseRecordsFolder_SyncsBehavior_RemovesFilesNotInCurrentBatch()
    {
        // Arrange - Set-DataverseRecordsFolder syncs the folder to match the current batch,
        // removing files that aren't in the current batch (unless -withdeletions is used)
        var folder = CreateTempFolder();
        var script = $@"
# First write - creates file for record 1
[PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000001'; TableName = 'contact'; name = 'First' }} | 
    Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'

$afterFirst = (Get-ChildItem -Path '{folder.Replace("'", "''")}' -Filter '*.json').Count

# Second write with different record - removes record 1, creates record 2
[PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000002'; TableName = 'contact'; name = 'Second' }} | 
    Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'

$afterSecond = (Get-ChildItem -Path '{folder.Replace("'", "''")}' -Filter '*.json').Count

# Verify: first call creates 1 file, second call also results in 1 file (previous was removed)
Write-Output ""After first: $afterFirst, After second: $afterSecond""
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
        result.StandardOutput.Should().Contain("After first: 1");
        result.StandardOutput.Should().Contain("After second: 1");
    }

    [Fact]
    public void DataverseRecordsFolder_HandlesMultipleTableTypes()
    {
        // Arrange
        var folder = CreateTempFolder();
        var script = $@"
$records = @(
    [PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000001'; TableName = 'contact'; name = 'Contact1' }},
    [PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000002'; TableName = 'account'; name = 'Account1' }}
)

$records | Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'

# Read all records
$all = @(Get-DataverseRecordsFolder -InputPath '{folder.Replace("'", "''")}')
$contacts = @($all | Where-Object {{ $_.TableName -eq 'contact' }})
$accounts = @($all | Where-Object {{ $_.TableName -eq 'account' }})

Write-Output ""Contacts: $($contacts.Count), Accounts: $($accounts.Count)""
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
        result.StandardOutput.Should().Contain("Contacts: 1");
        result.StandardOutput.Should().Contain("Accounts: 1");
    }

    [Fact]
    public void DataverseRecordsFolder_UsesRecordIdInFilename()
    {
        // Arrange
        var folder = CreateTempFolder();
        var testId = "11111111-1111-1111-1111-111111111111";
        var script = $@"
[PSCustomObject]@{{ Id = [Guid]'{testId}'; TableName = 'contact'; name = 'Test' }} | 
    Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'

# Check filename contains ID
$files = Get-ChildItem -Path '{folder.Replace("'", "''")}' -Filter '*.json'
$files.Name -match '{testId}'
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
        result.StandardOutput.Should().Contain("True");
    }

    [Fact]
    public void DataverseRecordsFolder_PreservesRecordMetadata()
    {
        // Arrange
        var folder = CreateTempFolder();
        var script = $@"
$record = [PSCustomObject]@{{
    Id = [Guid]'00000000-0000-0000-0000-000000000001'
    TableName = 'contact'
    customfield = 'value'
}}

$record | Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'
$results = @(Get-DataverseRecordsFolder -InputPath '{folder.Replace("'", "''")}')

# Check metadata is preserved
Write-Output ""Id: $($results[0].Id)""
Write-Output ""TableName: $($results[0].TableName)""
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
        result.StandardOutput.Should().Contain("00000000-0000-0000-0000-000000000001");
        result.StandardOutput.Should().Contain("TableName: contact");
    }
}
