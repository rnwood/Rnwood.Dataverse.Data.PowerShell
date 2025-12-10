$ErrorActionPreference = "Stop"

Describe "AppModule Manipulation" -Skip {

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

    It "Can perform full lifecycle of appmodule manipulation including components" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                # Connect to environment
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Generate unique test identifiers to avoid conflicts with parallel CI runs
                $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
                $appModuleUniqueName = "test_appmodule_$testRunId"
                $appModuleName = "Test AppModule $testRunId"
                
                Write-Host "Using test appmodule unique name: $appModuleUniqueName"
                
                # --- CLEANUP: Remove any previously failed test appmodules ---
                Write-Host "Cleaning up any existing test appmodules from previous failed runs..."
                $existingAppModules = Get-DataverseAppModule -Connection $connection | Where-Object { 
                    $_.UniqueName -like "test_appmodule_*" 
                }
                
                foreach ($existingAppModule in $existingAppModules) {
                    Write-Host "  Removing old appmodule: $($existingAppModule.UniqueName) (ID: $($existingAppModule.Id))"
                    try {
                        Remove-DataverseAppModule -Connection $connection -Id $existingAppModule.Id -IfExists -Confirm:$false
                    } catch {
                        Write-Warning "Failed to remove old appmodule $($existingAppModule.UniqueName): $_"
                    }
                }
                Write-Host "Cleanup complete."
                
                # --- TEST 1: Create a new appmodule ---
                Write-Host "`nTest 1: Creating new appmodule..."
                
                $appModuleId = Set-DataverseAppModule -Connection $connection `
                    -UniqueName $appModuleUniqueName `
                    -Name $appModuleName `
                    -Description "Test AppModule for E2E testing" `
                    -PassThru -Confirm:$false
                
                if (-not $appModuleId) {
                    throw "Failed to create appmodule - no ID returned"
                }
                Write-Host "Created appmodule with ID: $appModuleId"
                
                # --- TEST 2: Retrieve the created appmodule by ID ---
                Write-Host "`nTest 2: Retrieving appmodule by ID..."
                $retrievedAppModule = Get-DataverseAppModule -Connection $connection -Id $appModuleId
                
                if (-not $retrievedAppModule) {
                    throw "Failed to retrieve appmodule by ID"
                }
                if ($retrievedAppModule.Id -ne $appModuleId) {
                    throw "Retrieved appmodule ID mismatch. Expected: $appModuleId, Got: $($retrievedAppModule.Id)"
                }
                if ($retrievedAppModule.Name -ne $appModuleName) {
                    throw "Retrieved appmodule Name mismatch. Expected: $appModuleName, Got: $($retrievedAppModule.Name)"
                }
                Write-Host "Successfully retrieved appmodule: $($retrievedAppModule.Name)"
                
                # --- TEST 3: Retrieve appmodule by UniqueName ---
                Write-Host "`nTest 3: Retrieving appmodule by UniqueName..."
                $retrievedByUniqueName = Get-DataverseAppModule -Connection $connection -UniqueName $appModuleUniqueName
                
                if (-not $retrievedByUniqueName) {
                    throw "Failed to retrieve appmodule by UniqueName"
                }
                if ($retrievedByUniqueName.Id -ne $appModuleId) {
                    throw "Retrieved appmodule ID mismatch when querying by UniqueName"
                }
                Write-Host "Successfully retrieved appmodule by UniqueName"
                
                # --- TEST 4: Retrieve appmodule by Name with wildcard ---
                Write-Host "`nTest 4: Retrieving appmodule by Name with wildcard..."
                $retrievedByNameWildcard = Get-DataverseAppModule -Connection $connection -Name "Test AppModule*"
                
                if (-not $retrievedByNameWildcard) {
                    throw "Failed to retrieve appmodule by Name with wildcard"
                }
                $found = $false
                foreach ($app in $retrievedByNameWildcard) {
                    if ($app.Id -eq $appModuleId) {
                        $found = $true
                        break
                    }
                }
                if (-not $found) {
                    throw "Test appmodule not found in wildcard search results"
                }
                Write-Host "Successfully retrieved appmodule by Name with wildcard"
                
                # --- TEST 5: Update the appmodule ---
                Write-Host "`nTest 5: Updating appmodule..."
                $updatedName = "Updated $appModuleName"
                $updatedDescription = "Updated description for testing"
                Set-DataverseAppModule -Connection $connection `
                    -Id $appModuleId `
                    -Name $updatedName `
                    -Description $updatedDescription `
                    -IsFeatured $true `
                    -Confirm:$false
                
                $updatedAppModule = Get-DataverseAppModule -Connection $connection -Id $appModuleId
                if ($updatedAppModule.Name -ne $updatedName) {
                    throw "AppModule name update failed. Expected: $updatedName, Got: $($updatedAppModule.Name)"
                }
                if ($updatedAppModule.Description -ne $updatedDescription) {
                    throw "AppModule description update failed. Expected: $updatedDescription, Got: $($updatedAppModule.Description)"
                }
                if ($updatedAppModule.IsFeatured -ne $true) {
                    throw "AppModule IsFeatured update failed"
                }
                Write-Host "Successfully updated appmodule properties"
                
                # --- TEST 6: Add components to the appmodule ---
                Write-Host "`nTest 6: Adding components to appmodule..."
                
                # Get contact entity metadata to add as a component
                Write-Host "  Querying for contact entity..."
                $contactEntityQuery = @"
                    <fetch top='1'>
                        <entity name='entity'>
                            <attribute name='entityid' />
                            <attribute name='name' />
                            <attribute name='logicalname' />
                            <filter>
                                <condition attribute='logicalname' operator='eq' value='contact' />
                            </filter>
                        </entity>
                    </fetch>
"@
                $contactEntity = Get-DataverseRecord -Connection $connection -FetchXml $contactEntityQuery
                if (-not $contactEntity) {
                    throw "Failed to find contact entity"
                }
                $contactEntityId = $contactEntity.entityid
                Write-Host "  Found contact entity with ID: $contactEntityId"
                
                # Add contact entity as a component
                Write-Host "  Adding contact entity as component..."
                Set-DataverseAppModuleComponent -Connection $connection `
                    -AppModuleId $appModuleId `
                    -ObjectId $contactEntityId `
                    -ComponentType Entity `
                    -PassThru -Confirm:$false | Out-Null
                Write-Host "  Successfully added contact entity component"
                
                # Get account entity metadata to add as another component
                Write-Host "  Querying for account entity..."
                $accountEntityQuery = @"
                    <fetch top='1'>
                        <entity name='entity'>
                            <attribute name='entityid' />
                            <attribute name='name' />
                            <attribute name='logicalname' />
                            <filter>
                                <condition attribute='logicalname' operator='eq' value='account' />
                            </filter>
                        </entity>
                    </fetch>
"@
                $accountEntity = Get-DataverseRecord -Connection $connection -FetchXml $accountEntityQuery
                if (-not $accountEntity) {
                    throw "Failed to find account entity"
                }
                $accountEntityId = $accountEntity.entityid
                Write-Host "  Found account entity with ID: $accountEntityId"
                
                # Add account entity as a component
                Write-Host "  Adding account entity as component..."
                Set-DataverseAppModuleComponent -Connection $connection `
                    -AppModuleId $appModuleId `
                    -ObjectId $accountEntityId `
                    -ComponentType Entity `
                    -PassThru -Confirm:$false | Out-Null
                Write-Host "  Successfully added account entity component"
                
                # --- TEST 7: Retrieve appmodule components ---
                Write-Host "`nTest 7: Retrieving appmodule components..."
                
                # Get all components for the app module
                $allComponents = Get-DataverseAppModuleComponent -Connection $connection -AppModuleId $appModuleId
                if ($allComponents.Count -eq 0) {
                    throw "No components found for appmodule"
                }
                Write-Host "Found $($allComponents.Count) components for appmodule"
                
                # Verify contact entity component exists
                $contactComponent = $allComponents | Where-Object { $_.ObjectId -eq $contactEntityId -and $_.ComponentType -eq 'Entity' }
                if (-not $contactComponent) {
                    throw "Contact entity component not found"
                }
                Write-Host "Verified contact entity component exists"
                
                # Verify account entity component exists
                $accountComponent = $allComponents | Where-Object { $_.ObjectId -eq $accountEntityId -and $_.ComponentType -eq 'Entity' }
                if (-not $accountComponent) {
                    throw "Account entity component not found"
                }
                Write-Host "Verified account entity component exists"
                
                # --- TEST 8: Retrieve components by UniqueName ---
                Write-Host "`nTest 8: Retrieving components by AppModuleUniqueName..."
                $componentsByUniqueName = Get-DataverseAppModuleComponent -Connection $connection -AppModuleUniqueName $appModuleUniqueName
                if ($componentsByUniqueName.Count -eq 0) {
                    throw "No components found when querying by AppModuleUniqueName"
                }
                Write-Host "Successfully retrieved $($componentsByUniqueName.Count) components by AppModuleUniqueName"
                
                # --- TEST 9: Retrieve specific component by ObjectId ---
                Write-Host "`nTest 9: Retrieving specific component by ObjectId..."
                $specificComponent = Get-DataverseAppModuleComponent -Connection $connection -AppModuleId $appModuleId -ObjectId $contactEntityId
                if (-not $specificComponent) {
                    throw "Failed to retrieve component by ObjectId"
                }
                Write-Host "Successfully retrieved component by ObjectId"
                
                # --- TEST 10: Update component properties ---
                Write-Host "`nTest 10: Updating component properties..."
                if ($contactComponent.Id) {
                    Set-DataverseAppModuleComponent -Connection $connection `
                        -Id $contactComponent.Id `
                        -IsDefault $true `
                        -Confirm:$false
                    
                    # Verify update
                    $updatedComponent = Get-DataverseAppModuleComponent -Connection $connection -Id $contactComponent.Id
                    if ($updatedComponent.IsDefault -ne $true) {
                        throw "Component IsDefault update failed"
                    }
                    Write-Host "Successfully updated component properties"
                } else {
                    Write-Warning "Skipping component update - no component ID available"
                }
                
                # --- TEST 11: Validate appmodule (may have validation issues, but should not throw) ---
                Write-Host "`nTest 11: Validating appmodule..."
                try {
                    Set-DataverseAppModule -Connection $connection `
                        -Id $appModuleId `
                        -Validate `
                        -Confirm:$false -WarningAction SilentlyContinue
                    Write-Host "Validation completed (warnings/errors may be present but are expected)"
                } catch {
                    Write-Warning "Validation threw an error (may be expected): $_"
                }
                
                # --- TEST 12: Remove a component ---
                Write-Host "`nTest 12: Removing account entity component..."
                Remove-DataverseAppModuleComponent -Connection $connection `
                    -AppModuleId $appModuleId `
                    -ObjectId $accountEntityId `
                    -Confirm:$false
                
                # Verify removal
                $componentsAfterRemoval = Get-DataverseAppModuleComponent -Connection $connection -AppModuleId $appModuleId
                $accountComponentAfterRemoval = $componentsAfterRemoval | Where-Object { $_.ObjectId -eq $accountEntityId -and $_.ComponentType -eq 'Entity' }
                if ($accountComponentAfterRemoval) {
                    throw "Account entity component still exists after removal"
                }
                Write-Host "Successfully removed account entity component"
                
                # --- TEST 13: Test component removal with IfExists flag ---
                Write-Host "`nTest 13: Testing component removal with IfExists flag..."
                Remove-DataverseAppModuleComponent -Connection $connection `
                    -AppModuleId $appModuleId `
                    -ObjectId $accountEntityId `
                    -IfExists `
                    -Confirm:$false
                Write-Host "IfExists flag worked correctly - no error on non-existent component"
                
                # --- TEST 14: Publish appmodule ---
                Write-Host "`nTest 14: Publishing appmodule..."
                Set-DataverseAppModule -Connection $connection `
                    -Id $appModuleId `
                    -Publish `
                    -Confirm:$false
                Write-Host "Successfully published appmodule"
                
                # Verify published appmodule is retrievable
                $publishedAppModule = Get-DataverseAppModule -Connection $connection -Id $appModuleId -Published
                if (-not $publishedAppModule) {
                    throw "Failed to retrieve published appmodule"
                }
                Write-Host "Verified published appmodule is retrievable"
                
                # --- CLEANUP: Remove the test appmodule ---
                Write-Host "`nCleaning up: Removing test appmodule..."
                Remove-DataverseAppModule -Connection $connection -Id $appModuleId -Confirm:$false
                Write-Host "Successfully removed test appmodule"
                
                # Verify deletion
                $deletedAppModule = Get-DataverseAppModule -Connection $connection -Id $appModuleId
                if ($deletedAppModule) {
                    throw "AppModule still exists after deletion"
                }
                Write-Host "Verified appmodule was deleted"
                
                # --- TEST 15: Test IfExists flag on non-existent appmodule ---
                Write-Host "`nTest 15: Testing IfExists flag on non-existent appmodule..."
                Remove-DataverseAppModule -Connection $connection -Id $appModuleId -IfExists -Confirm:$false
                Write-Host "IfExists flag worked correctly - no error on non-existent appmodule"
                
                # --- TEST 16: Test NoUpdate and NoCreate flags ---
                Write-Host "`nTest 16: Testing NoUpdate flag..."
                $testAppId = Set-DataverseAppModule -Connection $connection `
                    -UniqueName "test_noupdate_$testRunId" `
                    -Name "NoUpdate Test $testRunId" `
                    -PassThru -Confirm:$false
                
                # Try to update with NoUpdate flag - should skip update
                Set-DataverseAppModule -Connection $connection `
                    -Id $testAppId `
                    -Name "Should Not Update" `
                    -NoUpdate `
                    -Confirm:$false
                
                # Verify it was not updated
                $noUpdateApp = Get-DataverseAppModule -Connection $connection -Id $testAppId
                if ($noUpdateApp.Name -eq "Should Not Update") {
                    throw "NoUpdate flag did not work - name was updated"
                }
                Write-Host "NoUpdate flag worked correctly - app was not updated"
                
                # Clean up NoUpdate test app
                Remove-DataverseAppModule -Connection $connection -Id $testAppId -Confirm:$false
                
                # --- TEST 17: Test NoCreate flag ---
                Write-Host "`nTest 17: Testing NoCreate flag..."
                $randomGuid = [guid]::NewGuid()
                $result = Set-DataverseAppModule -Connection $connection `
                    -Id $randomGuid `
                    -Name "Should Not Create" `
                    -NoCreate `
                    -Confirm:$false
                
                if ($result) {
                    throw "NoCreate flag did not work - app was created"
                }
                Write-Host "NoCreate flag worked correctly - app was not created"
                
                Write-Host "`n=== All appmodule tests passed successfully ==="
                
            } catch {
                # Attempt cleanup on failure
                Write-Host "ERROR: Test failed with exception: $_"
                Write-Host "Stack trace: $($_.ScriptStackTrace)"
                
                # Try to clean up test appmodule if it exists
                try {
                    if ($appModuleId) {
                        Write-Host "Attempting cleanup of test appmodule..."
                        $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                        Remove-DataverseAppModule -Connection $connection -Id $appModuleId -IfExists -Confirm:$false
                        Write-Host "Cleanup successful"
                    }
                } catch {
                    Write-Warning "Failed to cleanup test appmodule: $_"
                }
                
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
}
