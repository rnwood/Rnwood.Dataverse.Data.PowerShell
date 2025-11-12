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
    }

    It "Can create, read, update, and delete OneToMany and ManyToMany relationships comprehensively" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Generate unique test identifiers
                $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
                $entity1Name = "new_e2erel1_$testRunId"
                $entity1Schema = "new_E2ERel1_$testRunId"
                $entity2Name = "new_e2erel2_$testRunId"
                $entity2Schema = "new_E2ERel2_$testRunId"
                
                Write-Host "Test entities: $entity1Name, $entity2Name"
                
                # Cleanup any previous failed attempts
                Write-Host "Cleaning up any previous failed attempts..."
                $existingEntities = Get-DataverseEntityMetadata | Where-Object { $_.LogicalName -like "new_e2erel*" }
                foreach ($entity in $existingEntities) {
                    try {
                        Write-Host "  Removing leftover entity: $($entity.LogicalName)"
                        Remove-DataverseEntityMetadata -EntityName $entity.LogicalName -Confirm:$false -ErrorAction SilentlyContinue
                    } catch {
                        Write-Host "  Could not remove $($entity.LogicalName): $_"
                    }
                }
                
                Write-Host "Step 1: Creating first test entity..."
                Set-DataverseEntityMetadata `
                    -EntityName $entity1Name `
                    -SchemaName $entity1Schema `
                    -DisplayName "Relationship Test Entity 1" `
                    -DisplayCollectionName "Relationship Test Entities 1" `
                    -PrimaryAttributeSchemaName "new_name" `
                    -OwnershipType UserOwned `
                    -Confirm:$false
                
                Invoke-DataverseRequest -Connection $connection -Request @{
                    '@odata.type' = 'Microsoft.Crm.Sdk.Messages.PublishXmlRequest'
                    ParameterXml = "<importexportxml><entities><entity>$entity1Name</entity></entities></importexportxml>"
                }
                Write-Host "✓ Entity 1 created"
                
                Write-Host "Step 2: Creating second test entity..."
                Set-DataverseEntityMetadata `
                    -EntityName $entity2Name `
                    -SchemaName $entity2Schema `
                    -DisplayName "Relationship Test Entity 2" `
                    -DisplayCollectionName "Relationship Test Entities 2" `
                    -PrimaryAttributeSchemaName "new_name" `
                    -OwnershipType UserOwned `
                    -Confirm:$false
                
                Invoke-DataverseRequest -Connection $connection -Request @{
                    '@odata.type' = 'Microsoft.Crm.Sdk.Messages.PublishXmlRequest'
                    ParameterXml = "<importexportxml><entities><entity>$entity2Name</entity></entities></importexportxml>"
                }
                Write-Host "✓ Entity 2 created"
                
                Write-Host "Step 3: Creating OneToMany relationship with lookup..."
                $relName1 = "${entity1Name}_${entity2Name}_rel1"
                $lookupSchemaName = "new_Entity1Id"
                
                Set-DataverseRelationshipMetadata `
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
                
                Write-Host "✓ OneToMany relationship created"
                
                # Publish relationship
                Invoke-DataverseRequest -Connection $connection -Request @{
                    '@odata.type' = 'Microsoft.Crm.Sdk.Messages.PublishXmlRequest'
                    ParameterXml = "<importexportxml><entities><entity>$entity1Name</entity><entity>$entity2Name</entity></entities></importexportxml>"
                }
                
                Write-Host "Step 4: Creating self-referencing OneToMany relationship..."
                $selfRelName = "${entity1Name}_parent_rel"
                $parentLookupSchemaName = "new_ParentId"
                
                Set-DataverseRelationshipMetadata `
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
                
                Write-Host "✓ Self-referencing relationship created"
                
                # Publish
                Invoke-DataverseRequest -Connection $connection -Request @{
                    '@odata.type' = 'Microsoft.Crm.Sdk.Messages.PublishXmlRequest'
                    ParameterXml = "<importexportxml><entities><entity>$entity1Name</entity></entities></importexportxml>"
                }
                
                Write-Host "Step 5: Creating ManyToMany relationship..."
                $m2mRelName = "${entity1Name}_${entity2Name}_m2m"
                
                Set-DataverseRelationshipMetadata `
                    -SchemaName $m2mRelName `
                    -RelationshipType ManyToMany `
                    -ReferencedEntity $entity1Name `
                    -ReferencingEntity $entity2Name `
                    -IntersectEntitySchemaName "${m2mRelName}_intersect" `
                    -Confirm:$false
                
                Write-Host "✓ ManyToMany relationship created"
                
                # Publish
                Invoke-DataverseRequest -Connection $connection -Request @{
                    '@odata.type' = 'Microsoft.Crm.Sdk.Messages.PublishXmlRequest'
                    ParameterXml = "<importexportxml><entities><entity>$entity1Name</entity><entity>$entity2Name</entity></entities></importexportxml>"
                }
                
                Write-Host "Step 6: Reading and verifying relationships..."
                $relationships = Get-DataverseRelationshipMetadata -EntityName $entity1Name
                
                $ourRelationships = $relationships | Where-Object { 
                    $_.SchemaName -eq $relName1 -or 
                    $_.SchemaName -eq $selfRelName -or 
                    $_.SchemaName -eq $m2mRelName 
                }
                
                if ($ourRelationships.Count -ne 3) {
                    throw "Expected 3 relationships, found $($ourRelationships.Count)"
                }
                Write-Host "✓ All relationships verified"
                
                Write-Host "Step 7: Reading specific OneToMany relationship..."
                $rel1 = Get-DataverseRelationshipMetadata -SchemaName $relName1
                
                if ($rel1.RelationshipType -ne 'OneToManyRelationship') {
                    throw "Wrong relationship type for OneToMany"
                }
                if ($rel1.ReferencedEntity -ne $entity1Name) {
                    throw "Wrong referenced entity"
                }
                if ($rel1.ReferencingEntity -ne $entity2Name) {
                    throw "Wrong referencing entity"
                }
                Write-Host "✓ OneToMany relationship details verified"
                
                Write-Host "Step 8: Reading specific ManyToMany relationship..."
                $m2mRel = Get-DataverseRelationshipMetadata -SchemaName $m2mRelName
                
                if ($m2mRel.RelationshipType -ne 'ManyToManyRelationship') {
                    throw "Wrong relationship type for ManyToMany"
                }
                Write-Host "✓ ManyToMany relationship details verified"
                
                Write-Host "Step 9: Updating OneToMany relationship cascade behaviors..."
                Set-DataverseRelationshipMetadata `
                    -SchemaName $relName1 `
                    -RelationshipType OneToMany `
                    -ReferencedEntity $entity1Name `
                    -ReferencingEntity $entity2Name `
                    -CascadeDelete Cascade `
                    -CascadeAssign Cascade `
                    -Confirm:$false
                
                Write-Host "✓ Relationship updated"
                
                # Publish updates
                Invoke-DataverseRequest -Connection $connection -Request @{
                    '@odata.type' = 'Microsoft.Crm.Sdk.Messages.PublishXmlRequest'
                    ParameterXml = "<importexportxml><entities><entity>$entity1Name</entity><entity>$entity2Name</entity></entities></importexportxml>"
                }
                
                Write-Host "Step 10: Verifying relationship update..."
                $updatedRel = Get-DataverseRelationshipMetadata -SchemaName $relName1
                
                if ($updatedRel.CascadeConfiguration.Delete -ne 'Cascade') {
                    throw "Cascade delete not updated"
                }
                if ($updatedRel.CascadeConfiguration.Assign -ne 'Cascade') {
                    throw "Cascade assign not updated"
                }
                Write-Host "✓ Relationship updates verified"
                
                Write-Host "Step 11: Testing relationship retrieval with filters..."
                $oneToManyRels = Get-DataverseRelationshipMetadata -EntityName $entity1Name -RelationshipType OneToMany
                $manyToManyRels = Get-DataverseRelationshipMetadata -EntityName $entity1Name -RelationshipType ManyToMany
                
                Write-Host "  OneToMany: $($oneToManyRels.Count), ManyToMany: $($manyToManyRels.Count)"
                Write-Host "✓ Filtered retrieval successful"
                
                Write-Host "Step 12: Cleanup - Deleting relationships (implicit via entity deletion)..."
                # Note: Relationships are deleted when entities are deleted
                
                Write-Host "Step 13: Cleanup - Deleting test entities..."
                Remove-DataverseEntityMetadata -EntityName $entity2Name -Confirm:$false
                Write-Host "✓ Entity 2 deleted"
                
                Remove-DataverseEntityMetadata -EntityName $entity1Name -Confirm:$false
                Write-Host "✓ Entity 1 deleted"
                
                Write-Host "Step 14: Verifying cleanup..."
                Start-Sleep -Seconds 2
                $remainingEntities = Get-DataverseEntityMetadata | Where-Object { 
                    $_.LogicalName -eq $entity1Name -or $_.LogicalName -eq $entity2Name 
                }
                if ($remainingEntities) {
                    throw "Entities still exist after deletion"
                }
                Write-Host "✓ Cleanup verified"
                
                Write-Host "SUCCESS: All relationship metadata operations completed successfully"
                
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
