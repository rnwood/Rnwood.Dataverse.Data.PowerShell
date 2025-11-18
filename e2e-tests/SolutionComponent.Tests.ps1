$ErrorActionPreference = "Stop"

Describe "Solution Component E2E Tests" {

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

    It "Can add, read, update, and manage solution components" {
            
        $ErrorActionPreference = "Stop"
        $ConfirmPreference = 'None'  # Suppress all confirmation prompts in non-interactive mode
        $VerbosePreference = 'SilentlyContinue'  # Reduce noise; enable 'Continue' for debugging if needed
        
            
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
            $solutionName = "e2esolcomp_${timestamp}_$testRunId"
            $solutionDisplayName = "E2E Solution Component Test $testRunId"
            $publisherPrefix = "e2e"
            $entityName = "new_e2esolent_${timestamp}_$testRunId"
            $entitySchemaName = "new_E2ESolEnt_${timestamp}_$testRunId"
                
            Write-Host "Test solution: $solutionName"
            Write-Host "Test entity: $entityName"
                
            Write-Host "Step 1: Creating test solution..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                
                # Check if solution already exists and delete it
                $existingSolution = Get-DataverseSolution -Connection $connection -UniqueName $solutionName -ErrorAction SilentlyContinue
                if ($existingSolution) {
                    Write-Host "  Removing existing test solution..."
                    Remove-DataverseSolution -Connection $connection -UniqueName $solutionName -Confirm:$false
                }
                
                # Get or create publisher
                $publisher = Get-DataverseRecord -Connection $connection -TableName publisher -Filter @{ "customizationprefix" = $publisherPrefix } | Select-Object -First 1
                if (-not $publisher) {
                    Write-Host "  Creating test publisher..."
                    $publisher = @{
                        "uniquename" = "e2etestpublisher_$testRunId"
                        "friendlyname" = "E2E Test Publisher"
                        "customizationprefix" = $publisherPrefix
                    } | Set-DataverseRecord -Connection $connection -TableName publisher -PassThru
                }
                
                # Create solution using Set-DataverseSolution
                Set-DataverseSolution -Connection $connection `
                    -UniqueName $solutionName `
                    -Name $solutionDisplayName `
                    -Version "1.0.0.0" `
                    -PublisherUniqueName $publisher.uniquename `
                    -Confirm:$false
                
                # Get the solution to retrieve its ID
                $solution = Get-DataverseSolution -Connection $connection -UniqueName $solutionName
                $script:solutionId = $solution.solutionid
            }
        
            Write-Host "✓ Test solution created (ID: $($script:solutionId))"
                
            Write-Host "Step 2: Creating test entity..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                
                Set-DataverseEntityMetadata -Connection $connection `
                    -EntityName $entityName `
                    -SchemaName $entitySchemaName `
                    -DisplayName "E2E Solution Test Entity" `
                    -DisplayCollectionName "E2E Solution Test Entities" `
                    -PrimaryAttributeSchemaName "new_name" `
                    -OwnershipType UserOwned `
                    -Confirm:$false
            }
        
            Write-Host "✓ Test entity created"
            
            # Get the entity metadata to retrieve the object ID
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                $entityMetadata = Get-DataverseEntityMetadata -Connection $connection -EntityName $entityName
                $script:entityObjectId = $entityMetadata.MetadataId
            }
            Write-Host "  Entity ObjectId: $($script:entityObjectId)"
                
            Write-Host "Step 3: Adding entity to solution with Behavior 0 (Include Subcomponents)..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                
                $result = Set-DataverseSolutionComponent -Connection $connection `
                    -SolutionName $solutionName `
                    -ComponentId $script:entityObjectId `
                    -ComponentType 1 `
                    -Behavior 0 `
                    -PassThru `
                    -Confirm:$false
                
                if ($result.WasUpdated -eq $true) {
                    throw "Component should not be marked as updated when adding new component"
                }
                if ($result.BehaviorValue -ne 0) {
                    throw "Expected behavior value 0, got $($result.BehaviorValue)"
                }
            }
            Write-Host "✓ Entity added to solution with Behavior 0"
                
            Write-Host "Step 4: Verifying component exists in solution using Get-DataverseSolutionComponent..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                $components = Get-DataverseSolutionComponent -Connection $connection -SolutionName $solutionName
                
                $entityComponent = $components | Where-Object { $_.ObjectId -eq $script:entityObjectId }
                if (-not $entityComponent) {
                    throw "Entity component not found in solution"
                }
                if ($entityComponent.ComponentType -ne 1) {
                    throw "Expected component type 1 (Entity), got $($entityComponent.ComponentType)"
                }
                Write-Host "  Found component: Type=$($entityComponent.ComponentType), Behavior=$($entityComponent.Behavior)"
            }
            Write-Host "✓ Component verified in solution"
                
            Write-Host "Step 5: Attempting to add same component with same behavior (should be no-op)..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                
                $result = Set-DataverseSolutionComponent -Connection $connection `
                    -SolutionName $solutionName `
                    -ComponentId $script:entityObjectId `
                    -ComponentType 1 `
                    -Behavior 0 `
                    -PassThru `
                    -Confirm:$false
                
                if ($result.WasUpdated -eq $true) {
                    throw "Component should not be marked as updated when behavior is unchanged"
                }
            }
            Write-Host "✓ Idempotent behavior verified (no-op when behavior unchanged)"
                
            Write-Host "Step 6: Changing component behavior from 0 to 1 (Do Not Include Subcomponents)..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                
                $result = Set-DataverseSolutionComponent -Connection $connection `
                    -SolutionName $solutionName `
                    -ComponentId $script:entityObjectId `
                    -ComponentType 1 `
                    -Behavior 1 `
                    -PassThru `
                    -Confirm:$false
                
                if ($result.WasUpdated -ne $true) {
                    throw "Component should be marked as updated when behavior changes"
                }
                if ($result.BehaviorValue -ne 1) {
                    throw "Expected behavior value 1, got $($result.BehaviorValue)"
                }
                if ($result.Behavior -ne "Do Not Include Subcomponents") {
                    throw "Expected behavior name 'Do Not Include Subcomponents', got '$($result.Behavior)'"
                }
            }
            Write-Host "✓ Component behavior changed from 0 to 1"
                
            Write-Host "Step 7: Verifying behavior change using Get-DataverseSolutionComponent..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                $components = Get-DataverseSolutionComponent -Connection $connection -SolutionName $solutionName
                
                $entityComponent = $components | Where-Object { $_.ObjectId -eq $script:entityObjectId }
                if (-not $entityComponent) {
                    throw "Entity component not found in solution after behavior change"
                }
                Write-Host "  Component behavior: $($entityComponent.Behavior)"
            }
            Write-Host "✓ Behavior change verified"
                
            Write-Host "Step 8: Adding another attribute component to solution..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                
                # First create an attribute
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_testfield" `
                    -SchemaName "new_TestField" `
                    -AttributeType String `
                    -DisplayName "Test Field" `
                    -MaxLength 100 `
                    -Confirm:$false
            }
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                
                # Get the attribute metadata to get its object ID
                $attributeMetadata = Get-DataverseAttributeMetadata -Connection $connection -EntityName $entityName -AttributeName "new_testfield"
                $script:attributeObjectId = $attributeMetadata.MetadataId
                
                # Add attribute to solution
                Set-DataverseSolutionComponent -Connection $connection `
                    -SolutionName $solutionName `
                    -ComponentId $script:attributeObjectId `
                    -ComponentType 2 `
                    -Behavior 0 `
                    -Confirm:$false
            }
            Write-Host "✓ Attribute component added to solution"
                
            Write-Host "Step 9: Listing all components in solution..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                $allComponents = Get-DataverseSolutionComponent -Connection $connection -SolutionName $solutionName
                
                Write-Host "  Total components in solution: $($allComponents.Count)"
                
                # Verify we have at least our entity and attribute
                $entityComp = $allComponents | Where-Object { $_.ObjectId -eq $script:entityObjectId }
                $attributeComp = $allComponents | Where-Object { $_.ObjectId -eq $script:attributeObjectId }
                
                if (-not $entityComp) {
                    throw "Entity component not found in component list"
                }
                if (-not $attributeComp) {
                    throw "Attribute component not found in component list"
                }
            }
            Write-Host "✓ All components listed successfully"
                
            Write-Host "Step 10: Testing Get-DataverseSolutionComponent with SolutionId parameter..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                $componentsBySolutionId = Get-DataverseSolutionComponent -Connection $connection -SolutionId $script:solutionId
                
                if ($componentsBySolutionId.Count -eq 0) {
                    throw "No components found when querying by SolutionId"
                }
                
                Write-Host "  Found $($componentsBySolutionId.Count) components using SolutionId parameter"
            }
            Write-Host "✓ Get-DataverseSolutionComponent works with SolutionId parameter"
                
            Write-Host "Step 11: Testing Remove-DataverseSolutionComponent..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                
                # Remove the attribute component
                Remove-DataverseSolutionComponent -Connection $connection `
                    -SolutionName $solutionName `
                    -ComponentId $script:attributeObjectId `
                    -ComponentType 2 `
                    -Confirm:$false
            }
            Write-Host "✓ Attribute component removed from solution"
                
            Write-Host "Step 12: Verifying component removal..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                $components = Get-DataverseSolutionComponent -Connection $connection -SolutionName $solutionName
                
                $attributeComp = $components | Where-Object { $_.ObjectId -eq $script:attributeObjectId }
                if ($attributeComp) {
                    throw "Attribute component should not be in solution after removal"
                }
                
                # Entity should still be there
                $entityComp = $components | Where-Object { $_.ObjectId -eq $script:entityObjectId }
                if (-not $entityComp) {
                    throw "Entity component should still be in solution"
                }
            }
            Write-Host "✓ Component removal verified"
                
            Write-Host "Step 13: Testing Remove-DataverseSolutionComponent with IfExists (should not error)..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                
                # Try to remove the attribute again with IfExists - should not error
                Remove-DataverseSolutionComponent -Connection $connection `
                    -SolutionName $solutionName `
                    -ComponentId $script:attributeObjectId `
                    -ComponentType 2 `
                    -IfExists `
                    -Confirm:$false
            }
            Write-Host "✓ IfExists parameter works correctly"
                
            Write-Host "Step 14: Cleanup - Removing test solution (will cascade delete components)..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                Remove-DataverseSolution -Connection $connection -UniqueName $solutionName -Confirm:$false
            }
            Write-Host "✓ Test solution deleted"
                
            Write-Host "Step 15: Cleanup - Removing test entity..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
            }
            Write-Host "✓ Test entity deleted"
                
            Write-Host "Step 16: Cleanup any old test solutions from previous failed runs..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                
                # Only clean up solutions older than 1 hour to avoid interfering with concurrent tests
                $cutoffTime = [DateTime]::UtcNow.AddHours(-1)
                $cutoffTimestamp = $cutoffTime.ToString("yyyyMMddHHmm")
                
                $oldSolutions = Get-DataverseSolution -Connection $connection | Where-Object { 
                    $_.uniquename -like "e2esolcomp_*" -and 
                    $_.uniquename -ne $solutionName -and
                    # Extract timestamp from name (format: e2esolcomp_yyyyMMddHHmm_guid)
                    # Only clean up if timestamp is old enough (older than 1 hour)
                    ($_.uniquename -match "e2esolcomp_(\d{12})_" -and $matches[1] -lt $cutoffTimestamp)
                }
                if ($oldSolutions.Count -gt 0) {
                    Write-Host "  Found $($oldSolutions.Count) old test solutions (>1 hour old) to clean up"
                    foreach ($sol in $oldSolutions) {
                        try {
                            Write-Host "  Removing old solution: $($sol.uniquename)"
                            Remove-DataverseSolution -Connection $connection -UniqueName $sol.uniquename -Confirm:$false -ErrorAction SilentlyContinue
                        }
                        catch {
                            Write-Host "  Could not remove $($sol.uniquename): $_"
                        }
                    }
                }
                else {
                    Write-Host "  No old test solutions found (>1 hour old)"
                }
            }
            
            Write-Host "Step 17: Cleanup any old test entities from previous failed runs..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection
                
                # Only clean up entities older than 1 hour to avoid interfering with concurrent tests
                $cutoffTime = [DateTime]::UtcNow.AddHours(-1)
                $cutoffTimestamp = $cutoffTime.ToString("yyyyMMddHHmm")
                
                $oldEntities = Get-DataverseEntityMetadata -Connection $connection | Where-Object { 
                    $_.LogicalName -like "new_e2esolent_*" -and 
                    $_.LogicalName -ne $entityName -and
                    # Extract timestamp from name (format: new_e2esolent_yyyyMMddHHmm_guid)
                    # Only clean up if timestamp is old enough (older than 1 hour)
                    ($_.LogicalName -match "new_e2esolent_(\d{12})_" -and $matches[1] -lt $cutoffTimestamp)
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
                
            Write-Host "SUCCESS: All solution component operations completed successfully"
                
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }
}
