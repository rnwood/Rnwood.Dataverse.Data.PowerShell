$ErrorActionPreference = "Stop"

Describe "Attribute Metadata E2E Tests" {

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

    It "Can create, read, update, and delete all attribute types comprehensively" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            $ErrorActionPreference = "Stop"
            $ConfirmPreference = 'None'  # Suppress all confirmation prompts in non-interactive mode
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Generate unique test identifiers
                $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
                $entityName = "new_e2eattr_$testRunId"
                $schemaName = "new_E2EAttr_$testRunId"
                
                Write-Host "Test entity: $entityName"
                
                # Cleanup any previous failed attempts
                Write-Host "Cleaning up any previous failed attempts..."
                $existingEntities = Get-DataverseEntityMetadata -Connection $connection | Where-Object { $_.LogicalName -like "new_e2eattr_*" }
                foreach ($entity in $existingEntities) {
                    try {
                        Write-Host "  Removing leftover entity: $($entity.LogicalName)"
                        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entity.LogicalName -Confirm:$false -ErrorAction SilentlyContinue
                    } catch {
                        Write-Host "  Could not remove $($entity.LogicalName): $_"
                    }
                }
                
                # Add a delay after cleanup to allow Dataverse to process deletions
                if ($existingEntities.Count -gt 0) {
                    Write-Host "Waiting for cleanup to complete..."
                    Start-Sleep -Seconds 30
                }
                
                Write-Host "Step 1: Creating test entity..."
                Set-DataverseEntityMetadata -Connection $connection `
                    -EntityName $entityName `
                    -SchemaName $schemaName `
                    -DisplayName "Attribute Test Entity" `
                    -DisplayCollectionName "Attribute Test Entities" `
                    -PrimaryAttributeSchemaName "new_name" `
                    -OwnershipType UserOwned `
                    -Publish `
                    -Confirm:$false
                Write-Host "✓ Test entity created and published"
                
                Write-Host "Step 2: Creating String attribute..."
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_email" `
                    -SchemaName "new_Email" `
                    -AttributeType String `
                    -DisplayName "Email Address" `
                    -StringFormat Email `
                    -MaxLength 100 `
                    -RequiredLevel None `
                    -IsSearchable `
                    -IsAuditEnabled `
                    -Confirm:$false
                Write-Host "✓ String attribute created"
                
                Write-Host "Step 3: Creating Integer attribute..."
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_count" `
                    -SchemaName "new_Count" `
                    -AttributeType Integer `
                    -DisplayName "Count" `
                    -MinValue 0 `
                    -MaxValue 1000 `
                    -RequiredLevel ApplicationRequired `
                    -Confirm:$false
                Write-Host "✓ Integer attribute created"
                
                Write-Host "Step 4: Creating Decimal attribute..."
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_amount" `
                    -SchemaName "new_Amount" `
                    -AttributeType Decimal `
                    -DisplayName "Amount" `
                    -Precision 2 `
                    -MinValue 0 `
                    -MaxValue 999999.99 `
                    -Confirm:$false
                Write-Host "✓ Decimal attribute created"
                
                Write-Host "Step 5: Creating Boolean attribute..."
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_isactive" `
                    -SchemaName "new_IsActive" `
                    -AttributeType Boolean `
                    -DisplayName "Is Active" `
                    -TrueLabel "Active" `
                    -FalseLabel "Inactive" `
                    -DefaultValue $true `
                    -Confirm:$false
                Write-Host "✓ Boolean attribute created"
                
                Write-Host "Step 6: Creating DateTime attribute..."
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_duedate" `
                    -SchemaName "new_DueDate" `
                    -AttributeType DateTime `
                    -DisplayName "Due Date" `
                    -DateTimeFormat DateOnly `
                    -DateTimeBehavior UserLocal `
                    -Confirm:$false
                Write-Host "✓ DateTime attribute created"
                
                Write-Host "Step 7: Creating Picklist attribute..."
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_priority" `
                    -SchemaName "new_Priority" `
                    -AttributeType Picklist `
                    -DisplayName "Priority" `
                    -Options @(
                        @{Value=1; Label='Low'}
                        @{Value=2; Label='Medium'}
                        @{Value=3; Label='High'}
                    ) `
                    -Publish `
                    -Confirm:$false
                Write-Host "✓ Picklist attribute created and attributes 2-7 published"
                
                Write-Host "Step 8: Creating Lookup attribute with relationship..."
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_accountid" `
                    -SchemaName "new_AccountId" `
                    -AttributeType Lookup `
                    -DisplayName "Related Account" `
                    -Description "Lookup to account entity" `
                    -Targets @('account') `
                    -RelationshipSchemaName "new_account_${entityName}_test" `
                    -CascadeDelete RemoveLink `
                    -CascadeAssign NoCascade `
                    -RequiredLevel Recommended `
                    -Publish `
                    -Confirm:$false
                Write-Host "✓ Lookup attribute created with relationship and published"
                
                Write-Host "Step 9: Creating Memo attribute..."
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_notes" `
                    -SchemaName "new_Notes" `
                    -AttributeType Memo `
                    -DisplayName "Notes" `
                    -MaxLength 2000 `
                    -Publish `
                    -Confirm:$false
                Write-Host "✓ Memo attribute created and all attributes published"
                
                Write-Host "Step 10: Reading and verifying all attributes..."
                $attributes = Get-DataverseAttributeMetadata -Connection $connection -EntityName $entityName
                
                $expectedAttributes = @('new_email', 'new_count', 'new_amount', 'new_isactive', 'new_duedate', 'new_priority', 'new_accountid', 'new_notes')
                $foundAttributes = $attributes | Where-Object { $_.LogicalName -in $expectedAttributes }
                
                if ($foundAttributes.Count -ne $expectedAttributes.Count) {
                    throw "Expected $($expectedAttributes.Count) attributes, found $($foundAttributes.Count)"
                }
                Write-Host "✓ All attributes verified ($($foundAttributes.Count) found)"
                
                Write-Host "Step 11: Updating String attribute..."
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_email" `
                    -DisplayName "Email Address (Updated)" `
                    -Description "Updated email field" `
                    -MaxLength 150 `
                    -Publish `
                    -Confirm:$false
                Write-Host "✓ String attribute updated and published"
                
                Write-Host "Step 12: Updating Lookup attribute..."
                Set-DataverseAttributeMetadata -Connection $connection `
                    -EntityName $entityName `
                    -AttributeName "new_accountid" `
                    -DisplayName "Primary Account" `
                    -Description "Updated lookup description" `
                    -RequiredLevel ApplicationRequired `
                    -Publish `
                    -Confirm:$false
                Write-Host "✓ Lookup attribute updated and published"
                
                Write-Host "Step 13: Verifying updates..."
                $updatedEmail = Get-DataverseAttributeMetadata -Connection $connection -EntityName $entityName -AttributeName "new_email"
                if ($updatedEmail.DisplayName.UserLocalizedLabel.Label -ne "Email Address (Updated)") {
                    throw "Email attribute display name not updated"
                }
                if ($updatedEmail.MaxLength -ne 150) {
                    throw "Email attribute max length not updated"
                }
                
                $updatedLookup = Get-DataverseAttributeMetadata -Connection $connection -EntityName $entityName -AttributeName "new_accountid"
                if ($updatedLookup.DisplayName.UserLocalizedLabel.Label -ne "Primary Account") {
                    throw "Lookup attribute display name not updated"
                }
                Write-Host "✓ Updates verified"
                
                Write-Host "Step 14: Testing attribute deletion..."
                Remove-DataverseAttributeMetadata -Connection $connection -EntityName $entityName -AttributeName "new_notes" -Confirm:$false
                Write-Host "✓ Memo attribute deleted"
                
                Start-Sleep -Seconds 2
                $remainingAttrs = Get-DataverseAttributeMetadata -Connection $connection -EntityName $entityName | Where-Object { $_.LogicalName -eq 'new_notes' }
                if ($remainingAttrs) {
                    throw "Deleted attribute still exists"
                }
                Write-Host "✓ Deletion verified"
                
                Write-Host "Step 15: Cleanup - Deleting test entity..."
                Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
                Write-Host "✓ Test entity deleted"
                
                Write-Host "SUCCESS: All attribute metadata operations completed successfully"
                
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
