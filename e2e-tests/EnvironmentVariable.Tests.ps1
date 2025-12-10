$ErrorActionPreference = "Stop"

Describe "Environment Variable E2E Tests" -Skip {

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

    It "Can perform all environment variable operations comprehensively" {
        $ErrorActionPreference = "Stop"
        $ConfirmPreference = 'None'
        $VerbosePreference = 'Continue'
        
        # Helper function to clean up environment variable by definition ID
        function Remove-TestEnvironmentVariable {
            param(
                [Parameter(Mandatory)]
                $Connection,
                [Parameter(Mandatory)]
                [string]$DefinitionId,
                [Parameter(Mandatory)]
                [string]$SchemaName
            )
            
            try {
                Get-DataverseRecord -Connection $Connection -TableName environmentvariablevalue -FilterValues @{ environmentvariabledefinitionid = $DefinitionId } -Columns environmentvariablevalueid -ErrorAction SilentlyContinue | ForEach-Object {
                    Remove-DataverseRecord -Connection $Connection -TableName environmentvariablevalue -Id $_.Id -Confirm:$false -ErrorAction SilentlyContinue
                }
            } catch { }
            
            try {
                Remove-DataverseEnvironmentVariableDefinition -Connection $Connection -SchemaName $SchemaName -Confirm:$false -ErrorAction SilentlyContinue
            } catch { }
        }
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            
            # Generate unique test identifier
            $testPrefix = "e2etest_" + [Guid]::NewGuid().ToString("N").Substring(0, 8)
            Write-Host "Test prefix: $testPrefix"
            
            $schemaName1 = "${testPrefix}_Var1"
            $schemaName2 = "${testPrefix}_Var2"
            $schemaNameMismatch = "${testPrefix}_MismatchVar"
            $defId1 = $null
            $defId2 = $null
            $defIdMismatch = $null
            
            try {
                # ==========================================
                # Step 1: Create environment variable definitions
                # ==========================================
                Write-Host "Step 1: Creating environment variable definitions..."
                
                $defRecord1 = [PSCustomObject]@{
                    schemaname = $schemaName1
                    displayname = "E2E Test Variable 1"
                    type = 100000000
                }
                $createdDef1 = $defRecord1 | Set-DataverseRecord -Connection $connection -TableName environmentvariabledefinition -CreateOnly -PassThru
                $defId1 = $createdDef1.Id
                Write-Host "✓ Created definition 1 with ID: $defId1"
                
                $defRecord2 = [PSCustomObject]@{
                    schemaname = $schemaName2
                    displayname = "E2E Test Variable 2"
                    type = 100000000
                }
                $createdDef2 = $defRecord2 | Set-DataverseRecord -Connection $connection -TableName environmentvariabledefinition -CreateOnly -PassThru
                $defId2 = $createdDef2.Id
                Write-Host "✓ Created definition 2 with ID: $defId2"
                
                $defRecordMismatch = [PSCustomObject]@{
                    schemaname = $schemaNameMismatch
                    displayname = "E2E Test Mismatch Variable"
                    type = 100000000
                }
                $createdDefMismatch = $defRecordMismatch | Set-DataverseRecord -Connection $connection -TableName environmentvariabledefinition -CreateOnly -PassThru
                $defIdMismatch = $createdDefMismatch.Id
                Write-Host "✓ Created mismatch test definition with ID: $defIdMismatch"
                
                # ==========================================
                # Step 2: Test creating new environment variable values
                # ==========================================
                Write-Host "Step 2: Creating new environment variable values..."
                
                Set-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName1 -Value "InitialValue1"
                $value1 = Get-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName1
                if ($value1.Value -ne "InitialValue1") {
                    throw "Expected 'InitialValue1' but got '$($value1.Value)'"
                }
                Write-Host "✓ Created and verified value for $schemaName1"
                
                # ==========================================
                # Step 3: Test updating existing environment variable values
                # ==========================================
                Write-Host "Step 3: Updating existing environment variable value..."
                
                Set-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName1 -Value "UpdatedValue1"
                $value1Updated = Get-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName1
                if ($value1Updated.Value -ne "UpdatedValue1") {
                    throw "Expected 'UpdatedValue1' but got '$($value1Updated.Value)'"
                }
                Write-Host "✓ Updated and verified value for $schemaName1"
                
                # ==========================================
                # Step 4: Test setting multiple environment variable values
                # ==========================================
                Write-Host "Step 4: Setting multiple environment variable values..."
                
                Set-DataverseEnvironmentVariableValue -Connection $connection -EnvironmentVariableValues @{
                    $schemaName1 = "MultiValue1"
                    $schemaName2 = "MultiValue2"
                }
                
                $multiValue1 = Get-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName1
                $multiValue2 = Get-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName2
                
                if ($multiValue1.Value -ne "MultiValue1") {
                    throw "Expected 'MultiValue1' for $schemaName1 but got '$($multiValue1.Value)'"
                }
                if ($multiValue2.Value -ne "MultiValue2") {
                    throw "Expected 'MultiValue2' for $schemaName2 but got '$($multiValue2.Value)'"
                }
                Write-Host "✓ Set and verified multiple environment variable values"
                
                # ==========================================
                # Step 5: Test the bug fix - mismatched schemaname scenario
                # This is the critical test for the bug report
                # ==========================================
                Write-Host "Step 5: Testing bug fix - mismatched schemaname scenario..."
                
                # Create a value with a GUID as schemaname (simulating old buggy data)
                $fakeGuidSchemaName = [Guid]::NewGuid().ToString()
                $mismatchValueRecord = [PSCustomObject]@{
                    schemaname = $fakeGuidSchemaName
                    value = "OldMismatchValue"
                    environmentvariabledefinitionid = $defIdMismatch
                }
                $createdMismatchValue = $mismatchValueRecord | Set-DataverseRecord -Connection $connection -TableName environmentvariablevalue -CreateOnly -PassThru
                Write-Host "  Created value with mismatched schemaname: $fakeGuidSchemaName"
                
                # Now try to update using the correct definition schema name
                # Before the fix, this would fail with database constraint violation
                Set-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaNameMismatch -Value "NewMismatchValue"
                
                # Verify the value was updated (not a new record created)
                $mismatchValue = Get-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaNameMismatch
                if ($mismatchValue.Value -ne "NewMismatchValue") {
                    throw "Expected 'NewMismatchValue' but got '$($mismatchValue.Value)'"
                }
                
                # Verify only one value record exists for this definition
                $allMismatchValues = Get-DataverseRecord -Connection $connection -TableName environmentvariablevalue -FilterValues @{ environmentvariabledefinitionid = $defIdMismatch } -Columns environmentvariablevalueid
                if ($allMismatchValues.Count -ne 1) {
                    throw "Expected 1 value record but found $($allMismatchValues.Count) - fix should update existing record, not create new"
                }
                
                # Verify Get-DataverseEnvironmentVariableValue returns correct schema name from definition
                if ($mismatchValue.SchemaName -ne $schemaNameMismatch) {
                    throw "Expected SchemaName '$schemaNameMismatch' but got '$($mismatchValue.SchemaName)'"
                }
                Write-Host "✓ Bug fix verified - mismatched schemaname handled correctly"
                
                # ==========================================
                # Step 6: Test Remove-DataverseEnvironmentVariableValue with mismatched schemaname
                # ==========================================
                Write-Host "Step 6: Testing Remove-DataverseEnvironmentVariableValue..."
                
                Remove-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaNameMismatch -Confirm:$false
                
                $remainingValues = Get-DataverseRecord -Connection $connection -TableName environmentvariablevalue -FilterValues @{ environmentvariabledefinitionid = $defIdMismatch } -Columns environmentvariablevalueid -ErrorAction SilentlyContinue
                if ($remainingValues) {
                    throw "Value should have been removed but still exists"
                }
                Write-Host "✓ Remove-DataverseEnvironmentVariableValue works with mismatched schemaname"
                
                # ==========================================
                # Step 7: Cleanup
                # ==========================================
                Write-Host "Step 7: Cleanup..."
                
                Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $defId1 -SchemaName $schemaName1
                Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $defId2 -SchemaName $schemaName2
                Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $defIdMismatch -SchemaName $schemaNameMismatch
                
                Write-Host "✓ Cleanup complete"
                
                Write-Host ""
                Write-Host "SUCCESS: All environment variable operations completed successfully"
                
            } finally {
                # Final cleanup in case of failures
                if ($defId1) { Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $defId1 -SchemaName $schemaName1 }
                if ($defId2) { Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $defId2 -SchemaName $schemaName2 }
                if ($defIdMismatch) { Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $defIdMismatch -SchemaName $schemaNameMismatch }
            }
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }
}
