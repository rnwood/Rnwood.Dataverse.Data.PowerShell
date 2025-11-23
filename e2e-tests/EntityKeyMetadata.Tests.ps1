$ErrorActionPreference = "Stop"

Describe "Entity Key Metadata E2E Tests" {

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

    It "Can create, read, and delete alternate keys" {
            
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
            $timestamp = [DateTime]::UtcNow.ToString("yyyyMMddHHmm")
            $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
            $entityName = "new_e2ekey_${timestamp}_$testRunId"
            $schemaName = "new_E2EKey_${timestamp}_$testRunId"
                
            Write-Host "Test entity: $entityName"
                
            Write-Host "Step 1: Creating test entity..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                
                Set-DataverseEntityMetadata -Connection $connection `
                    -EntityName $entityName `
                    -SchemaName $schemaName `
                    -DisplayName "Entity Key Test Entity" `
                    -DisplayCollectionName "Entity Key Test Entities" `
                    -PrimaryAttributeSchemaName "new_name" `
                    -OwnershipType UserOwned `
                    -Confirm:$false
            }
        
            Write-Host "✓ Test entity created"
                
            Write-Host "Step 2: Creating String attribute for key..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_email" `
                    -SchemaName "new_Email" `
                    -AttributeType String `
                    -DisplayName "Email Address" `
                    -StringFormat Email `
                    -MaxLength 100 `
                    -RequiredLevel None `
                    -Confirm:$false
            }
            Write-Host "✓ String attribute created"
                
            Write-Host "Step 3: Creating Integer attribute for composite key..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_code" `
                    -SchemaName "new_Code" `
                    -AttributeType Integer `
                    -DisplayName "Code" `
                    -MinValue 0 `
                    -MaxValue 999999 `
                    -RequiredLevel None `
                    -Confirm:$false
            }
            Write-Host "✓ Integer attribute created"
                
            Write-Host "Step 4: Creating single-attribute alternate key..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Set-DataverseEntityKeyMetadata -Connection $connection `
                    -EntityName $entityName `
                    -SchemaName "${entityName}_email_key" `
                    -DisplayName "Email Key" `
                    -KeyAttributes @("new_email") `
                    -Confirm:$false
            }
            Write-Host "✓ Single-attribute alternate key created"
                
            Write-Host "Step 5: Creating composite alternate key..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Set-DataverseEntityKeyMetadata -Connection $connection `
                    -EntityName $entityName `
                    -SchemaName "${entityName}_email_code_key" `
                    -DisplayName "Email Code Key" `
                    -KeyAttributes @("new_email", "new_code") `
                    -Confirm:$false
            }
            Write-Host "✓ Composite alternate key created"
                
            Write-Host "Step 6: Publishing entity to activate keys..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Publish-DataverseCustomizations -Connection $connection -Confirm:$false
            }
            Write-Host "✓ Entity published"
                
            Write-Host "Step 7: Reading and verifying all keys..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $keys = Get-DataverseEntityKeyMetadata -Connection $connection -EntityName $entityName
                
                if ($keys.Count -ne 2) {
                    throw "Expected 2 keys, found $($keys.Count)"
                }
                
                $emailKey = $keys | Where-Object { $_.LogicalName -like "*email_key" }
                if (-not $emailKey) {
                    throw "Email key not found"
                }
                if ($emailKey.KeyAttributes.Count -ne 1 -or $emailKey.KeyAttributes[0] -ne "new_email") {
                    throw "Email key has incorrect attributes"
                }
                
                $compositeKey = $keys | Where-Object { $_.LogicalName -like "*email_code_key" }
                if (-not $compositeKey) {
                    throw "Composite key not found"
                }
                if ($compositeKey.KeyAttributes.Count -ne 2) {
                    throw "Composite key should have 2 attributes, found $($compositeKey.KeyAttributes.Count)"
                }
            }
            Write-Host "✓ All keys verified (2 found)"
                
            Write-Host "Step 8: Reading specific key..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $specificKey = Get-DataverseEntityKeyMetadata -Connection $connection -EntityName $entityName -KeyName "${entityName}_email_key"
                
                if (-not $specificKey) {
                    throw "Specific key not found"
                }
                if ($specificKey.KeyAttributes.Count -ne 1) {
                    throw "Expected 1 attribute in specific key"
                }
            }
            Write-Host "✓ Specific key retrieved successfully"
                
            Write-Host "Step 9: Testing key deletion..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Remove-DataverseEntityKeyMetadata -Connection $connection -EntityName $entityName -KeyName "${entityName}_email_key" -Confirm:$false
            }
            Write-Host "✓ Email key deleted"
                
            Write-Host "Step 10: Verifying deletion..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $remainingKeys = Get-DataverseEntityKeyMetadata -Connection $connection -EntityName $entityName
                if ($remainingKeys.Count -ne 1) {
                    throw "Expected 1 remaining key, found $($remainingKeys.Count)"
                }
                $deletedKey = $remainingKeys | Where-Object { $_.LogicalName -like "*email_key" }
                if ($deletedKey) {
                    throw "Deleted key still exists"
                }
            }
            Write-Host "✓ Deletion verified (1 key remaining)"
                
            Write-Host "Step 11: Cleanup - Deleting test entity..."
            try {
                Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false -ErrorAction Stop
                }
                Write-Host "✓ Test entity deleted"
            }
            catch {
                # Entity may already be deleted or not exist
                Write-Host "  Note: Could not delete test entity (may already be deleted): $_"
            }
                
            Write-Host "Step 12: Cleanup any old test entities from previous failed runs..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                
                # Only clean up entities older than 1 hour to avoid interfering with concurrent tests
                $cutoffTime = [DateTime]::UtcNow.AddHours(-1)
                $cutoffTimestamp = $cutoffTime.ToString("yyyyMMddHHmm")
                
                $oldEntities = Get-DataverseEntityMetadata -Connection $connection | Where-Object { 
                    $_.LogicalName -like "new_e2ekey_*" -and 
                    $_.LogicalName -ne $entityName -and
                    # Extract timestamp from name (format: new_e2ekey_yyyyMMddHHmm_guid)
                    # Only clean up if timestamp is old enough (older than 1 hour)
                    ($_.LogicalName -match "new_e2ekey_(\d{12})_" -and $matches[1] -lt $cutoffTimestamp)
                }
                if ($oldEntities.Count -gt 0) {
                    Write-Host "  Found $($oldEntities.Count) old test entities (>1 hour old) to clean up"
                    foreach ($entity in $oldEntities) {
                        try {
                            Write-Host "  Removing old entity: $($entity.LogicalName)"
                            # Use ErrorAction Stop to ensure we catch entity-not-found errors
                            Remove-DataverseEntityMetadata -Connection $connection -EntityName $entity.LogicalName -Confirm:$false -ErrorAction Stop
                        }
                        catch {
                            # Entity may have been deleted by another process or doesn't exist
                            # This is not a failure - just log and continue
                            if ($_.Exception.Message -like "*could not find*" -or $_.Exception.Message -like "*does not exist*") {
                                Write-Host "  Entity $($entity.LogicalName) already deleted or doesn't exist"
                            }
                            else {
                                Write-Host "  Could not remove $($entity.LogicalName): $_"
                            }
                        }
                    }
                }
                else {
                    Write-Host "  No old test entities found (>1 hour old)"
                }
            }
                
            Write-Host "SUCCESS: All entity key metadata operations completed successfully"
                
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }
}
