$ErrorActionPreference = "Stop"

Describe "Entity Metadata E2E Tests" {

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

    It "Can create, read, update, and delete custom entity with all features" {
            
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

            # Generate unique entity name with timestamp to enable age-based cleanup
            $timestamp = [DateTime]::UtcNow.ToString("yyyyMMddHHmm")
            $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
            $entityName = "new_e2etest_${timestamp}_$testRunId"
            $schemaName = "new_E2ETest_${timestamp}_$testRunId"
            $webResourceName1 = "svg_test_${timestamp}_$testRunId"
            $webResourceName2 = "svg_test_updated_${timestamp}_$testRunId"
                
            Write-Host "Test entity: $entityName"
            Write-Host "Web resources: $webResourceName1, $webResourceName2"
                
            Write-Host "Step 0: Creating required web resources for icon testing..."
                
            # Create a simple SVG content for testing
            $svgContent = @"
<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 16 16">
  <rect width="16" height="16" fill="#0078D4"/>
</svg>
"@
            $svgBase64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($svgContent))
                
            # Create web resource 1
    
            $webResource1 = @{
                name            = $webResourceName1
                displayname     = "E2E Test Icon $testRunId"
                webresourcetype = 11  # SVG
                content         = $svgBase64
            } | set-dataverserecord -Connection $connection -TableName webresource -Verbose
                    
            Write-Host "  ✓ Created web resource: $webResourceName1"
                 
            # Create web resource 2

            $webResource2 = @{
                name            = $webResourceName2
                displayname     = "E2E Test Icon Updated $testRunId"
                webresourcetype = 11  # SVG
                content         = $svgBase64
            } | set-dataverserecord -Connection $connection -TableName webresource -Verbose
                  
            Write-Host "  ✓ Created web resource: $webResourceName2"
              
            Write-Host "Step 1: Creating custom entity with all features..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                    
                Set-DataverseEntityMetadata `
                    -Connection $connection `
                    -EntityName $entityName `
                    -SchemaName $schemaName `
                    -DisplayName "E2E Test Entity" `
                    -DisplayCollectionName "E2E Test Entities" `
                    -Description "Entity created for E2E testing" `
                    -PrimaryAttributeSchemaName "new_name" `
                    -PrimaryAttributeDisplayName "Name" `
                    -PrimaryAttributeMaxLength 100 `
                    -OwnershipType UserOwned `
                    -HasActivities `
                    -HasNotes `
                    -IsAuditEnabled `
                    -ChangeTrackingEnabled `
                    -IconVectorName $webResourceName1 `
                    -Confirm:$false
            }
            Write-Host "✓ Entity created"
                
            Write-Host "Step 2: Reading entity metadata..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $script:entity = Get-DataverseEntityMetadata -Connection $connection -EntityName $entityName
                    
                if (-not $script:entity) {
                    throw "Failed to retrieve entity metadata"
                }
                if ($script:entity.DisplayName.UserLocalizedLabel.Label -ne "E2E Test Entity") {
                    throw "Display name mismatch"
                }
                if ($script:entity.IsAuditEnabled.Value -ne $true) {
                    throw "Audit not enabled"
                }
                if ($script:entity.ChangeTrackingEnabled -ne $true) {
                    throw "Change tracking not enabled"
                }
            }
            Write-Host "✓ Entity metadata verified"
                
            Write-Host "Step 3: Updating entity metadata..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                    
                Set-DataverseEntityMetadata `
                    -Connection $connection `
                    -EntityName $entityName `
                    -DisplayName "E2E Test Entity (Updated)" `
                    -Description "Updated description" `
                    -IconVectorName $webResourceName2 `
                    -Confirm:$false
            }
            Write-Host "✓ Entity updated"
                
            Write-Host "Step 4: Verifying update..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $script:updatedEntity = Get-DataverseEntityMetadata -Connection $connection -EntityName $entityName
                    
                if ($script:updatedEntity.DisplayName.UserLocalizedLabel.Label -ne "E2E Test Entity (Updated)") {
                    throw "Display name not updated"
                }
                if ($script:updatedEntity.IconVectorName -ne $webResourceName2) {
                    throw "Icon not updated"
                }
            }
            Write-Host "✓ Updates verified"
                
            Write-Host "Step 5: Testing EntityMetadata object update..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                    
                $script:entityObj = Get-DataverseEntityMetadata -Connection $connection -EntityName $entityName
                $script:entityObj.Description = New-Object Microsoft.Xrm.Sdk.Label
                $script:entityObj.Description.UserLocalizedLabel = New-Object Microsoft.Xrm.Sdk.LocalizedLabel("Bulk update test", 1033)
                    
                Set-DataverseEntityMetadata -Connection $connection -EntityMetadata $script:entityObj -Confirm:$false
            }
            Write-Host "✓ EntityMetadata object update complete"
                
            Write-Host "Step 6: Cleanup - Deleting entity..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                    
                Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
            }
            Write-Host "✓ Entity deleted"
                
            Write-Host "Step 7: Verifying deletion..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $script:deletedEntity = Get-DataverseEntityMetadata -Connection $connection | Where-Object { $_.LogicalName -eq $entityName }
                if ($script:deletedEntity) {
                    throw "Entity still exists after deletion"
                }
            }
            Write-Host "✓ Deletion verified"
                
            Write-Host "Step 8: Cleanup any old test entities from previous failed runs..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                
                # Only clean up entities older than 1 hour to avoid interfering with concurrent tests
                $cutoffTime = [DateTime]::UtcNow.AddHours(-1)
                $cutoffTimestamp = $cutoffTime.ToString("yyyyMMddHHmm")
                
                $oldEntities = Get-DataverseEntityMetadata -Connection $connection | Where-Object { 
                    $_.LogicalName -like "new_e2etest_*" -and 
                    $_.LogicalName -ne $entityName -and
                    # Extract timestamp from name (format: new_e2etest_yyyyMMddHHmm_guid)
                    # Only clean up if timestamp is old enough (older than 1 hour)
                    ($_.LogicalName -match "new_e2etest_(\d{12})_" -and $matches[1] -lt $cutoffTimestamp)
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
            
            Write-Host "Step 9: Cleanup web resources..."
            try {
                # Delete web resource 1
                Get-DataverseRecord -Connection $connection -TableName webresource -FilterValues @{ name = $webResourceName1 } | 
                    ForEach-Object { 
                        Remove-DataverseRecord -Connection $connection -TableName webresource -Id $_.webresourceid -Confirm:$false 
                        Write-Host "  ✓ Deleted web resource: $webResourceName1"
                    }
                
                # Delete web resource 2
                Get-DataverseRecord -Connection $connection -TableName webresource -FilterValues @{ name = $webResourceName2 } | 
                    ForEach-Object { 
                        Remove-DataverseRecord -Connection $connection -TableName webresource -Id $_.webresourceid -Confirm:$false 
                        Write-Host "  ✓ Deleted web resource: $webResourceName2"
                    }
                    
                # Cleanup old web resources from previous failed runs (older than 1 hour)
                $cutoffTime = [DateTime]::UtcNow.AddHours(-1)
                $cutoffTimestamp = $cutoffTime.ToString("yyyyMMddHHmm")
                
                $oldWebResources = Get-DataverseRecord -Connection $connection -TableName webresource -FilterValues @{ "name:Like" = "svg_test_%" } | 
                    Where-Object { 
                        $_.name -notin @($webResourceName1, $webResourceName2) -and
                        # Extract timestamp from name (format: svg_test_yyyyMMddHHmm_guid)
                        ($_.name -match "svg_test_(\d{12})_" -and $matches[1] -lt $cutoffTimestamp)
                    }
                
                if ($oldWebResources) {
                    Write-Host "  Found $(@($oldWebResources).Count) old test web resources (>1 hour old) to clean up"
                    foreach ($wr in $oldWebResources) {
                        try {
                            Remove-DataverseRecord -Connection $connection -TableName webresource -Id $wr.webresourceid -Confirm:$false -ErrorAction SilentlyContinue
                            Write-Host "  ✓ Deleted old web resource: $($wr.name)"
                        }
                        catch {
                            Write-Host "  Could not remove web resource $($wr.name): $_"
                        }
                    }
                }
            }
            catch {
                Write-Warning "Could not cleanup web resources: $_"
            }
                
            Write-Host "SUCCESS: All entity metadata operations completed successfully"
                
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Can create and delete custom activity entity" {
            
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

            # Generate unique entity name with timestamp to enable age-based cleanup
            $timestamp = [DateTime]::UtcNow.ToString("yyyyMMddHHmm")
            $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
            $activityEntityName = "new_e2eactivity_${timestamp}_$testRunId"
            $activitySchemaName = "new_E2EActivity_${timestamp}_$testRunId"
                
            Write-Host "Test activity entity: $activityEntityName"
              
            Write-Host "Step 1: Creating custom activity entity..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                    
                Set-DataverseEntityMetadata `
                    -Connection $connection `
                    -EntityName $activityEntityName `
                    -SchemaName $activitySchemaName `
                    -DisplayName "E2E Test Activity" `
                    -DisplayCollectionName "E2E Test Activities" `
                    -Description "Activity entity created for E2E testing" `
                    -PrimaryAttributeSchemaName "Subject" `
                    -PrimaryAttributeDisplayName "Subject" `
                    -PrimaryAttributeMaxLength 200 `
                    -OwnershipType UserOwned `
                    -IsActivity `
                    -Confirm:$false
            }
            Write-Host "✓ Activity entity created"
                
            Write-Host "Step 2: Reading activity entity metadata and verifying IsActivity is true..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $script:activityEntity = Get-DataverseEntityMetadata -Connection $connection -EntityName $activityEntityName
                    
                if (-not $script:activityEntity) {
                    throw "Failed to retrieve activity entity metadata"
                }
                if ($script:activityEntity.IsActivity -ne $true) {
                    throw "IsActivity should be true but was: $($script:activityEntity.IsActivity)"
                }
                if ($script:activityEntity.DisplayName.UserLocalizedLabel.Label -ne "E2E Test Activity") {
                    throw "Display name mismatch: expected 'E2E Test Activity' but got '$($script:activityEntity.DisplayName.UserLocalizedLabel.Label)'"
                }
            }
            Write-Host "✓ Activity entity metadata verified - IsActivity is true"
                
            Write-Host "Step 3: Cleanup - Deleting activity entity..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                    
                Remove-DataverseEntityMetadata -Connection $connection -EntityName $activityEntityName -Confirm:$false
            }
            Write-Host "✓ Activity entity deleted"
                
            Write-Host "Step 4: Verifying deletion..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $script:deletedActivityEntity = Get-DataverseEntityMetadata -Connection $connection | Where-Object { $_.LogicalName -eq $activityEntityName }
                if ($script:deletedActivityEntity) {
                    throw "Activity entity still exists after deletion"
                }
            }
            Write-Host "✓ Deletion verified"
                
            Write-Host "Step 5: Cleanup any old test activity entities from previous failed runs..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                
                # Only clean up entities older than 1 hour to avoid interfering with concurrent tests
                $cutoffTime = [DateTime]::UtcNow.AddHours(-1)
                $cutoffTimestamp = $cutoffTime.ToString("yyyyMMddHHmm")
                
                $oldActivityEntities = Get-DataverseEntityMetadata -Connection $connection | Where-Object { 
                    $_.LogicalName -like "new_e2eactivity_*" -and 
                    $_.LogicalName -ne $activityEntityName -and
                    # Extract timestamp from name (format: new_e2eactivity_yyyyMMddHHmm_guid)
                    # Only clean up if timestamp is old enough (older than 1 hour)
                    ($_.LogicalName -match "new_e2eactivity_(\d{12})_" -and $matches[1] -lt $cutoffTimestamp)
                }
                if ($oldActivityEntities.Count -gt 0) {
                    Write-Host "  Found $($oldActivityEntities.Count) old test activity entities (>1 hour old) to clean up"
                    foreach ($entity in $oldActivityEntities) {
                        try {
                            Write-Host "  Removing old activity entity: $($entity.LogicalName)"
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
                    Write-Host "  No old test activity entities found (>1 hour old)"
                }
            }
                
            Write-Host "SUCCESS: Activity entity operations completed successfully"
                
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }    
}
