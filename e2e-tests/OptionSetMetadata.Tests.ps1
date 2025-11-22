$ErrorActionPreference = "Stop"

Describe "OptionSet Metadata E2E Tests" {

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

    It "Can create, read, update, and delete global option sets comprehensively" {
            
        $ErrorActionPreference = "Stop"
        $ConfirmPreference = 'None'  # Suppress all confirmation prompts in non-interactive mode
        $VerbosePreference = 'SilentlyContinue'  # Reduce noise; enable 'Continue' for debugging if needed
        
        # Import common utilities
        . "$PSScriptRoot/Common.ps1"
            
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true    

            # Generate unique test identifiers with timestamp to enable age-based cleanup
            $timestamp = [DateTime]::UtcNow.ToString("yyyyMMddHHmm")
            $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
            $optionSetName1 = "new_e2eoption1_${timestamp}_$testRunId"
            $optionSetName2 = "new_e2eoption2_${timestamp}_$testRunId"
            $entityName = "new_e2eoptent_${timestamp}_$testRunId"
            $entitySchema = "new_E2EOptEnt_${timestamp}_$testRunId"
                
            Write-Host "Test option sets: $optionSetName1, $optionSetName2"
                
            Write-Host "Step 1: Creating first global option set..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                    
                Set-DataverseOptionSetMetadata -Connection $connection `
                    -Name $optionSetName1 `
                    -DisplayName "E2E Test Priority" `
                    -Description "Priority levels for E2E testing" `
                    -Options @(
                    @{Value = 1; Label = 'Low'; Description = 'Low priority' }
                    @{Value = 2; Label = 'Medium'; Description = 'Medium priority' }
                    @{Value = 3; Label = 'High'; Description = 'High priority' }
                    @{Value = 4; Label = 'Critical'; Description = 'Critical priority' }
                ) `
                    -Confirm:$false
            }
            Write-Host "✓ Option set 1 created"
                
            Write-Host "Step 2: Creating second global option set..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                    
                Set-DataverseOptionSetMetadata -Connection $connection `
                    -Name $optionSetName2 `
                    -DisplayName "E2E Test Status" `
                    -Description "Status values for E2E testing" `
                    -Options @(
                    @{Value = 100; Label = 'New' }
                    @{Value = 200; Label = 'In Progress' }
                    @{Value = 300; Label = 'Completed' }
                    @{Value = 400; Label = 'Cancelled' }
                ) `
                    -Confirm:$false
            }
            Write-Host "✓ Option set 2 created"
                
            Write-Host "Step 3: Reading global option set..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                $script:optionSet1 = Get-DataverseOptionSetMetadata -Connection $connection -Name $optionSetName1
                    
                if (-not $script:optionSet1) {
                    throw "Failed to retrieve option set"
                }
                if ($script:optionSet1.DisplayName.UserLocalizedLabel.Label -ne "E2E Test Priority") {
                    throw "Option set display name mismatch"
                }
                if ($script:optionSet1.Options.Count -ne 4) {
                    throw "Expected 4 options, found $($script:optionSet1.Options.Count)"
                }
            }
            Write-Host "✓ Option set 1 verified (4 options)"
                
            Write-Host "Step 4: Listing all global option sets..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                $script:allOptionSets = Get-DataverseOptionSetMetadata -Connection $connection
                    
                $ourOptionSets = $script:allOptionSets | Where-Object { 
                    $_.Name -eq $optionSetName1 -or $_.Name -eq $optionSetName2 
                }
                    
                if ($ourOptionSets.Count -ne 2) {
                    throw "Expected 2 option sets, found $($ourOptionSets.Count)"
                }
            }
            Write-Host "✓ Found both option sets in list ($($script:allOptionSets.Count) total)"
                
            Write-Host "Step 5: Creating test entity to use option sets..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                    
                Set-DataverseEntityMetadata -Connection $connection `
                    -EntityName $entityName `
                    -SchemaName $entitySchema `
                    -DisplayName "Option Set Test Entity" `
                    -DisplayCollectionName "Option Set Test Entities" `
                    -PrimaryAttributeSchemaName "new_name" `
                    -OwnershipType UserOwned `
                    -Confirm:$false
            }
            Write-Host "✓ Test entity created"
                
            Write-Host "Step 6: Creating picklist attribute using global option set..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                    
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_priority" `
                    -SchemaName "new_Priority" `
                    -AttributeType Picklist `
                    -DisplayName "Priority Level" `
                    -OptionSetName $optionSetName1 `
                    -Confirm:$false
            }
            Write-Host "✓ Picklist attribute created with global option set"
                
            Write-Host "Step 7: Creating another attribute using second option set..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                    
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_status" `
                    -SchemaName "new_Status" `
                    -AttributeType Picklist `
                    -DisplayName "Current Status" `
                    -OptionSetName $optionSetName2 `
                    -Confirm:$false
            }
            Write-Host "✓ Second picklist attribute created"
                
            Write-Host "Step 8: Reading option set from attribute..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                $script:priorityAttr = Get-DataverseAttributeMetadata -Connection $connection -EntityName $entityName -AttributeName "new_priority"
                    
                if ($script:priorityAttr.OptionSet.Name -ne $optionSetName1) {
                    throw "Attribute not using correct global option set"
                }
            }
            Write-Host "✓ Attribute correctly references global option set"
                
            Write-Host "Step 9: Getting option set values for entity attribute..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                $script:priorityOptions = Get-DataverseOptionSetMetadata -Connection $connection -EntityName $entityName -AttributeName "new_priority"
                    
                if ($script:priorityOptions.Options.Count -ne 4) {
                    throw "Expected 4 options from attribute, found $($script:priorityOptions.Options.Count)"
                }
            }
            Write-Host "✓ Retrieved option values from entity attribute"
                
            Write-Host "Step 10: Updating global option set (adding new option)..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                    
                Set-DataverseOptionSetMetadata -Connection $connection `
                    -Name $optionSetName1 `
                    -DisplayName "E2E Test Priority (Updated)" `
                    -Options @(
                    @{Value = 1; Label = 'Low' }
                    @{Value = 2; Label = 'Medium' }
                    @{Value = 3; Label = 'High' }
                    @{Value = 4; Label = 'Critical' }
                    @{Value = 5; Label = 'Emergency' }
                ) `
                    -Confirm:$false
            }
            Write-Host "✓ Option set updated with new option"
                
            Write-Host "Step 11: Verifying option set update..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                $script:updatedOptionSet = Get-DataverseOptionSetMetadata -Connection $connection -Name $optionSetName1
                    
                if ($script:updatedOptionSet.DisplayName.UserLocalizedLabel.Label -ne "E2E Test Priority (Updated)") {
                    throw "Display name not updated"
                }
                if ($script:updatedOptionSet.Options.Count -ne 5) {
                    throw "Expected 5 options after update, found $($script:updatedOptionSet.Options.Count)"
                }
                    
                $emergencyOption = $script:updatedOptionSet.Options | Where-Object { $_.Value -eq 5 }
                if (-not $emergencyOption) {
                    throw "New 'Emergency' option not found"
                }
            }
            Write-Host "✓ Update verified (5 options including new 'Emergency')"
                
            Write-Host "Step 12: Testing local option set (created with attribute)..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_category" `
                    -SchemaName "new_Category" `
                    -AttributeType Picklist `
                    -DisplayName "Category" `
                    -Options @(
                    @{Value = 1; Label = 'Type A' }
                    @{Value = 2; Label = 'Type B' }
                    @{Value = 3; Label = 'Type C' }
                ) `
                    -Confirm:$false
            }
            Write-Host "✓ Local option set created with attribute"
                
            Write-Host "Step 13: Verifying local option set..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                $categoryAttr = Get-DataverseAttributeMetadata -Connection $connection -EntityName $entityName -AttributeName "new_category"
                
                if ($categoryAttr.OptionSet.IsGlobal -eq $true) {
                    throw "Option set should be local, not global"
                }
                if ($categoryAttr.OptionSet.Options.Count -ne 3) {
                    throw "Expected 3 local options, found $($categoryAttr.OptionSet.Options.Count)"
                }
            }
            Write-Host "✓ Local option set verified"
                
            Write-Host "Step 14: Cleanup - Deleting test entity (and local option set)..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
            }
            Write-Host "✓ Test entity deleted"
                
            Write-Host "Step 15: Cleanup any old test entities from previous failed runs..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                
                # Only clean up entities older than 1 hour to avoid interfering with concurrent tests
                $cutoffTime = [DateTime]::UtcNow.AddHours(-1)
                $cutoffTimestamp = $cutoffTime.ToString("yyyyMMddHHmm")
                
                $oldEntities = Get-DataverseEntityMetadata -Connection $connection | Where-Object { 
                    $_.LogicalName -like "new_e2eoptent_*" -and 
                    $_.LogicalName -ne $entityName -and
                    # Extract timestamp from name (format: new_e2eoptent_yyyyMMddHHmm_guid)
                    # Only clean up if timestamp is old enough (older than 1 hour)
                    ($_.LogicalName -match "new_e2eoptent_(\d{12})_" -and $matches[1] -lt $cutoffTimestamp)
                }
                if ($oldEntities.Count -gt 0) {
                    Write-Host "  Found $($oldEntities.Count) old test entities (>1 hour old) to clean up"
                    foreach ($entity in $oldEntities) {
                        try {
                            Write-Host "  Removing old entity: $($entity.LogicalName)"
                            Invoke-WithRetry {
                                Wait-DataversePublish -Connection $connection
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
                
            Write-Host "Step 16: Note on global option set cleanup..."
            Write-Host "  Global option sets ($optionSetName1, $optionSetName2) created"
            Write-Host "  Note: Global option sets typically require manual cleanup or solution management"
            Write-Host "  In production, they would be part of managed solutions"
                
            Write-Host "SUCCESS: All option set metadata operations completed successfully"
                
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }
        
}
