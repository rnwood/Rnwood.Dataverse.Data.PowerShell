. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseFileData' {
    It "Downloads file data to file path" {
        $downloadCalled = $false
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.RequestName -eq "InitializeFileBlocksDownload") {
                $response = New-Object Microsoft.Crm.Sdk.Messages.InitializeFileBlocksDownloadResponse
                $response.Results["FileContinuationToken"] = "test-token"
                $response.Results["FileSizeInBytes"] = [long]100
                $response.Results["FileName"] = "test.txt"
                $response.Results["IsChunkingSupported"] = $true
                return $response
            }
            
            if ($request.RequestName -eq "DownloadBlock") {
                $script:downloadCalled = $true
                $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                $response.ResponseName = "DownloadBlock"
                $response.Results["Data"] = [System.Text.Encoding]::UTF8.GetBytes("Test content")
                return $response
            }
        }
        
        $testFile = "$([IO.Path]::GetTempPath())test-download.txt"
        
        try {
            $result = Get-DataverseFileData -Connection $connection -TableName "account" -Id ([Guid]::NewGuid()) -ColumnName "file1" -FilePath $testFile
            
            $result | Should -Not -BeNullOrEmpty
            $result.FullName | Should -Be $testFile
            Test-Path $testFile | Should -Be $true
            
            $content = Get-Content $testFile -Raw
            $content | Should -Be "Test content"
            $downloadCalled | Should -Be $true
        }
        finally {
            if (Test-Path $testFile) {
                Remove-Item $testFile -Force
            }
        }
    }

    It "Downloads file data to folder with auto-generated filename" {
        $downloadCalled = $false
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.RequestName -eq "InitializeFileBlocksDownload") {
                $response = New-Object Microsoft.Crm.Sdk.Messages.InitializeFileBlocksDownloadResponse
                $response.Results["FileContinuationToken"] = "test-token"
                $response.Results["FileSizeInBytes"] = [long]100
                $response.Results["FileName"] = "autoname.txt"
                $response.Results["IsChunkingSupported"] = $true
                return $response
            }
            
            if ($request.RequestName -eq "DownloadBlock") {
                $script:downloadCalled = $true
                $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                $response.ResponseName = "DownloadBlock"
                $response.Results["Data"] = [System.Text.Encoding]::UTF8.GetBytes("Auto test")
                return $response
            }
        }
        
        $testFolder = [IO.Path]::GetTempPath()
        $expectedFile = Join-Path $testFolder "autoname.txt"
        
        try {
            $result = Get-DataverseFileData -Connection $connection -TableName "account" -Id ([Guid]::NewGuid()) -ColumnName "file1" -FolderPath $testFolder
            
            $result | Should -Not -BeNullOrEmpty
            $result.Name | Should -Be "autoname.txt"
            Test-Path $expectedFile | Should -Be $true
            
            $content = Get-Content $expectedFile -Raw
            $content | Should -Be "Auto test"
            $downloadCalled | Should -Be $true
        }
        finally {
            if (Test-Path $expectedFile) {
                Remove-Item $expectedFile -Force
            }
        }
    }

    It "Returns file data as byte array" {
        $downloadCalled = $false
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.RequestName -eq "InitializeFileBlocksDownload") {
                $response = New-Object Microsoft.Crm.Sdk.Messages.InitializeFileBlocksDownloadResponse
                $response.Results["FileContinuationToken"] = "test-token"
                $response.Results["FileSizeInBytes"] = [long]100
                $response.Results["FileName"] = "bytes.bin"
                $response.Results["IsChunkingSupported"] = $true
                return $response
            }
            
            if ($request.RequestName -eq "DownloadBlock") {
                $script:downloadCalled = $true
                $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                $response.ResponseName = "DownloadBlock"
                $response.Results["Data"] = [System.Text.Encoding]::UTF8.GetBytes("Byte data")
                return $response
            }
        }
        
        $result = Get-DataverseFileData -Connection $connection -TableName "account" -Id ([Guid]::NewGuid()) -ColumnName "file1" -AsBytes
        
        $result | Should -Not -BeNullOrEmpty
        $result | Should -BeOfType [byte[]]
        $resultString = [System.Text.Encoding]::UTF8.GetString($result)
        $resultString | Should -Be "Byte data"
        $downloadCalled | Should -Be $true
    }

    It "Handles empty file" {
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.RequestName -eq "InitializeFileBlocksDownload") {
                $response = New-Object Microsoft.Crm.Sdk.Messages.InitializeFileBlocksDownloadResponse
                $response.Results["FileContinuationToken"] = "test-token"
                $response.Results["FileSizeInBytes"] = [long]0
                $response.Results["FileName"] = "empty.txt"
                $response.Results["IsChunkingSupported"] = $true
                return $response
            }
        }
        
        $testFile = "$([IO.Path]::GetTempPath())test-empty.txt"
        
        try {
            $result = Get-DataverseFileData -Connection $connection -TableName "account" -Id ([Guid]::NewGuid()) -ColumnName "file1" -FilePath $testFile -WarningAction SilentlyContinue
            
            # Should not create file for empty file
            Test-Path $testFile | Should -Be $false
        }
        finally {
            if (Test-Path $testFile) {
                Remove-Item $testFile -Force
            }
        }
    }
}

