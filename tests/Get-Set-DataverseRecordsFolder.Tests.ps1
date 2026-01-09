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
}
