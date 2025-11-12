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
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Generate unique entity name
                $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
                $entityName = "new_e2etest_$testRunId"
                $schemaName = "new_E2ETest_$testRunId"
                
                Write-Host "Test entity: $entityName"
                
                # Cleanup any previous failed attempts with similar names
                Write-Host "Cleaning up any previous failed attempts..."
                $existingEntities = Get-DataverseEntityMetadata | Where-Object { $_.LogicalName -like "new_e2etest_*" }
                foreach ($entity in $existingEntities) {
                    try {
                        Write-Host "  Removing leftover entity: $($entity.LogicalName)"
                        Remove-DataverseEntityMetadata -EntityName $entity.LogicalName -Confirm:$false -ErrorAction SilentlyContinue
                    } catch {
                        Write-Host "  Could not remove $($entity.LogicalName): $_"
                    }
                }
                
                Write-Host "Step 1: Creating custom entity with all features..."
                Set-DataverseEntityMetadata `
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
                
                Write-Host "✓ Entity created"
                
                # Publish the entity
                Write-Host "Publishing entity..."
                Invoke-DataverseRequest -Connection $connection -Request @{
                    '@odata.type' = 'Microsoft.Crm.Sdk.Messages.PublishXmlRequest'
                    ParameterXml = "<importexportxml><entities><entity>$entityName</entity></entities></importexportxml>"
                }
                Write-Host "✓ Entity published"
                
                Write-Host "Step 2: Reading entity metadata..."
                $entity = Get-DataverseEntityMetadata -EntityName $entityName -IncludeAttributes
                
                if (-not $entity) {
                    throw "Failed to retrieve entity metadata"
                }
                if ($entity.DisplayName.UserLocalizedLabel.Label -ne "E2E Test Entity") {
                    throw "Display name mismatch"
                }
                if ($entity.IsAuditEnabled.Value -ne $true) {
                    throw "Audit not enabled"
                }
                if ($entity.ChangeTrackingEnabled -ne $true) {
                    throw "Change tracking not enabled"
                }
                Write-Host "✓ Entity metadata verified"
                
                Write-Host "Step 3: Updating entity metadata..."
                Set-DataverseEntityMetadata `
                    -EntityName $entityName `
                    -DisplayName "E2E Test Entity (Updated)" `
                    -Description "Updated description" `
                    -IconVectorName "svg_test_updated" `
                    -Confirm:$false
                
                Write-Host "✓ Entity updated"
                
                # Publish the updates
                Invoke-DataverseRequest -Connection $connection -Request @{
                    '@odata.type' = 'Microsoft.Crm.Sdk.Messages.PublishXmlRequest'
                    ParameterXml = "<importexportxml><entities><entity>$entityName</entity></entities></importexportxml>"
                }
                
                Write-Host "Step 4: Verifying update..."
                $updatedEntity = Get-DataverseEntityMetadata -EntityName $entityName
                
                if ($updatedEntity.DisplayName.UserLocalizedLabel.Label -ne "E2E Test Entity (Updated)") {
                    throw "Display name not updated"
                }
                if ($updatedEntity.IconVectorName -ne "svg_test_updated") {
                    throw "Icon not updated"
                }
                Write-Host "✓ Updates verified"
                
                Write-Host "Step 5: Testing EntityMetadata object update..."
                $entityObj = Get-DataverseEntityMetadata -EntityName $entityName
                $entityObj.Description = New-Object Microsoft.Xrm.Sdk.Label
                $entityObj.Description.UserLocalizedLabel = New-Object Microsoft.Xrm.Sdk.LocalizedLabel("Bulk update test", 1033)
                
                Set-DataverseEntityMetadata -EntityMetadata $entityObj -Confirm:$false
                Write-Host "✓ EntityMetadata object update complete"
                
                Write-Host "Step 6: Cleanup - Deleting entity..."
                Remove-DataverseEntityMetadata -EntityName $entityName -Confirm:$false
                Write-Host "✓ Entity deleted"
                
                Write-Host "Step 7: Verifying deletion..."
                Start-Sleep -Seconds 2  # Give it time to process
                $deletedEntity = Get-DataverseEntityMetadata | Where-Object { $_.LogicalName -eq $entityName }
                if ($deletedEntity) {
                    throw "Entity still exists after deletion"
                }
                Write-Host "✓ Deletion verified"
                
                Write-Host "SUCCESS: All entity metadata operations completed successfully"
                
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
