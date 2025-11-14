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
    }

    It "Can create, read, update, and delete custom entity with all features" {
        $output = pwsh -noprofile -command {
            [console]::InputEncoding = [console]::OutputEncoding = [System.Text.UTF8Encoding]::new()
            $env:PSModulePath = $env:ChildProcessPSModulePath
            $ErrorActionPreference = "Stop"
            $ConfirmPreference = 'None'  # Suppress all confirmation prompts in non-interactive mode
            $VerbosePreference = 'Continue'  # Enable verbose output
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            # Retry helper function with exponential backoff
            function Invoke-WithRetry {
                param(
                    [Parameter(Mandatory=$true)]
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
                
                # Generate unique entity name
                $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
                $entityName = "new_e2etest_$testRunId"
                $schemaName = "new_E2ETest_$testRunId"
                
                Write-Host "Test entity: $entityName"
                
                Write-Host "Step 0: Creating required web resources for icon testing..."
                
                # Create a simple SVG content for testing
                $svgContent = @"
<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 16 16">
  <rect width="16" height="16" fill="#0078D4"/>
</svg>
"@
                $svgBase64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($svgContent))
                
                # Create svg_test web resource
                try {
                    $webResource1 = @{
                        name = "svg_test"
                        displayname = "E2E Test Icon"
                        webresourcetype = 11  # SVG
                        content = $svgBase64
                    }
                    
                    Invoke-DataverseRequest -Connection $connection -Request ([Microsoft.Xrm.Sdk.Messages.CreateRequest]@{
                        Target = New-Object Microsoft.Xrm.Sdk.Entity("webresource", $null, @{
                            name = $webResource1.name
                            displayname = $webResource1.displayname
                            webresourcetype = New-Object Microsoft.Xrm.Sdk.OptionSetValue($webResource1.webresourcetype)
                            content = $webResource1.content
                        })
                    }) | Out-Null
                    Write-Host "  ✓ Created web resource: svg_test"
                } catch {
                    # Web resource might already exist, try to update it
                    try {
                        $existing = Invoke-DataverseRequest -Connection $connection -Request ([Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest]@{
                            Query = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression("webresource") -Property @{
                                ColumnSet = New-Object Microsoft.Xrm.Sdk.Query.ColumnSet("webresourceid")
                                Criteria = New-Object Microsoft.Xrm.Sdk.Query.FilterExpression -Property @{
                                    Conditions = @(
                                        New-Object Microsoft.Xrm.Sdk.Query.ConditionExpression("name", [Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal, "svg_test")
                                    )
                                }
                            }
                        }) | Select-Object -ExpandProperty Results | Select-Object -ExpandProperty Entities | Select-Object -First 1
                        
                        if ($existing) {
                            Write-Host "  ✓ Web resource svg_test already exists"
                        }
                    } catch {
                        Write-Host "  ! Could not create or find svg_test: $_"
                    }
                }
                
                # Create svg_test_updated web resource
                try {
                    $webResource2 = @{
                        name = "svg_test_updated"
                        displayname = "E2E Test Icon Updated"
                        webresourcetype = 11  # SVG
                        content = $svgBase64
                    }
                    
                    Invoke-DataverseRequest -Connection $connection -Request ([Microsoft.Xrm.Sdk.Messages.CreateRequest]@{
                        Target = New-Object Microsoft.Xrm.Sdk.Entity("webresource", $null, @{
                            name = $webResource2.name
                            displayname = $webResource2.displayname
                            webresourcetype = New-Object Microsoft.Xrm.Sdk.OptionSetValue($webResource2.webresourcetype)
                            content = $webResource2.content
                        })
                    }) | Out-Null
                    Write-Host "  ✓ Created web resource: svg_test_updated"
                } catch {
                    # Web resource might already exist
                    try {
                        $existing = Invoke-DataverseRequest -Connection $connection -Request ([Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest]@{
                            Query = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression("webresource") -Property @{
                                ColumnSet = New-Object Microsoft.Xrm.Sdk.Query.ColumnSet("webresourceid")
                                Criteria = New-Object Microsoft.Xrm.Sdk.Query.FilterExpression -Property @{
                                    Conditions = @(
                                        New-Object Microsoft.Xrm.Sdk.Query.ConditionExpression("name", [Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal, "svg_test_updated")
                                    )
                                }
                            }
                        }) | Select-Object -ExpandProperty Results | Select-Object -ExpandProperty Entities | Select-Object -First 1
                        
                        if ($existing) {
                            Write-Host "  ✓ Web resource svg_test_updated already exists"
                        }
                    } catch {
                        Write-Host "  ! Could not create or find svg_test_updated: $_"
                    }
                }
                
                # Publish web resources
                try {
                    $publishRequest = New-Object Microsoft.Crm.Sdk.Messages.PublishXmlRequest
                    $publishRequest.ParameterXml = "<importexportxml><webresources><webresource>svg_test</webresource><webresource>svg_test_updated</webresource></webresources></importexportxml>"
                    Invoke-DataverseRequest -Connection $connection -Request $publishRequest | Out-Null
                    Write-Host "  ✓ Published web resources"
                } catch {
                    Write-Host "  ! Could not publish web resources: $_"
                }
                
                Write-Host "Step 1: Creating custom entity with all features..."
                Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection
                    
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
                        -IconVectorName "svg_test" `
                        -Confirm:$false
                }
                Write-Host "✓ Entity created"
                
                Write-Host "Step 2: Reading entity metadata..."
                Invoke-WithRetry {
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
                    Wait-DataversePublish -Connection $connection
                    
                    Set-DataverseEntityMetadata `
                        -Connection $connection `
                        -EntityName $entityName `
                        -DisplayName "E2E Test Entity (Updated)" `
                        -Description "Updated description" `
                        -IconVectorName "svg_test_updated" `
                        -Confirm:$false
                }
                Write-Host "✓ Entity updated"
                
                Write-Host "Step 4: Verifying update..."
                Invoke-WithRetry {
                    $script:updatedEntity = Get-DataverseEntityMetadata -Connection $connection -EntityName $entityName
                    
                    if ($script:updatedEntity.DisplayName.UserLocalizedLabel.Label -ne "E2E Test Entity (Updated)") {
                        throw "Display name not updated"
                    }
                    if ($script:updatedEntity.IconVectorName -ne "svg_test_updated") {
                        throw "Icon not updated"
                    }
                }
                Write-Host "✓ Updates verified"
                
                Write-Host "Step 5: Testing EntityMetadata object update..."
                Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection
                    
                    $script:entityObj = Get-DataverseEntityMetadata -Connection $connection -EntityName $entityName
                    $script:entityObj.Description = New-Object Microsoft.Xrm.Sdk.Label
                    $script:entityObj.Description.UserLocalizedLabel = New-Object Microsoft.Xrm.Sdk.LocalizedLabel("Bulk update test", 1033)
                    
                    Set-DataverseEntityMetadata -Connection $connection -EntityMetadata $script:entityObj -Confirm:$false
                }
                Write-Host "✓ EntityMetadata object update complete"
                
                Write-Host "Step 6: Cleanup - Deleting entity..."
                Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection
                    
                    Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
                }
                Write-Host "✓ Entity deleted"
                
                Write-Host "Step 7: Verifying deletion..."
                Invoke-WithRetry {
                    Start-Sleep -Seconds 2  # Give it time to process
                    $script:deletedEntity = Get-DataverseEntityMetadata -Connection $connection | Where-Object { $_.LogicalName -eq $entityName }
                    if ($script:deletedEntity) {
                        throw "Entity still exists after deletion"
                    }
                }
                Write-Host "✓ Deletion verified"
                
                Write-Host "Step 8: Cleanup any old test entities from previous failed runs..."
                $oldEntities = Get-DataverseEntityMetadata -Connection $connection | Where-Object { 
                    $_.LogicalName -like "new_e2etest_*" -and 
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
                
                Write-Host "SUCCESS: All entity metadata operations completed successfully"
                
            } catch {
                Write-Host "ERROR: $($_ | Out-String)"
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }
        
        # Display the output from pwsh
        Write-Host $output

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
}
