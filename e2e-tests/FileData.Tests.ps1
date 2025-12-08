$ErrorActionPreference = "Stop"

Describe "File Data E2E Tests" {

    BeforeAll {
        if ($env:TESTMODULEPATH) {
            $source = $env:TESTMODULEPATH
        }
        else {
            $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
        }

        $tempmodulefolder = "$([IO.Path]::GetTempPath())/$([Guid]::NewGuid())"
        new-item -ItemType Directory $tempmodulefolder
        copy-item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
        $env:PSModulePath = $tempmodulefolder;
        $env:ChildProcessPSModulePath = $tempmodulefolder

        Import-Module Rnwood.Dataverse.Data.PowerShell
    }

    It "Can upload, download, and delete file data" {
            
        $ErrorActionPreference = "Stop"
        $ConfirmPreference = 'None'  # Suppress all confirmation prompts in non-interactive mode
        $VerbosePreference = 'Continue'  # Enable verbose output
        
            
        # Retry helper function with exponential backoff
        function Invoke-WithRetry {
            param(
                [Parameter(Mandatory = $true)]
                [scriptblock]$ScriptBlock,
                [int]$MaxRetries = 5,
                [int]$InitialDelaySeconds = 10
            )
                
            $attempt = 0
            $delay = $InitialDelaySeconds
                
            while ($attempt -lt $MaxRetries) {
                try {
                    $attempt++
                    Write-Verbose "Attempt $attempt of $MaxRetries"
                    & $ScriptBlock
                    return  # Success, exit function
                }
                catch {
                    # Check if this is an EntityCustomization operation error
                    if ($_.Exception.Message -like "*Cannot start the requested operation*EntityCustomization*") {
                        Write-Warning "EntityCustomization operation conflict detected. Waiting 2 minutes before retry without incrementing attempt count..."
                        $attempt--  # Don't count this as a retry attempt
                        Start-Sleep -Seconds 120
                        continue
                    }
                    
                    if ($attempt -eq $MaxRetries) {
                        Write-Error "All $MaxRetries attempts failed. Last error: $_"
                        throw
                    }
                        
                    Write-Warning "Attempt $attempt failed: $_. Retrying in $delay seconds..."
                    Start-Sleep -Seconds $delay
                    $delay = $delay * 2  # Exponential backoff
                }
            }
        }
            
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true    

            # Generate unique test identifiers with timestamp to enable age-based cleanup
            $timestamp = [DateTime]::UtcNow.ToString("yyyyMMddHHmmss")
            $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
            $entityName = "new_e2efile_${timestamp}_$testRunId"
            $schemaName = "new_E2EFile_${timestamp}_$testRunId"
                
            Write-Host "Test entity: $entityName"
                
            Write-Host "Step 1: Creating test entity with file column..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                
                Set-DataverseEntityMetadata -Connection $connection `
                    -EntityName $entityName `
                    -SchemaName $schemaName `
                    -DisplayName "File Test Entity" `
                    -DisplayCollectionName "File Test Entities" `
                    -PrimaryAttributeSchemaName "new_name" `
                    -OwnershipType UserOwned `
                    -Confirm:$false
            }
        
            Write-Host "✓ Test entity created"
                
            Write-Host "Step 2: Creating File column..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_document" `
                    -SchemaName "new_Document" `
                    -AttributeType File `
                    -DisplayName "Document" `
                    -MaxSizeInKB 32768 `
                    -Confirm:$false
            }
            Write-Host "✓ File column created"
                
            Write-Host "Step 3: Creating a test record..."
            $recordId = $null
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $record = @{
                    new_name = "Test File Record $testRunId"
                } | Set-DataverseRecord -Connection $connection -TableName $entityName -PassThru -Confirm:$false
                $script:recordId = $record.Id
            }
            Write-Host "✓ Test record created with ID: $recordId"
                
            # Create test file content
            $testContent = "This is a test file for E2E testing. Timestamp: $timestamp"
            $testBytes = [System.Text.Encoding]::UTF8.GetBytes($testContent)
            $testFileName = "test-document-$testRunId.txt"
                
            Write-Host "Step 4: Uploading file from byte array..."
            Invoke-WithRetry {
                Set-DataverseFileData -Connection $connection `
                    -TableName $entityName `
                    -Id $recordId `
                    -ColumnName "new_document" `
                    -FileContent $testBytes `
                    -FileName $testFileName `
                    -Confirm:$false
            }
            Write-Host "✓ File uploaded from byte array"
                
            Write-Host "Step 5: Downloading file as byte array..."
            $downloadedBytes = $null
            Invoke-WithRetry {
                $script:downloadedBytes = Get-DataverseFileData -Connection $connection `
                    -TableName $entityName `
                    -Id $recordId `
                    -ColumnName "new_document" `
                    -AsBytes
            }
            
            if ($null -eq $downloadedBytes -or $downloadedBytes.Length -eq 0) {
                throw "Downloaded file is empty or null"
            }
            
            $downloadedContent = [System.Text.Encoding]::UTF8.GetString($downloadedBytes)
            if ($downloadedContent -ne $testContent) {
                throw "Downloaded content does not match uploaded content. Expected: '$testContent', Got: '$downloadedContent'"
            }
            Write-Host "✓ File downloaded and content verified"
                
            Write-Host "Step 6: Downloading file to folder..."
            $tempFolder = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [guid]::NewGuid().ToString())
            New-Item -ItemType Directory -Path $tempFolder -Force | Out-Null
            
            $downloadedFile = $null
            Invoke-WithRetry {
                $script:downloadedFile = Get-DataverseFileData -Connection $connection `
                    -TableName $entityName `
                    -Id $recordId `
                    -ColumnName "new_document" `
                    -FolderPath $tempFolder
            }
            
            if (-not (Test-Path $downloadedFile.FullName)) {
                throw "Downloaded file does not exist at expected path: $($downloadedFile.FullName)"
            }
            
            $fileContent = Get-Content -Path $downloadedFile.FullName -Raw
            if ($fileContent -ne $testContent) {
                throw "File content does not match. Expected: '$testContent', Got: '$fileContent'"
            }
            Write-Host "✓ File downloaded to folder and verified: $($downloadedFile.Name)"
                
            # Clean up temp folder
            Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue
                
            Write-Host "Step 7: Uploading new file from path..."
            $newTestContent = "Updated file content for E2E testing. Timestamp: $timestamp"
            $tempFilePath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "test-upload-$testRunId.txt")
            $newTestContent | Out-File -FilePath $tempFilePath -NoNewline -Encoding UTF8
            
            Invoke-WithRetry {
                Set-DataverseFileData -Connection $connection `
                    -TableName $entityName `
                    -Id $recordId `
                    -ColumnName "new_document" `
                    -FilePath $tempFilePath `
                    -Confirm:$false
            }
            Write-Host "✓ New file uploaded from path"
                
            # Verify updated content
            Write-Host "Step 8: Verifying updated file content..."
            $updatedBytes = $null
            Invoke-WithRetry {
                $script:updatedBytes = Get-DataverseFileData -Connection $connection `
                    -TableName $entityName `
                    -Id $recordId `
                    -ColumnName "new_document" `
                    -AsBytes
            }
            
            $updatedContent = [System.Text.Encoding]::UTF8.GetString($updatedBytes)
            if ($updatedContent -ne $newTestContent) {
                throw "Updated content does not match. Expected: '$newTestContent', Got: '$updatedContent'"
            }
            Write-Host "✓ Updated file content verified"
                
            # Clean up temp file
            Remove-Item -Path $tempFilePath -Force -ErrorAction SilentlyContinue
                
            Write-Host "Step 9: Deleting file data..."
            Invoke-WithRetry {
                Remove-DataverseFileData -Connection $connection `
                    -TableName $entityName `
                    -Id $recordId `
                    -ColumnName "new_document" `
                    -Confirm:$false
            }
            Write-Host "✓ File data deleted"
                
            Write-Host "Step 10: Verifying file deletion (with IfExists)..."
            Invoke-WithRetry {
                # This should not throw error with IfExists
                Remove-DataverseFileData -Connection $connection `
                    -TableName $entityName `
                    -Id $recordId `
                    -ColumnName "new_document" `
                    -IfExists `
                    -Confirm:$false
            }
            Write-Host "✓ File deletion verified (IfExists worked correctly)"
                
            Write-Host "Step 11: Cleanup - Deleting test record..."
            Invoke-WithRetry {
                Remove-DataverseRecord -Connection $connection `
                    -TableName $entityName `
                    -Id $recordId `
                    -Confirm:$false
            }
            Write-Host "✓ Test record deleted"
                
            Write-Host "Step 12: Cleanup - Deleting test entity..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
            }
            Write-Host "✓ Test entity deleted"
                
            Write-Host "Step 13: Cleanup any old test entities from previous failed runs..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                
                # Only clean up entities older than 1 hour to avoid interfering with concurrent tests
                $cutoffTime = [DateTime]::UtcNow.AddHours(-1)
                $cutoffTimestamp = $cutoffTime.ToString("yyyyMMddHHmmss")
                
                $oldEntities = Get-DataverseEntityMetadata -Connection $connection | Where-Object { 
                    $_.LogicalName -like "new_e2efile_*" -and 
                    $_.LogicalName -ne $entityName -and
                    # Extract timestamp from name (format: new_e2efile_yyyyMMddHHmmss_guid)
                    # Only clean up if timestamp is old enough (older than 1 hour)
                    ($_.LogicalName -match "new_e2efile_(\d{14})_" -and $matches[1] -lt $cutoffTimestamp)
                }
                if ($oldEntities.Count -gt 0) {
                    Write-Host "  Found $($oldEntities.Count) old test entities (>1 hour old) to clean up"
                    foreach ($entity in $oldEntities) {
                        try {
                            Write-Host "  Removing old entity: $($entity.LogicalName)"
                            Invoke-WithRetry {
                                Wait-DataversePublish -Connection $connection -Verbose
                                Remove-DataverseEntityMetadata -Connection $connection -EntityName $entity.LogicalName -Confirm:$false -ErrorAction SilentlyContinue
                            }
                        }
                        catch {
                            Write-Host "  Could not remove $($entity.LogicalName): $_"
                        }
                    }
                }
                else {
                    Write-Host "  No old test entities found (>1 hour old)"
                }
            }
                
            Write-Host "SUCCESS: All file data operations completed successfully"
                
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }
}
