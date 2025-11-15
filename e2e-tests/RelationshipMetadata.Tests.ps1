$ErrorActionPreference = "Stop"


Describe "Relationship Metadata E2E Tests" {

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

    It "Can create, read, update, and delete OneToMany and ManyToMany relationships comprehensively" {
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

            # Generate unique test identifiers
            $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
            $entity1Name = "new_e2erel1_$testRunId"
            $entity1Schema = "new_E2ERel1_$testRunId"
            $entity2Name = "new_e2erel2_$testRunId"
            $entity2Schema = "new_E2ERel2_$testRunId"
                
            Write-Host "Test entities: $entity1Name, $entity2Name"
            
            Write-Host "Step 0: Cleanup any old test entities from previous failed runs..."
            Wait-DataversePublish -Connection $connection -Verbose
            $oldEntities = Get-DataverseEntityMetadata -Connection $connection | Where-Object { 
                $_.LogicalName -like "new_e2erel*" -and $_.LogicalName -notlike "*m2m_intersect"
            }
            if ($oldEntities.Count -gt 0) {
                Write-Host "  Found $($oldEntities.Count) old test entities to clean up"
                foreach ($entity in $oldEntities) {
                    try {
                        Write-Host "  Removing old entity: $($entity.LogicalName)"
                        Wait-DataversePublish -Connection $connection -Verbose
                        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entity.LogicalName -Confirm:$false -ErrorAction Stop
                    }
                    catch {
                        Write-Host "  Could not remove $($entity.LogicalName): $_"
                    }
                }
            }
            else {
                Write-Host "  No old test entities found"
            }
                
            Write-Host "Step 1: Creating first test entity..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Set-DataverseEntityMetadata -Connection $connection `
                    -EntityName $entity1Name `
                    -SchemaName $entity1Schema `
                    -DisplayName "Relationship Test Entity 1" `
                    -DisplayCollectionName "Relationship Test Entities 1" `
                    -PrimaryAttributeSchemaName "new_name" `
                    -OwnershipType UserOwned `
                    -Confirm:$false
            }
            Write-Host "✓ Entity 1 created"
                
            Write-Host "Step 2: Creating second test entity..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Set-DataverseEntityMetadata -Connection $connection `
                    -EntityName $entity2Name `
                    -SchemaName $entity2Schema `
                    -DisplayName "Relationship Test Entity 2" `
                    -DisplayCollectionName "Relationship Test Entities 2" `
                    -PrimaryAttributeSchemaName "new_name" `
                    -OwnershipType UserOwned `
                    -Confirm:$false
            }
            Write-Host "✓ Entity 2 created"
                
            Write-Host "Step 3: Creating OneToMany relationship with lookup..."
            $relName1 = "${entity1Name}_${entity2Name}_rel1"
            $lookupSchemaName = "new_Entity1Id"
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Set-DataverseRelationshipMetadata `
                    -Connection $connection `
                    -SchemaName $relName1 `
                    -RelationshipType OneToMany `
                    -ReferencedEntity $entity1Name `
                    -ReferencingEntity $entity2Name `
                    -LookupAttributeSchemaName $lookupSchemaName `
                    -LookupAttributeDisplayName "Related Entity 1" `
                    -LookupAttributeDescription "Lookup to entity 1" `
                    -LookupAttributeRequiredLevel None `
                    -CascadeDelete RemoveLink `
                    -CascadeAssign NoCascade `
                    -CascadeShare NoCascade `
                    -CascadeUnshare NoCascade `
                    -CascadeReparent NoCascade `
                    -CascadeMerge NoCascade `
                    -IsSearchable `
                    -Confirm:$false
            }
            Write-Host "✓ OneToMany relationship created"
                
            Write-Host "Step 4: Creating self-referencing OneToMany relationship..."
            $selfRelName = "${entity1Name}_parent_rel"
            $parentLookupSchemaName = "new_ParentId"
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Set-DataverseRelationshipMetadata `
                    -Connection $connection `
                    -SchemaName $selfRelName `
                    -RelationshipType OneToMany `
                    -ReferencedEntity $entity1Name `
                    -ReferencingEntity $entity1Name `
                    -LookupAttributeSchemaName $parentLookupSchemaName `
                    -LookupAttributeDisplayName "Parent Record" `
                    -LookupAttributeRequiredLevel None `
                    -CascadeDelete RemoveLink `
                    -IsHierarchical `
                    -Confirm:$false
            }
            Write-Host "✓ Self-referencing relationship created"
                
            Write-Host "Step 5: Creating ManyToMany relationship..."
            $m2mRelName = "${entity1Name}_${entity2Name}_m2m"
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Set-DataverseRelationshipMetadata `
                    -Connection $connection `
                    -SchemaName $m2mRelName `
                    -RelationshipType ManyToMany `
                    -ReferencedEntity $entity1Name `
                    -ReferencingEntity $entity2Name `
                    -IntersectEntitySchemaName "${m2mRelName}_intersect" `
                    -Confirm:$false
            }
            Write-Host "✓ ManyToMany relationship created"
                
            Write-Host "Step 6: Reading and verifying relationships..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $relationships = Get-DataverseRelationshipMetadata -Connection $connection -EntityName $entity1Name
                
                $ourRelationships = $relationships | Where-Object { 
                    $_.SchemaName -eq $relName1 -or 
                    $_.SchemaName -eq $selfRelName -or 
                    $_.SchemaName -eq $m2mRelName 
                }
                
                if ($ourRelationships.Count -ne 3) {
                    throw "Expected 3 relationships, found $($ourRelationships.Count)"
                }
            }
            Write-Host "✓ All relationships verified"
                
            Write-Host "Step 7: Reading specific OneToMany relationship..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $rel1 = Get-DataverseRelationshipMetadata -Connection $connection -SchemaName $relName1
                
                if ($rel1.RelationshipType -ne 'OneToManyRelationship') {
                    throw "Wrong relationship type for OneToMany"
                }
                if ($rel1.ReferencedEntity -ne $entity1Name) {
                    throw "Wrong referenced entity"
                }
                if ($rel1.ReferencingEntity -ne $entity2Name) {
                    throw "Wrong referencing entity"
                }
            }
            Write-Host "✓ OneToMany relationship details verified"
                
            Write-Host "Step 8: Reading specific ManyToMany relationship..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $m2mRel = Get-DataverseRelationshipMetadata -Connection $connection -SchemaName $m2mRelName
                
                if ($m2mRel.RelationshipType -ne 'ManyToManyRelationship') {
                    throw "Wrong relationship type for ManyToMany"
                }
            }
            Write-Host "✓ ManyToMany relationship details verified"
                
            Write-Host "Step 9: Updating OneToMany relationship cascade behaviors..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Set-DataverseRelationshipMetadata `
                    -Connection $connection `
                    -SchemaName $relName1 `
                    -RelationshipType OneToMany `
                    -ReferencedEntity $entity1Name `
                    -ReferencingEntity $entity2Name `
                    -CascadeDelete Cascade `
                    -CascadeAssign Cascade `
                    -Confirm:$false
            }
            Write-Host "✓ Relationship updated"
                
            Write-Host "Step 10: Verifying relationship update..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $updatedRel = Get-DataverseRelationshipMetadata -Connection $connection -SchemaName $relName1
                
                if ($updatedRel.CascadeConfiguration.Delete -ne 'Cascade') {
                    throw "Cascade delete not updated"
                }
                if ($updatedRel.CascadeConfiguration.Assign -ne 'Cascade') {
                    throw "Cascade assign not updated"
                }
            }
            Write-Host "✓ Relationship updates verified"
                
            Write-Host "Step 11: Testing relationship retrieval with filters..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $oneToManyRels = Get-DataverseRelationshipMetadata -Connection $connection -EntityName $entity1Name -RelationshipType OneToMany
                $manyToManyRels = Get-DataverseRelationshipMetadata -Connection $connection -EntityName $entity1Name -RelationshipType ManyToMany
                
                Write-Host "  OneToMany: $($oneToManyRels.Count), ManyToMany: $($manyToManyRels.Count)"
            }
            Write-Host "✓ Filtered retrieval successful"
                
            Write-Host "Step 12: Cleanup - Deleting relationships (implicit via entity deletion)..."
            # Note: Relationships are deleted when entities are deleted
                
            Write-Host "Step 13: Cleanup - Deleting test entities..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Remove-DataverseEntityMetadata -Connection $connection -EntityName $entity2Name -Confirm:$false
            }
            Write-Host "✓ Entity 2 deleted"
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Remove-DataverseEntityMetadata -Connection $connection -EntityName $entity1Name -Confirm:$false
            }
            Write-Host "✓ Entity 1 deleted"
                
            Write-Host "Step 14: Verifying cleanup..."
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $remainingEntities = Get-DataverseEntityMetadata -Connection $connection | Where-Object { 
                    $_.LogicalName -eq $entity1Name -or $_.LogicalName -eq $entity2Name 
                }
                if ($remainingEntities) {
                    throw "Entities still exist after deletion"
                }
            }
            Write-Host "✓ Cleanup verified"
                
            Write-Host "SUCCESS: All relationship metadata operations completed successfully"
                
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }

    }
}