Describe 'Set-DataverseFileData' {
    It "Uploads file from file path" {
        $uploadedData = $null
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.RequestName -eq "InitializeFileBlocksUpload") {
                $response = New-Object Microsoft.Crm.Sdk.Messages.InitializeFileBlocksUploadResponse
                $response.Results["FileContinuationToken"] = "upload-token"
                return $response
            }
            
            if ($request.RequestName -eq "UploadBlock") {
                $script:uploadedData = $request.Parameters["BlockData"]
                return New-Object Microsoft.Xrm.Sdk.OrganizationResponse
            }
            
            if ($request.RequestName -eq "CommitFileBlocksUpload") {
                return New-Object Microsoft.Crm.Sdk.OrganizationResponse
            }
        }
        
        $testFile = "$([IO.Path]::GetTempPath())test-upload.txt"
        "Upload test content" | Out-File -FilePath $testFile -NoNewline
        
        try {
            Set-DataverseFileData -Connection $connection -TableName "account" -Id ([Guid]::NewGuid()) -ColumnName "file1" -FilePath $testFile -Confirm:$false
            
            $uploadedData | Should -Not -BeNullOrEmpty
            $uploadedString = [System.Text.Encoding]::UTF8.GetString($uploadedData)
            $uploadedString | Should -Be "Upload test content"
        }
        finally {
            if (Test-Path $testFile) {
                Remove-Item $testFile -Force
            }
        }
    }

    It "Uploads file from byte array" {
        $uploadedData = $null
        $uploadedFileName = $null
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.RequestName -eq "InitializeFileBlocksUpload") {
                $script:uploadedFileName = $request.Parameters["FileName"]
                $response = New-Object Microsoft.Crm.Sdk.Messages.InitializeFileBlocksUploadResponse
                $response.Results["FileContinuationToken"] = "upload-token"
                return $response
            }
            
            if ($request.RequestName -eq "UploadBlock") {
                $script:uploadedData = $request.Parameters["BlockData"]
                return New-Object Microsoft.Xrm.Sdk.OrganizationResponse
            }
            
            if ($request.RequestName -eq "CommitFileBlocksUpload") {
                return New-Object Microsoft.Crm.Sdk.OrganizationResponse
            }
        }
        
        $bytes = [System.Text.Encoding]::UTF8.GetBytes("Byte upload")
        Set-DataverseFileData -Connection $connection -TableName "account" -Id ([Guid]::NewGuid()) -ColumnName "file1" -FileContent $bytes -FileName "custom.bin" -Confirm:$false
        
        $uploadedData | Should -Not -BeNullOrEmpty
        $uploadedString = [System.Text.Encoding]::UTF8.GetString($uploadedData)
        $uploadedString | Should -Be "Byte upload"
        $uploadedFileName | Should -Be "custom.bin"
    }

    It "Supports WhatIf" {
        $wasUploaded = $false
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.RequestName -eq "InitializeFileBlocksUpload") {
                $script:wasUploaded = $true
                $response = New-Object Microsoft.Crm.Sdk.Messages.InitializeFileBlocksUploadResponse
                $response.Results["FileContinuationToken"] = "upload-token"
                return $response
            }
        }
        
        $testFile = "$([IO.Path]::GetTempPath())test-whatif.txt"
        "WhatIf test" | Out-File -FilePath $testFile -NoNewline
        
        try {
            Set-DataverseFileData -Connection $connection -TableName "account" -Id ([Guid]::NewGuid()) -ColumnName "file1" -FilePath $testFile -WhatIf
            
            $wasUploaded | Should -Be $false
        }
        finally {
            if (Test-Path $testFile) {
                Remove-Item $testFile -Force
            }
        }
    }
}

