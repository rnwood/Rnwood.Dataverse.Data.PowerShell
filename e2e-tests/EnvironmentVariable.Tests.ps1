$ErrorActionPreference = "Stop"

Describe "Environment Variable E2E Tests" {

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
            # Delete value records by definition ID (handles mismatched schemaname case)
            Get-DataverseRecord -Connection $Connection -TableName environmentvariablevalue -FilterValues @{ environmentvariabledefinitionid = $DefinitionId } -Columns environmentvariablevalueid -ErrorAction SilentlyContinue | ForEach-Object {
                Remove-DataverseRecord -Connection $Connection -TableName environmentvariablevalue -Id $_.environmentvariablevalueid -Confirm:$false -ErrorAction SilentlyContinue
            }
        } catch { }
        
        try {
            Remove-DataverseEnvironmentVariableDefinition -Connection $Connection -SchemaName $SchemaName -Confirm:$false -ErrorAction SilentlyContinue
        } catch { }
    }

    It "Set-DataverseEnvironmentVariableValue can create a new environment variable value" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            
            # Generate unique schema name for test
            $testPrefix = "e2etest_" + [Guid]::NewGuid().ToString("N").Substring(0, 8)
            $schemaName = "${testPrefix}_TestVar"
            $defId = $null
            Write-Host "Using schema name: $schemaName"
            
            try {
                # First, create the environment variable definition
                Write-Host "Creating environment variable definition..."
                $defRecord = [PSCustomObject]@{
                    schemaname = $schemaName
                    displayname = "E2E Test Variable"
                    type = 100000000  # String type
                }
                $createdDef = $defRecord | Set-DataverseRecord -Connection $connection -TableName environmentvariabledefinition -CreateOnly -PassThru
                $defId = $createdDef.environmentvariabledefinitionid
                Write-Host "Created definition with ID: $defId"
                
                # Set the environment variable value using the cmdlet
                Write-Host "Setting environment variable value..."
                Set-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName -Value "TestValue123"
                
                # Verify the value was set
                Write-Host "Verifying environment variable value..."
                $value = Get-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName
                
                if ($value.Value -ne "TestValue123") {
                    throw "Expected value 'TestValue123' but got '$($value.Value)'"
                }
                
                Write-Host "✓ Environment variable value was set correctly"
                
            } finally {
                # Cleanup
                Write-Host "Cleaning up..."
                if ($defId) {
                    Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $defId -SchemaName $schemaName
                }
            }
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Set-DataverseEnvironmentVariableValue can update an existing environment variable value" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            
            # Generate unique schema name for test
            $testPrefix = "e2etest_" + [Guid]::NewGuid().ToString("N").Substring(0, 8)
            $schemaName = "${testPrefix}_UpdateVar"
            $defId = $null
            Write-Host "Using schema name: $schemaName"
            
            try {
                # First, create the environment variable definition
                Write-Host "Creating environment variable definition..."
                $defRecord = [PSCustomObject]@{
                    schemaname = $schemaName
                    displayname = "E2E Test Update Variable"
                    type = 100000000  # String type
                }
                $createdDef = $defRecord | Set-DataverseRecord -Connection $connection -TableName environmentvariabledefinition -CreateOnly -PassThru
                $defId = $createdDef.environmentvariabledefinitionid
                Write-Host "Created definition with ID: $defId"
                
                # Create the initial environment variable value
                Write-Host "Setting initial environment variable value..."
                Set-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName -Value "InitialValue"
                
                # Update the environment variable value
                Write-Host "Updating environment variable value..."
                Set-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName -Value "UpdatedValue"
                
                # Verify the value was updated
                Write-Host "Verifying environment variable value..."
                $value = Get-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName
                
                if ($value.Value -ne "UpdatedValue") {
                    throw "Expected value 'UpdatedValue' but got '$($value.Value)'"
                }
                
                Write-Host "✓ Environment variable value was updated correctly"
                
            } finally {
                # Cleanup
                Write-Host "Cleaning up..."
                if ($defId) {
                    Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $defId -SchemaName $schemaName
                }
            }
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Set-DataverseEnvironmentVariableValue can update value when value schemaname differs from definition schemaname" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            
            # Generate unique schema name for test
            $testPrefix = "e2etest_" + [Guid]::NewGuid().ToString("N").Substring(0, 8)
            $schemaName = "${testPrefix}_MismatchVar"
            $defId = $null
            Write-Host "Using schema name: $schemaName"
            
            try {
                # First, create the environment variable definition
                Write-Host "Creating environment variable definition..."
                $defRecord = [PSCustomObject]@{
                    schemaname = $schemaName
                    displayname = "E2E Test Mismatch Variable"
                    type = 100000000  # String type
                }
                $createdDef = $defRecord | Set-DataverseRecord -Connection $connection -TableName environmentvariabledefinition -CreateOnly -PassThru
                $defId = $createdDef.environmentvariabledefinitionid
                Write-Host "Created definition with ID: $defId"
                
                # Simulate the bug scenario: create an environment variable value with a GUID
                # as its schemaname (like older records may have)
                Write-Host "Creating environment variable value with mismatched schemaname (simulating old data)..."
                $fakeGuidSchemaName = [Guid]::NewGuid().ToString()
                $valueRecord = [PSCustomObject]@{
                    schemaname = $fakeGuidSchemaName  # This simulates the old buggy data
                    value = "OldValue"
                    environmentvariabledefinitionid = $defId
                }
                $createdValue = $valueRecord | Set-DataverseRecord -Connection $connection -TableName environmentvariablevalue -CreateOnly -PassThru
                Write-Host "Created value with mismatched schemaname: $fakeGuidSchemaName"
                
                # Now try to update using the correct definition schema name
                # This would fail before the fix because the query looked for values by schemaname
                Write-Host "Updating environment variable value using definition schemaname..."
                Set-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName -Value "NewValue"
                
                # Verify the value was updated (not a new record created)
                Write-Host "Verifying environment variable value..."
                $value = Get-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName
                
                if ($value.Value -ne "NewValue") {
                    throw "Expected value 'NewValue' but got '$($value.Value)'"
                }
                
                # Verify only one value record exists for this definition
                $allValues = Get-DataverseRecord -Connection $connection -TableName environmentvariablevalue -FilterValues @{ environmentvariabledefinitionid = $defId } -Columns environmentvariablevalueid
                if ($allValues.Count -ne 1) {
                    throw "Expected 1 value record but found $($allValues.Count) - the fix should update existing record, not create a new one"
                }
                
                Write-Host "✓ Environment variable value was updated correctly even with mismatched schemaname"
                
            } finally {
                # Cleanup
                Write-Host "Cleaning up..."
                if ($defId) {
                    Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $defId -SchemaName $schemaName
                }
            }
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Get-DataverseEnvironmentVariableValue can retrieve value when schemaname differs from definition" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            
            # Generate unique schema name for test
            $testPrefix = "e2etest_" + [Guid]::NewGuid().ToString("N").Substring(0, 8)
            $schemaName = "${testPrefix}_GetMismatchVar"
            $defId = $null
            Write-Host "Using schema name: $schemaName"
            
            try {
                # First, create the environment variable definition
                Write-Host "Creating environment variable definition..."
                $defRecord = [PSCustomObject]@{
                    schemaname = $schemaName
                    displayname = "E2E Test Get Mismatch Variable"
                    type = 100000000  # String type
                }
                $createdDef = $defRecord | Set-DataverseRecord -Connection $connection -TableName environmentvariabledefinition -CreateOnly -PassThru
                $defId = $createdDef.environmentvariabledefinitionid
                Write-Host "Created definition with ID: $defId"
                
                # Create an environment variable value with a GUID as its schemaname
                Write-Host "Creating environment variable value with mismatched schemaname..."
                $fakeGuidSchemaName = [Guid]::NewGuid().ToString()
                $valueRecord = [PSCustomObject]@{
                    schemaname = $fakeGuidSchemaName
                    value = "TestGetValue"
                    environmentvariabledefinitionid = $defId
                }
                $createdValue = $valueRecord | Set-DataverseRecord -Connection $connection -TableName environmentvariablevalue -CreateOnly -PassThru
                Write-Host "Created value with schemaname: $fakeGuidSchemaName"
                
                # Try to get the value using the correct definition schema name
                Write-Host "Getting environment variable value using definition schemaname..."
                $value = Get-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName
                
                if (-not $value) {
                    throw "Value not found - Get-DataverseEnvironmentVariableValue should find value by joining with definition"
                }
                
                if ($value.Value -ne "TestGetValue") {
                    throw "Expected value 'TestGetValue' but got '$($value.Value)'"
                }
                
                # Verify the SchemaName returned is the correct one from the definition
                if ($value.SchemaName -ne $schemaName) {
                    throw "Expected SchemaName '$schemaName' but got '$($value.SchemaName)'"
                }
                
                Write-Host "✓ Get-DataverseEnvironmentVariableValue correctly found and returned the value"
                
            } finally {
                # Cleanup
                Write-Host "Cleaning up..."
                if ($defId) {
                    Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $defId -SchemaName $schemaName
                }
            }
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Remove-DataverseEnvironmentVariableValue can remove value when schemaname differs from definition" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            
            # Generate unique schema name for test
            $testPrefix = "e2etest_" + [Guid]::NewGuid().ToString("N").Substring(0, 8)
            $schemaName = "${testPrefix}_RemoveMismatchVar"
            $defId = $null
            Write-Host "Using schema name: $schemaName"
            
            try {
                # First, create the environment variable definition
                Write-Host "Creating environment variable definition..."
                $defRecord = [PSCustomObject]@{
                    schemaname = $schemaName
                    displayname = "E2E Test Remove Mismatch Variable"
                    type = 100000000  # String type
                }
                $createdDef = $defRecord | Set-DataverseRecord -Connection $connection -TableName environmentvariabledefinition -CreateOnly -PassThru
                $defId = $createdDef.environmentvariabledefinitionid
                Write-Host "Created definition with ID: $defId"
                
                # Create an environment variable value with a GUID as its schemaname
                Write-Host "Creating environment variable value with mismatched schemaname..."
                $fakeGuidSchemaName = [Guid]::NewGuid().ToString()
                $valueRecord = [PSCustomObject]@{
                    schemaname = $fakeGuidSchemaName
                    value = "TestRemoveValue"
                    environmentvariabledefinitionid = $defId
                }
                $createdValue = $valueRecord | Set-DataverseRecord -Connection $connection -TableName environmentvariablevalue -CreateOnly -PassThru
                Write-Host "Created value with schemaname: $fakeGuidSchemaName"
                
                # Try to remove the value using the correct definition schema name
                Write-Host "Removing environment variable value using definition schemaname..."
                Remove-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName -Confirm:$false
                
                # Verify the value was removed
                $remainingValues = Get-DataverseRecord -Connection $connection -TableName environmentvariablevalue -FilterValues @{ environmentvariabledefinitionid = $defId } -Columns environmentvariablevalueid
                if ($remainingValues) {
                    throw "Value should have been removed but still exists"
                }
                
                Write-Host "✓ Remove-DataverseEnvironmentVariableValue correctly removed the value"
                
            } finally {
                # Cleanup
                Write-Host "Cleaning up..."
                if ($defId) {
                    Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $defId -SchemaName $schemaName
                }
            }
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Set-DataverseEnvironmentVariableValue works with multiple environment variables" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            
            # Generate unique schema names for test
            $testPrefix = "e2etest_" + [Guid]::NewGuid().ToString("N").Substring(0, 8)
            $schemaName1 = "${testPrefix}_MultiVar1"
            $schemaName2 = "${testPrefix}_MultiVar2"
            $defId1 = $null
            $defId2 = $null
            Write-Host "Using schema names: $schemaName1, $schemaName2"
            
            try {
                # Create two environment variable definitions
                Write-Host "Creating environment variable definitions..."
                $defRecord1 = [PSCustomObject]@{
                    schemaname = $schemaName1
                    displayname = "E2E Test Multi Variable 1"
                    type = 100000000
                }
                $createdDef1 = $defRecord1 | Set-DataverseRecord -Connection $connection -TableName environmentvariabledefinition -CreateOnly -PassThru
                $defId1 = $createdDef1.environmentvariabledefinitionid
                
                $defRecord2 = [PSCustomObject]@{
                    schemaname = $schemaName2
                    displayname = "E2E Test Multi Variable 2"
                    type = 100000000
                }
                $createdDef2 = $defRecord2 | Set-DataverseRecord -Connection $connection -TableName environmentvariabledefinition -CreateOnly -PassThru
                $defId2 = $createdDef2.environmentvariabledefinitionid
                
                Write-Host "Created definitions with IDs: $defId1, $defId2"
                
                # Set multiple environment variable values at once
                Write-Host "Setting multiple environment variable values..."
                Set-DataverseEnvironmentVariableValue -Connection $connection -EnvironmentVariableValues @{
                    $schemaName1 = "Value1"
                    $schemaName2 = "Value2"
                }
                
                # Verify both values were set
                Write-Host "Verifying environment variable values..."
                $value1 = Get-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName1
                $value2 = Get-DataverseEnvironmentVariableValue -Connection $connection -SchemaName $schemaName2
                
                if ($value1.Value -ne "Value1") {
                    throw "Expected value 'Value1' for $schemaName1 but got '$($value1.Value)'"
                }
                if ($value2.Value -ne "Value2") {
                    throw "Expected value 'Value2' for $schemaName2 but got '$($value2.Value)'"
                }
                
                Write-Host "✓ Multiple environment variable values were set correctly"
                
            } finally {
                # Cleanup
                Write-Host "Cleaning up..."
                if ($defId1) {
                    Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $defId1 -SchemaName $schemaName1
                }
                if ($defId2) {
                    Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $defId2 -SchemaName $schemaName2
                }
            }
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }
}
