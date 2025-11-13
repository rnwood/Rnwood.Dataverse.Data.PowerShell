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
    }

    It "Can create, read, update, and delete global option sets comprehensively" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            $ErrorActionPreference = "Stop"
            $ConfirmPreference = 'None'  # Suppress all confirmation prompts in non-interactive mode
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Generate unique test identifiers
                $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
                $optionSetName1 = "new_e2eoption1_$testRunId"
                $optionSetName2 = "new_e2eoption2_$testRunId"
                $entityName = "new_e2eoptent_$testRunId"
                $entitySchema = "new_E2EOptEnt_$testRunId"
                
                Write-Host "Test option sets: $optionSetName1, $optionSetName2"
                
                Write-Host "Step 1: Creating first global option set..."
                Set-DataverseOptionSetMetadata -Connection $connection `
                    -Name $optionSetName1 `
                    -DisplayName "E2E Test Priority" `
                    -Description "Priority levels for E2E testing" `
                    -Options @(
                        @{Value=1; Label='Low'; Description='Low priority'}
                        @{Value=2; Label='Medium'; Description='Medium priority'}
                        @{Value=3; Label='High'; Description='High priority'}
                        @{Value=4; Label='Critical'; Description='Critical priority'}
                    ) `
                    -Confirm:$false
                
                Write-Host "✓ Option set 1 created"
                
                # Additional wait to ensure customization lock is released
                Write-Host "Waiting for customization lock to be released..."
                Start-Sleep -Seconds 3
                
                Write-Host "Step 2: Creating second global option set..."
                Set-DataverseOptionSetMetadata -Connection $connection `
                    -Name $optionSetName2 `
                    -DisplayName "E2E Test Status" `
                    -Description "Status values for E2E testing" `
                    -Options @(
                        @{Value=100; Label='New'}
                        @{Value=200; Label='In Progress'}
                        @{Value=300; Label='Completed'}
                        @{Value=400; Label='Cancelled'}
                    ) `
                    -Confirm:$false
                
                Write-Host "✓ Option set 2 created"
                
                Write-Host "Step 3: Reading global option set..."
                $optionSet1 = Get-DataverseOptionSetMetadata -Connection $connection -Name $optionSetName1
                
                if (-not $optionSet1) {
                    throw "Failed to retrieve option set"
                }
                if ($optionSet1.DisplayName.UserLocalizedLabel.Label -ne "E2E Test Priority") {
                    throw "Option set display name mismatch"
                }
                if ($optionSet1.Options.Count -ne 4) {
                    throw "Expected 4 options, found $($optionSet1.Options.Count)"
                }
                Write-Host "✓ Option set 1 verified (4 options)"
                
                Write-Host "Step 4: Listing all global option sets..."
                $allOptionSets = Get-DataverseOptionSetMetadata -Connection $connection
                
                $ourOptionSets = $allOptionSets | Where-Object { 
                    $_.Name -eq $optionSetName1 -or $_.Name -eq $optionSetName2 
                }
                
                if ($ourOptionSets.Count -ne 2) {
                    throw "Expected 2 option sets, found $($ourOptionSets.Count)"
                }
                Write-Host "✓ Found both option sets in list ($($allOptionSets.Count) total)"
                
                # Additional wait to ensure customization lock is released
                Write-Host "Waiting for customization lock to be released..."
                Start-Sleep -Seconds 3
                
                Write-Host "Step 5: Creating test entity to use option sets..."
                Set-DataverseEntityMetadata -Connection $connection `
                    -EntityName $entityName `
                    -SchemaName $entitySchema `
                    -DisplayName "Option Set Test Entity" `
                    -DisplayCollectionName "Option Set Test Entities" `
                    -PrimaryAttributeSchemaName "new_name" `
                    -OwnershipType UserOwned `
                    -Confirm:$false
                Write-Host "✓ Test entity created"
                
                Write-Host "Step 6: Creating picklist attribute using global option set..."
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_priority" `
                    -SchemaName "new_Priority" `
                    -AttributeType Picklist `
                    -DisplayName "Priority Level" `
                    -OptionSetName $optionSetName1 `
                    -Confirm:$false
                
                Write-Host "✓ Picklist attribute created with global option set"
                
                Write-Host "Step 7: Creating another attribute using second option set..."
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_status" `
                    -SchemaName "new_Status" `
                    -AttributeType Picklist `
                    -DisplayName "Current Status" `
                    -OptionSetName $optionSetName2 `
                    -Confirm:$false
                
                Write-Host "✓ Second picklist attribute created"
                
                Write-Host "Step 8: Reading option set from attribute..."
                $priorityAttr = Get-DataverseAttributeMetadata -Connection $connection -EntityName $entityName -AttributeName "new_priority"
                
                if ($priorityAttr.OptionSet.Name -ne $optionSetName1) {
                    throw "Attribute not using correct global option set"
                }
                Write-Host "✓ Attribute correctly references global option set"
                
                Write-Host "Step 9: Getting option set values for entity attribute..."
                $priorityOptions = Get-DataverseOptionSetMetadata -Connection $connection -EntityName $entityName -AttributeName "new_priority"
                
                if ($priorityOptions.Options.Count -ne 4) {
                    throw "Expected 4 options from attribute, found $($priorityOptions.Options.Count)"
                }
                Write-Host "✓ Retrieved option values from entity attribute"
                
                Write-Host "Step 10: Updating global option set (adding new option)..."
                Set-DataverseOptionSetMetadata -Connection $connection `
                    -Name $optionSetName1 `
                    -DisplayName "E2E Test Priority (Updated)" `
                    -Options @(
                        @{Value=1; Label='Low'}
                        @{Value=2; Label='Medium'}
                        @{Value=3; Label='High'}
                        @{Value=4; Label='Critical'}
                        @{Value=5; Label='Emergency'}
                    ) `
                    -Confirm:$false
                
                Write-Host "✓ Option set updated with new option"
                
                Write-Host "Step 11: Verifying option set update..."
                $updatedOptionSet = Get-DataverseOptionSetMetadata -Connection $connection -Name $optionSetName1
                
                if ($updatedOptionSet.DisplayName.UserLocalizedLabel.Label -ne "E2E Test Priority (Updated)") {
                    throw "Display name not updated"
                }
                if ($updatedOptionSet.Options.Count -ne 5) {
                    throw "Expected 5 options after update, found $($updatedOptionSet.Options.Count)"
                }
                
                $emergencyOption = $updatedOptionSet.Options | Where-Object { $_.Value -eq 5 }
                if (-not $emergencyOption) {
                    throw "New 'Emergency' option not found"
                }
                Write-Host "✓ Update verified (5 options including new 'Emergency')"
                
                Write-Host "Step 12: Testing local option set (created with attribute)..."
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_category" `
                    -SchemaName "new_Category" `
                    -AttributeType Picklist `
                    -DisplayName "Category" `
                    -Options @(
                        @{Value=1; Label='Type A'}
                        @{Value=2; Label='Type B'}
                        @{Value=3; Label='Type C'}
                    ) `
                    -Confirm:$false
                
                Write-Host "✓ Local option set created with attribute"
                
                Write-Host "Step 13: Verifying local option set..."
                $categoryAttr = Get-DataverseAttributeMetadata -Connection $connection -EntityName $entityName -AttributeName "new_category"
                
                if ($categoryAttr.OptionSet.IsGlobal -eq $true) {
                    throw "Option set should be local, not global"
                }
                if ($categoryAttr.OptionSet.Options.Count -ne 3) {
                    throw "Expected 3 local options, found $($categoryAttr.OptionSet.Options.Count)"
                }
                Write-Host "✓ Local option set verified"
                
                Write-Host "Step 14: Cleanup - Deleting test entity (and local option set)..."
                Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
                Write-Host "✓ Test entity deleted"
                
                Write-Host "Step 15: Cleanup any old test entities from previous failed runs..."
                $oldEntities = Get-DataverseEntityMetadata -Connection $connection | Where-Object { 
                    $_.LogicalName -like "new_e2eoptent_*" -and 
                    $_.LogicalName -ne $entityName 
                }
                if ($oldEntities.Count -gt 0) {
                    Write-Host "  Found $($oldEntities.Count) old test entities to clean up"
                    foreach ($entity in $oldEntities) {
                        try {
                            Write-Host "  Removing old entity: $($entity.LogicalName)"
                            Remove-DataverseEntityMetadata -Connection $connection -EntityName $entity.LogicalName -Confirm:$false -ErrorAction SilentlyContinue
                        } catch {
                            Write-Host "  Could not remove $($entity.LogicalName): $_"
                        }
                    }
                } else {
                    Write-Host "  No old test entities found"
                }
                
                Write-Host "Step 16: Note on global option set cleanup..."
                Write-Host "  Global option sets ($optionSetName1, $optionSetName2) created"
                Write-Host "  Note: Global option sets typically require manual cleanup or solution management"
                Write-Host "  In production, they would be part of managed solutions"
                
                Write-Host "SUCCESS: All option set metadata operations completed successfully"
                
            } catch {
                Write-Host "ERROR: $($_ | Out-String)"
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
}