Describe 'Remove-DataverseFileData' {
    It "Deletes file data" {
        $wasDeleted = $false
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.RequestName -eq "Retrieve") {
                $entity = New-Object Microsoft.Xrm.Sdk.Entity("account")
                $entity.Id = $request.Target.Id
                $entity["file1"] = [Guid]::NewGuid()
                return $entity
            }
            
            if ($request.RequestName -eq "DeleteFile") {
                $script:wasDeleted = $true
                return New-Object Microsoft.Xrm.Sdk.OrganizationResponse
            }
        }
        
        Remove-DataverseFileData -Connection $connection -TableName "account" -Id ([Guid]::NewGuid()) -ColumnName "file1" -Confirm:$false
        
        $wasDeleted | Should -Be $true
    }

    It "Supports IfExists flag when file doesn't exist" {
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.RequestName -eq "Retrieve") {
                $entity = New-Object Microsoft.Xrm.Sdk.Entity("account")
                $entity.Id = $request.Target.Id
                # Don't include file1 column
                return $entity
            }
        }
        
        # Should not throw error with IfExists
        { Remove-DataverseFileData -Connection $connection -TableName "account" -Id ([Guid]::NewGuid()) -ColumnName "file1" -IfExists -Confirm:$false } | Should -Not -Throw
    }

    It "Supports WhatIf" {
        $wasDeleted = $false
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.RequestName -eq "Retrieve") {
                $entity = New-Object Microsoft.Xrm.Sdk.Entity("account")
                $entity.Id = $request.Target.Id
                $entity["file1"] = [Guid]::NewGuid()
                return $entity
            }
            
            if ($request.RequestName -eq "DeleteFile") {
                $script:wasDeleted = $true
                return New-Object Microsoft.Xrm.Sdk.OrganizationResponse
            }
        }
        
        Remove-DataverseFileData -Connection $connection -TableName "account" -Id ([Guid]::NewGuid()) -ColumnName "file1" -WhatIf
        
        $wasDeleted | Should -Be $false
    }
}

Describe 'File cmdlets - Piping from Get-DataverseRecord' {
    It "Can pipe record with Id and TableName to Get-DataverseFileData" {
        $downloadCalled = $false
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.RequestName -eq "InitializeFileBlocksDownload") {
                $response = New-Object Microsoft.Crm.Sdk.Messages.InitializeFileBlocksDownloadResponse
                $response.Results["FileContinuationToken"] = "test-token"
                $response.Results["FileSizeInBytes"] = [long]50
                $response.Results["FileName"] = "piped.txt"
                $response.Results["IsChunkingSupported"] = $true
                return $response
            }
            
            if ($request.RequestName -eq "DownloadBlock") {
                $script:downloadCalled = $true
                $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                $response.ResponseName = "DownloadBlock"
                $response.Results["Data"] = [System.Text.Encoding]::UTF8.GetBytes("Piped")
                return $response
            }
        }
        
        $record = [PSCustomObject]@{
            Id = [Guid]::NewGuid()
            TableName = "account"
        }
        
        $result = $record | Get-DataverseFileData -Connection $connection -ColumnName "file1" -AsBytes
        
        $result | Should -Not -BeNullOrEmpty
        $resultString = [System.Text.Encoding]::UTF8.GetString($result)
        $resultString | Should -Be "Piped"
        $downloadCalled | Should -Be $true
    }

    It "Can pipe record to Set-DataverseFileData" {
        $uploadedForRecordId = $null
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.RequestName -eq "InitializeFileBlocksUpload") {
                $script:uploadedForRecordId = $request.Target.Id
                $response = New-Object Microsoft.Crm.Sdk.Messages.InitializeFileBlocksUploadResponse
                $response.Results["FileContinuationToken"] = "upload-token"
                return $response
            }
            
            if ($request.RequestName -eq "UploadBlock") {
                return New-Object Microsoft.Xrm.Sdk.OrganizationResponse
            }
            
            if ($request.RequestName -eq "CommitFileBlocksUpload") {
                return New-Object Microsoft.Xrm.Sdk.OrganizationResponse
            }
        }
        
        $recordId = [Guid]::NewGuid()
        $record = [PSCustomObject]@{
            Id = $recordId
            TableName = "account"
        }
        
        $bytes = [System.Text.Encoding]::UTF8.GetBytes("test")
        $record | Set-DataverseFileData -Connection $connection -ColumnName "file1" -FileContent $bytes -Confirm:$false
        
        $uploadedForRecordId | Should -Be $recordId
    }

    It "Can pipe record to Remove-DataverseFileData" {
        $deletedForRecordId = $null
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.RequestName -eq "Retrieve") {
                $script:deletedForRecordId = $request.Target.Id
                $entity = New-Object Microsoft.Xrm.Sdk.Entity("account")
                $entity.Id = $request.Target.Id
                $entity["file1"] = [Guid]::NewGuid()
                return $entity
            }
            
            if ($request.RequestName -eq "DeleteFile") {
                return New-Object Microsoft.Xrm.Sdk.OrganizationResponse
            }
        }
        
        $recordId = [Guid]::NewGuid()
        $record = [PSCustomObject]@{
            Id = $recordId
            TableName = "account"
        }
        
        $record | Remove-DataverseFileData -Connection $connection -ColumnName "file1" -Confirm:$false
        
        $deletedForRecordId | Should -Be $recordId
    }
}
