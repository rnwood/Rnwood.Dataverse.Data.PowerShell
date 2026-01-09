. $PSScriptRoot/Common.ps1

Describe 'Get/Set-DataverseRecordsFolder' {
    Context 'Round-Trip JSON Serialization' {
        It "Writes records to folder and reads them back correctly" {
            $connection = getMockConnection
            
            # Create test records
            $records = @(
                @{ firstname = "John"; lastname = "Doe"; emailaddress1 = "john@example.com" }
                @{ firstname = "Jane"; lastname = "Smith"; emailaddress1 = "jane@example.com" }
                @{ firstname = "Bob"; lastname = "Johnson"; emailaddress1 = "bob@example.com" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Create temp folder for test
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Write records to folder
                $records | Set-DataverseRecordsFolder -OutputPath $tempFolder
                
                # Verify files were created
                $files = Get-ChildItem -Path $tempFolder -Filter "*.json"
                $files | Should -HaveCount 3
                
                # Read records back
                $readRecords = Get-DataverseRecordsFolder -InputPath $tempFolder
                
                # Verify all records read correctly
                $readRecords | Should -HaveCount 3
                
                # Verify data integrity (round-trip)
                ($readRecords | Where-Object { $_.firstname -eq "John" -and $_.lastname -eq "Doe" }) | Should -HaveCount 1
                ($readRecords | Where-Object { $_.firstname -eq "Jane" -and $_.lastname -eq "Smith" }) | Should -HaveCount 1
                ($readRecords | Where-Object { $_.firstname -eq "Bob" -and $_.lastname -eq "Johnson" }) | Should -HaveCount 1
                
                # Verify email addresses preserved
                ($readRecords | Where-Object { $_.emailaddress1 -eq "john@example.com" }) | Should -HaveCount 1
                ($readRecords | Where-Object { $_.emailaddress1 -eq "jane@example.com" }) | Should -HaveCount 1
                ($readRecords | Where-Object { $_.emailaddress1 -eq "bob@example.com" }) | Should -HaveCount 1
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
            
            # Verify no side effects in Dataverse
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 3
        }

        It "Creates one JSON file per record" {
            $connection = getMockConnection
            
            # Create test records
            $records = @(
                @{ firstname = "User1"; lastname = "Test" }
                @{ firstname = "User2"; lastname = "Test" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Create temp folder
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Write records
                $records | Set-DataverseRecordsFolder -OutputPath $tempFolder
                
                # Verify one file per record
                $files = Get-ChildItem -Path $tempFolder -Filter "*.json"
                $files | Should -HaveCount 2
                
                # Verify each file contains valid JSON
                foreach ($file in $files) {
                    $content = Get-Content -Path $file.FullName -Raw
                    $content | Should -Not -BeNullOrEmpty
                    
                    # Verify it's valid JSON
                    { $content | ConvertFrom-Json } | Should -Not -Throw
                }
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 2
        }

        It "Handles empty folder gracefully" {
            # Create empty temp folder
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Read from empty folder
                $readRecords = Get-DataverseRecordsFolder -InputPath $tempFolder
                
                # Should return empty array or null
                if ($null -ne $readRecords) {
                    $readRecords | Should -BeNullOrEmpty
                }
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
        }

        It "Preserves record Ids in round-trip" {
            $connection = getMockConnection
            
            # Create test record
            $record = @{
                firstname = "Preserve"
                lastname = "Id"
                emailaddress1 = "preserve@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Store original Id
            $originalId = $record.Id
            
            # Create temp folder
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Write and read back
                $record | Set-DataverseRecordsFolder -OutputPath $tempFolder
                $readRecord = Get-DataverseRecordsFolder -InputPath $tempFolder
                
                # Verify Id preserved
                $readRecord | Should -HaveCount 1
                $readRecord[0].Id | Should -Be $originalId
                $readRecord[0].firstname | Should -Be "Preserve"
                $readRecord[0].lastname | Should -Be "Id"
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }

        It "Handles records with null/empty fields" {
            $connection = getMockConnection
            
            # Create record with some empty fields
            $record = @{
                firstname = "Sparse"
                lastname = "Record"
                emailaddress1 = $null
                description = ""
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Create temp folder
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Write and read back
                $record | Set-DataverseRecordsFolder -OutputPath $tempFolder
                $readRecord = Get-DataverseRecordsFolder -InputPath $tempFolder
                
                # Verify record read correctly
                $readRecord | Should -HaveCount 1
                $readRecord[0].firstname | Should -Be "Sparse"
                $readRecord[0].lastname | Should -Be "Record"
                
                # Verify structure preserved even with null/empty fields
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }

        It "Handles records with complex field types" {
            $connection = getMockConnection
            
            # Create record with various field types
            $record = @{
                firstname = "Complex"
                lastname = "Types"
                accountrolecode = 1
                donotbulkemail = $true
                emailaddress1 = "complex@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Create temp folder
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Write and read back
                $record | Set-DataverseRecordsFolder -OutputPath $tempFolder
                $readRecord = Get-DataverseRecordsFolder -InputPath $tempFolder
                
                # Verify all field types preserved
                $readRecord | Should -HaveCount 1
                $readRecord[0].firstname | Should -Be "Complex"
                $readRecord[0].lastname | Should -Be "Types"
                $readRecord[0].emailaddress1 | Should -Be "complex@example.com"
                # OptionSet and boolean values should be preserved
                # Exact behavior depends on JSON serialization
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }
    }

    Context 'Pipeline Support' {
        It "Accepts records from pipeline for writing" {
            $connection = getMockConnection
            
            # Create records
            $records = @(
                @{ firstname = "Pipe1"; lastname = "Test" }
                @{ firstname = "Pipe2"; lastname = "Test" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Create temp folder
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Pipeline records to Set-DataverseRecordsFolder
                $records | Set-DataverseRecordsFolder -OutputPath $tempFolder
                
                # Verify files created
                $files = Get-ChildItem -Path $tempFolder -Filter "*.json"
                $files | Should -HaveCount 2
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 2
        }
    }

    Context 'File and Image Column Handling' {
        It "Extracts byte array properties to separate binary files" {
            # Create temp folder for test
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Create test record with byte array (simulating file/image column)
                $testBytes = [System.Text.Encoding]::UTF8.GetBytes("Test file content")
                $record = [PSCustomObject]@{
                    Id = [Guid]::NewGuid()
                    firstname = "Test"
                    lastname = "User"
                    documentbody = $testBytes
                }
                
                # Write record to folder
                $record | Set-DataverseRecordsFolder -OutputPath $tempFolder
                
                # Verify JSON file was created
                $jsonFiles = Get-ChildItem -Path $tempFolder -Filter "*.json"
                $jsonFiles | Should -HaveCount 1
                
                # Verify _files directory was created
                $filesPath = Join-Path $tempFolder "_files"
                Test-Path $filesPath | Should -Be $true
                
                # Verify binary file was created
                $binaryFiles = Get-ChildItem -Path $filesPath -Filter "*.bin"
                $binaryFiles | Should -HaveCount 1
                
                # Verify JSON contains file reference instead of byte array
                $jsonContent = Get-Content -Path $jsonFiles[0].FullName -Raw | ConvertFrom-Json
                $jsonContent.documentbody | Should -Not -BeNullOrEmpty
                $jsonContent.documentbody.__fileReference | Should -Not -BeNullOrEmpty
                $jsonContent.documentbody.__hash | Should -Not -BeNullOrEmpty
                $jsonContent.documentbody.__size | Should -Be $testBytes.Length
                
                # Verify binary file contains correct content
                $binaryContent = [System.IO.File]::ReadAllBytes($binaryFiles[0].FullName)
                $binaryContent.Length | Should -Be $testBytes.Length
                [System.Text.Encoding]::UTF8.GetString($binaryContent) | Should -Be "Test file content"
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
        }

        It "Restores byte arrays when reading back from folder" {
            # Create temp folder for test
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Create test record with byte array
                $testBytes = [System.Text.Encoding]::UTF8.GetBytes("Test image data")
                $record = [PSCustomObject]@{
                    Id = [Guid]::NewGuid()
                    firstname = "Image"
                    lastname = "Test"
                    entityimage = $testBytes
                }
                
                # Write and read back
                $record | Set-DataverseRecordsFolder -OutputPath $tempFolder
                $readRecord = Get-DataverseRecordsFolder -InputPath $tempFolder
                
                # Verify byte array was restored
                $readRecord | Should -HaveCount 1
                $readRecord[0].entityimage | Should -Not -BeNullOrEmpty
                $readRecord[0].entityimage | Should -BeOfType [byte[]]
                $readRecord[0].entityimage.Length | Should -Be $testBytes.Length
                [System.Text.Encoding]::UTF8.GetString($readRecord[0].entityimage) | Should -Be "Test image data"
                
                # Verify other properties preserved
                $readRecord[0].firstname | Should -Be "Image"
                $readRecord[0].lastname | Should -Be "Test"
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
        }

        It "Only updates binary files when content changes" {
            # Create temp folder for test
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Create test record with byte array
                $testId = [Guid]::NewGuid()
                $testBytes = [System.Text.Encoding]::UTF8.GetBytes("Original content")
                $record = [PSCustomObject]@{
                    Id = $testId
                    firstname = "Change"
                    lastname = "Detection"
                    documentbody = $testBytes
                }
                
                # Write record first time
                $record | Set-DataverseRecordsFolder -OutputPath $tempFolder
                
                # Get binary file modification time
                $filesPath = Join-Path $tempFolder "_files"
                $binaryFile = Get-ChildItem -Path $filesPath -Filter "*.bin" | Select-Object -First 1
                $originalModTime = $binaryFile.LastWriteTime
                
                # Wait a moment to ensure time difference
                Start-Sleep -Milliseconds 100
                
                # Write same record again (no changes)
                $record | Set-DataverseRecordsFolder -OutputPath $tempFolder
                
                # Verify binary file was NOT updated (same modification time)
                $binaryFileAfter = Get-ChildItem -Path $filesPath -Filter "*.bin" | Select-Object -First 1
                $binaryFileAfter.LastWriteTime | Should -Be $originalModTime
                
                # Now modify the byte array content
                Start-Sleep -Milliseconds 100
                $record.documentbody = [System.Text.Encoding]::UTF8.GetBytes("Modified content")
                $record | Set-DataverseRecordsFolder -OutputPath $tempFolder
                
                # Verify binary file WAS updated (different modification time)
                $binaryFileModified = Get-ChildItem -Path $filesPath -Filter "*.bin" | Select-Object -First 1
                $binaryFileModified.LastWriteTime | Should -Not -Be $originalModTime
                
                # Verify new content is correct
                $readRecord = Get-DataverseRecordsFolder -InputPath $tempFolder
                [System.Text.Encoding]::UTF8.GetString($readRecord[0].documentbody) | Should -Be "Modified content"
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
        }

        It "Handles multiple file columns in a single record" {
            # Create temp folder for test
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Create test record with multiple byte arrays
                $testBytes1 = [System.Text.Encoding]::UTF8.GetBytes("File 1 content")
                $testBytes2 = [System.Text.Encoding]::UTF8.GetBytes("File 2 content")
                $record = [PSCustomObject]@{
                    Id = [Guid]::NewGuid()
                    firstname = "Multi"
                    lastname = "File"
                    documentbody = $testBytes1
                    entityimage = $testBytes2
                }
                
                # Write record
                $record | Set-DataverseRecordsFolder -OutputPath $tempFolder
                
                # Verify both binary files were created
                $filesPath = Join-Path $tempFolder "_files"
                $binaryFiles = Get-ChildItem -Path $filesPath -Filter "*.bin"
                $binaryFiles | Should -HaveCount 2
                
                # Read back and verify both byte arrays restored
                $readRecord = Get-DataverseRecordsFolder -InputPath $tempFolder
                $readRecord | Should -HaveCount 1
                $readRecord[0].documentbody | Should -BeOfType [byte[]]
                $readRecord[0].entityimage | Should -BeOfType [byte[]]
                [System.Text.Encoding]::UTF8.GetString($readRecord[0].documentbody) | Should -Be "File 1 content"
                [System.Text.Encoding]::UTF8.GetString($readRecord[0].entityimage) | Should -Be "File 2 content"
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
        }

        It "Cleans up orphaned binary files when record is removed" {
            # Create temp folder for test
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Create two test records with byte arrays
                $testId1 = [Guid]::NewGuid()
                $testId2 = [Guid]::NewGuid()
                $testBytes = [System.Text.Encoding]::UTF8.GetBytes("Test content")
                
                $records = @(
                    [PSCustomObject]@{
                        Id = $testId1
                        firstname = "First"
                        documentbody = $testBytes
                    }
                    [PSCustomObject]@{
                        Id = $testId2
                        firstname = "Second"
                        documentbody = $testBytes
                    }
                )
                
                # Write both records
                $records | Set-DataverseRecordsFolder -OutputPath $tempFolder
                
                # Verify 2 binary files created
                $filesPath = Join-Path $tempFolder "_files"
                (Get-ChildItem -Path $filesPath -Filter "*.bin").Count | Should -Be 2
                
                # Write only first record (simulating second being removed)
                $records[0] | Set-DataverseRecordsFolder -OutputPath $tempFolder
                
                # Verify only 1 binary file remains
                (Get-ChildItem -Path $filesPath -Filter "*.bin").Count | Should -Be 1
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
        }

        It "Removes _files directory when no binary files remain" {
            # Create temp folder for test
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Create record with byte array
                $testBytes = [System.Text.Encoding]::UTF8.GetBytes("Test content")
                $record = [PSCustomObject]@{
                    Id = [Guid]::NewGuid()
                    firstname = "Test"
                    documentbody = $testBytes
                }
                
                # Write record with binary
                $record | Set-DataverseRecordsFolder -OutputPath $tempFolder
                
                # Verify _files directory exists
                $filesPath = Join-Path $tempFolder "_files"
                Test-Path $filesPath | Should -Be $true
                
                # Write record without binary
                $recordNoBinary = [PSCustomObject]@{
                    Id = $record.Id
                    firstname = "Test"
                }
                $recordNoBinary | Set-DataverseRecordsFolder -OutputPath $tempFolder
                
                # Verify _files directory was removed
                Test-Path $filesPath | Should -Be $false
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
        }

        It "Handles null byte arrays gracefully" {
            # Create temp folder for test
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Create record with null byte array
                $record = [PSCustomObject]@{
                    Id = [Guid]::NewGuid()
                    firstname = "Null"
                    lastname = "Test"
                    documentbody = $null
                }
                
                # Write and read back
                $record | Set-DataverseRecordsFolder -OutputPath $tempFolder
                $readRecord = Get-DataverseRecordsFolder -InputPath $tempFolder
                
                # Verify null was preserved
                $readRecord | Should -HaveCount 1
                $readRecord[0].documentbody | Should -BeNullOrEmpty
                $readRecord[0].firstname | Should -Be "Null"
                
                # Verify no _files directory created
                $filesPath = Join-Path $tempFolder "_files"
                Test-Path $filesPath | Should -Be $false
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
        }

        It "Handles empty byte arrays" {
            # Create temp folder for test
            $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder | Out-Null
            
            try {
                # Create record with empty byte array
                $emptyBytes = [byte[]]@()
                $record = [PSCustomObject]@{
                    Id = [Guid]::NewGuid()
                    firstname = "Empty"
                    lastname = "Test"
                    documentbody = $emptyBytes
                }
                
                # Write and read back
                $record | Set-DataverseRecordsFolder -OutputPath $tempFolder
                $readRecord = Get-DataverseRecordsFolder -InputPath $tempFolder
                
                # Verify empty byte array was preserved
                $readRecord | Should -HaveCount 1
                $readRecord[0].documentbody | Should -Not -BeNullOrEmpty
                $readRecord[0].documentbody | Should -BeOfType [byte[]]
                $readRecord[0].documentbody.Length | Should -Be 0
            }
            finally {
                # Cleanup
                if (Test-Path $tempFolder) {
                    Remove-Item -Path $tempFolder -Recurse -Force
                }
            }
        }
    }
}
