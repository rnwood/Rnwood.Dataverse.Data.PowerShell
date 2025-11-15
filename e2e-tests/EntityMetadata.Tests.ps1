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
                [int]$MaxRetries = 3,
                [int]$InitialDelaySeconds = 2
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

            # Generate unique entity name
            $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
            $entityName = "new_e2etest_$testRunId"
            $schemaName = "new_E2ETest_$testRunId"
            $webResourceName1 = "svg_test_$testRunId"
            $webResourceName2 = "svg_test_updated_$testRunId"
                
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
          
                
            # Publish web resources

            $publishRequest = New-Object Microsoft.Crm.Sdk.Messages.PublishXmlRequest
            $publishRequest.ParameterXml = "<importexportxml><webresources><webresource>$webResourceName1</webresource><webresource>$webResourceName2</webresource></webresources></importexportxml>"
            Invoke-DataverseRequest -Connection $connection -Request $publishRequest | Out-Null
            Write-Host "  ✓ Published web resources"
          
                
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
                $script:entity = Get-DataverseEntityMetadata -Connection $connection -EntityName $entityName -IncludeAttributes
                    
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
                $oldEntities = Get-DataverseEntityMetadata -Connection $connection | Where-Object { 
                    $_.LogicalName -like "new_e2etest_*" -and 
                    $_.LogicalName -ne $entityName 
                }
                if ($oldEntities.Count -gt 0) {
                    Write-Host "  Found $($oldEntities.Count) old test entities to clean up"
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
                    Write-Host "  No old test entities found"
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
                    
                # Cleanup old web resources from previous failed runs
                $oldWebResources = Get-DataverseRecord -Connection $connection -TableName webresource -FilterValues @{ "name:Like" = "svg_test_%" } | 
                    Where-Object { $_.name -notin @($webResourceName1, $webResourceName2) }
                
                if ($oldWebResources) {
                    Write-Host "  Found $(@($oldWebResources).Count) old test web resources to clean up"
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
}
