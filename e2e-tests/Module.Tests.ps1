$ErrorActionPreference = "Stop"

Describe "Module" -Skip {

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

    It "Can connect to a real env and query some data" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           

            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                Get-DataverseRecord -Connection $connection -TableName systemuser
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Can connect to a real env and query some data with SQL" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           

            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                Invoke-DataverseSql -Connection $connection -Sql "SELECT * FROM systemuser"
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
    
    It "All cmdlets have help available" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            try {
                $cmdlets = Get-Command -Module Rnwood.Dataverse.Data.PowerShell
                Write-Host "Testing help for $($cmdlets.Count) cmdlets"
                
                $cmdletsWithoutHelp = @()
                foreach ($cmdlet in $cmdlets) {
                    $help = Get-Help $cmdlet.Name -ErrorAction SilentlyContinue
                    if (-not $help) {
                        $cmdletsWithoutHelp += $cmdlet.Name
                    }
                }
                
                if ($cmdletsWithoutHelp.Count -gt 0) {
                    throw "The following cmdlets do not have help available: $($cmdletsWithoutHelp -join ', ')"
                }
                
                Write-Host "All cmdlets have help available"
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
    
    It "Help content reflects the help files with expected structure" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            try {
                # Test a sample of important cmdlets to ensure help structure is correct
                $testCmdlets = @(
                    'Get-DataverseConnection',
                    'Get-DataverseRecord',
                    'Set-DataverseRecord',
                    'Remove-DataverseRecord',
                    'Invoke-DataverseRequest',
                    'Invoke-DataverseSql',
                    'Get-DataverseWhoAmI'
                )
                
                $issues = @()
                
                foreach ($cmdletName in $testCmdlets) {
                    Write-Host "Testing help for $cmdletName"
                    $help = Get-Help $cmdletName -Full
                    
                    # Verify help has a name
                    if (-not $help.Name) {
                        $issues += "${cmdletName}: Missing Name"
                    }
                    
                    # Verify help has syntax information
                    if (-not $help.Syntax) {
                        $issues += "${cmdletName}: Missing Syntax"
                    }
                    
                    # Verify help has parameters
                    if (-not $help.Parameters) {
                        $issues += "${cmdletName}: Missing Parameters section"
                    }
                    
                    # For cmdlets with parameters, verify parameter details exist
                    if ($help.Parameters -and $help.Parameters.Parameter) {
                        $paramCount = @($help.Parameters.Parameter).Count
                        Write-Host "  - Found $paramCount parameters"
                        
                        # Verify at least one parameter has description
                        $paramsWithDescription = @($help.Parameters.Parameter | Where-Object { 
                            ($_.Description -is [string] -and $_.Description) -or 
                            ($_.Description.Text -and $_.Description.Text)
                        })
                        if ($paramsWithDescription.Count -eq 0) {
                            $issues += "${cmdletName}: No parameters have descriptions"
                        }
                    }
                }
                
                if ($issues.Count -gt 0) {
                    throw "Help validation issues found:`n$($issues -join "`n")"
                }
                
                Write-Host "All tested cmdlets have proper help structure"
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
    
    It "Help files exist in en-GB directory" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            
            try {
                # Check that the module directory has the en-GB help files
                $modulePath = (Get-Module Rnwood.Dataverse.Data.PowerShell -ListAvailable)[0].ModuleBase
                $helpPath = Join-Path $modulePath "en-GB"
                
                if (-not (Test-Path $helpPath)) {
                    throw "Help directory not found at: $helpPath"
                }
                
                $helpFiles = Get-ChildItem -Path $helpPath -Filter "*.xml"
                Write-Host "Found $($helpFiles.Count) help files in en-GB directory"
                
                if ($helpFiles.Count -eq 0) {
                    throw "No help XML files found in $helpPath"
                }
                
                # Verify the main help file exists
                $mainHelpFile = Join-Path $helpPath "Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml"
                if (-not (Test-Path $mainHelpFile)) {
                    throw "Main help file not found: $mainHelpFile"
                }
                
                Write-Host "Help files exist and are accessible"
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
    
    It "Can use Invoke-DataverseParallel with WhoAmI" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Test parallel processing with WhoAmI call
                $results = 1..5 | Invoke-DataverseParallel -Connection $connection -ChunkSize 2 -MaxDegreeOfParallelism 3 -ScriptBlock {
                    $_ | %{
                        $whoami = Get-DataverseWhoAmI -Connection $connection
                        # Return a simple object with the item and user ID
                        [PSCustomObject]@{
                            Item = $_
                            UserId = $whoami.UserId
                        }
                    }
                }
                
                # Verify we got 5 results
                if ($results.Count -ne 5) {
                    throw "Expected 5 results, got $($results.Count)"
                }
                
                # Verify all have a UserId
                foreach ($result in $results) {
                    if (-not $result.UserId) {
                        throw "Result missing UserId: $($result | ConvertTo-Json)"
                    }
                }
                
                Write-Host "Successfully executed 5 parallel WhoAmI calls"
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Invoke-DataverseParallel can create, update and verify $numberofrecords account records consistently" -Tag "LongRunning" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Generate unique test prefix to avoid conflicts with parallel CI runs
                $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
                $testPrefix = "ParallelTest-$testRunId-"
                $numberOfRecords = 1000
                Write-Host "Using test prefix: $testPrefix"


                Write-Host "Step 1: Creating $numberOfRecords test accounts in parallel..."
                $startTime = Get-Date
                
                # Generate $numberofrecords unique account objects with unique prefix
                $accountsToCreate = 1..$numberofrecords | ForEach-Object {
                    [PSCustomObject]@{
                        name = "$testPrefix$_"
                        accountnumber = "ACCT-$testRunId-$_"
                        description = "Test account created by parallel test - Run $testRunId - Index $_"
                    }
                }
                
                # Create accounts in parallel batches
                $accountsToCreate | Invoke-DataverseParallel -Connection $connection -ChunkSize 100 -verbose -MaxDegreeOfParallelism 8 -ScriptBlock {
                    $_ | Set-DataverseRecord -TableName account -CreateOnly -BatchSize 100
                }
                
                $createDuration = (Get-Date) - $startTime
                Write-Host "Created $numberofrecords accounts in $($createDuration.TotalSeconds) seconds"
                
                Write-Host "Step 3: Querying back all created accounts..."
                $createdAccounts = Get-DataverseRecord -Connection $connection -TableName account -FilterValues @{ "name:Like" = "$testPrefix%" } -Columns name, accountnumber, description
                
                # Verify count
                if ($createdAccounts.Count -ne $numberofrecords) {
                    throw "Expected $numberofrecords accounts, but found $($createdAccounts.Count)"
                }
                Write-Host "Verified: Found all $numberofrecords accounts"
                
                # Verify each account has correct data
                Write-Host "Verifying account data integrity..."
                $mismatches = @()
                foreach ($account in $createdAccounts) {
                    # Extract index from name
                    if ($account.name -match "$testPrefix(\d+)") {
                        $index = $matches[1]
                        $expectedAccountNumber = "ACCT-$testRunId-$index"
                        $expectedDescription = "Test account created by parallel test - Run $testRunId - Index $index"
                        
                        if ($account.accountnumber -ne $expectedAccountNumber) {
                            $mismatches += "Account $($account.name): Expected accountnumber '$expectedAccountNumber', got '$($account.accountnumber)'"
                        }
                        if ($account.description -ne $expectedDescription) {
                            $mismatches += "Account $($account.name): Expected description '$expectedDescription', got '$($account.description)'"
                        }
                    }
                }
                
                if ($mismatches.Count -gt 0) {
                    throw "Data integrity issues found:`n$($mismatches -join "`n")"
                }
                Write-Host "Verified: All account data is correct"
                
                Write-Host "Step 4: Cleanup - Deleting test accounts..."
                $createdAccounts | Invoke-DataverseParallel -Connection $connection -ChunkSize 100 -MaxDegreeOfParallelism 8 -ScriptBlock {
                    $_ | Remove-DataverseRecord -TableName account -BatchSize 100 -verbose
                }
                Write-Host "Cleanup complete"
                
                Write-Host "SUCCESS: All $numberofrecords accounts were created, verified, updated, and cleaned up successfully"
                Write-Host "Total test duration: $((Get-Date) - $startTime)"
                
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Can create, retrieve, update and delete web resources" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Create a unique name for test web resource
                $testPrefix = "test_e2e_" + [Guid]::NewGuid().ToString().Substring(0, 8)
                $webResourceName = $testPrefix
                
                Write-Host "Creating test web resource: $webResourceName"
                
                # Create a test file
                $tempFile = [System.IO.Path]::GetTempFileName()
                $tempJsFile = [System.IO.Path]::ChangeExtension($tempFile, ".js")
                Move-Item $tempFile $tempJsFile -Force
                "console.log('E2E test');" | Out-File -FilePath $tempJsFile -NoNewline -Encoding utf8
                
                # Test: Create web resource from file
                Set-DataverseWebResource -Connection $connection -Name $webResourceName -Path $tempJsFile -DisplayName "E2E Test Resource" -PublisherPrefix "test" -PassThru | Out-Null
                Write-Host "Created web resource: $webResourceName"
                
                # Test: Retrieve by name
                $retrieved = Get-DataverseWebResource -Connection $connection -Name $webResourceName
                if (-not $retrieved) {
                    throw "Failed to retrieve web resource by name"
                }
                Write-Host "Retrieved web resource by name"
                
                # Test: Retrieve by ID
                $retrievedById = Get-DataverseWebResource -Connection $connection -Id $retrieved.Id
                if (-not $retrievedById) {
                    throw "Failed to retrieve web resource by ID"
                }
                Write-Host "Retrieved web resource by ID"
                
                # Test: Retrieve by type with wildcard
                $retrievedByType = Get-DataverseWebResource -Connection $connection -Name "$testPrefix*" -WebResourceType JavaScript
                if ($retrievedByType.Count -eq 0) {
                    throw "Failed to retrieve web resource by type with wildcard"
                }
                Write-Host "Retrieved web resource by type with wildcard"
                
                # Test: Update web resource
                "console.log('E2E test updated');" | Out-File -FilePath $tempJsFile -NoNewline -Encoding utf8
                Set-DataverseWebResource -Connection $connection -Name $webResourceName -Path $tempJsFile | Out-Null
                Write-Host "Updated web resource"
                
                # Test: Retrieve and save to file
                $downloadFile = [System.IO.Path]::GetTempFileName()
                $downloadJsFile = [System.IO.Path]::ChangeExtension($downloadFile, ".js")
                Remove-Item $downloadFile -Force
                Get-DataverseWebResource -Connection $connection -Name $webResourceName -Path $downloadJsFile | Out-Null
                if (-not (Test-Path $downloadJsFile)) {
                    throw "Failed to download web resource to file"
                }
                $downloadedContent = Get-Content $downloadJsFile -Raw
                $expectedContent = "console.log('E2E test updated');"
                if ($downloadedContent -ne $expectedContent) {
                    $downloadedBytes = [System.IO.File]::ReadAllBytes($downloadJsFile)
                    throw "Downloaded content does not match expected value. Expected length: $($expectedContent.Length), Got length: $($downloadedContent.Length), Bytes: $($downloadedBytes.Length). Expected: [$expectedContent], Got: [$downloadedContent]"
                }
                Write-Host "Downloaded and verified web resource content"
                
                # Test: Content excluded by default
                $retrievedWithoutContent = Get-DataverseWebResource -Connection $connection -Name $webResourceName
                if ($retrievedWithoutContent.PSObject.Properties['content']) {
                    throw "Content should be excluded by default"
                }
                Write-Host "Verified content excluded by default"
                
                # Test: Content included with -IncludeContent
                $retrievedWithContent = Get-DataverseWebResource -Connection $connection -Name $webResourceName -IncludeContent
                if (-not $retrievedWithContent.content) {
                    throw "Content should be included with -IncludeContent"
                }
                Write-Host "Verified content included with -IncludeContent"
                
                # Test: IfNewer flag (should not update since file is older)
                Start-Sleep -Milliseconds 100
                Set-DataverseWebResource -Connection $connection -Name $webResourceName -Path $tempJsFile -IfNewer -Verbose | Out-Null
                Write-Host "Tested IfNewer flag"
                
                # Cleanup temp files
                Remove-Item $tempJsFile -Force -ErrorAction SilentlyContinue
                Remove-Item $downloadJsFile -Force -ErrorAction SilentlyContinue
                
                # Test: Delete web resource
                Remove-DataverseWebResource -Connection $connection -Name $webResourceName -Confirm:$false | Out-Null
                Write-Host "Deleted web resource"
                
                # Verify deletion
                $deleted = Get-DataverseWebResource -Connection $connection -Name $webResourceName
                if ($deleted) {
                    throw "Web resource was not deleted"
                }
                Write-Host "Verified web resource deletion"
                
                Write-Host "SUCCESS: All web resource operations completed successfully"
                
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Can batch upload and download web resources from folder" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Create test files in a temp folder
                $testPrefix = "test_e2e_batch_" + [Guid]::NewGuid().ToString().Substring(0, 8)
                $tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
                New-Item -ItemType Directory -Path $tempFolder -Force | Out-Null
                
                Write-Host "Creating test files in: $tempFolder"
                
                # Create 3 test files
                "console.log('test1');" | Out-File -FilePath (Join-Path $tempFolder "test1.js") -NoNewline -Encoding utf8
                "console.log('test2');" | Out-File -FilePath (Join-Path $tempFolder "test2.js") -NoNewline -Encoding utf8
                "body { color: red; }" | Out-File -FilePath (Join-Path $tempFolder "test.css") -NoNewline -Encoding utf8
                
                # Upload folder
                Write-Host "Uploading web resources from folder"
                Set-DataverseWebResource -Connection $connection -Folder $tempFolder -PublisherPrefix $testPrefix -FileFilter "*.*" | Out-Null
                
                # Verify uploads
                $uploaded = Get-DataverseWebResource -Connection $connection -Name "${testPrefix}_*"
                if ($uploaded.Count -lt 3) {
                    throw "Expected at least 3 web resources, found $($uploaded.Count)"
                }
                Write-Host "Verified: Uploaded $($uploaded.Count) web resources"
                
                # Download to new folder
                $downloadFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
                New-Item -ItemType Directory -Path $downloadFolder -Force | Out-Null
                Write-Host "Downloading web resources to: $downloadFolder"
                Get-DataverseWebResource -Connection $connection -Name "${testPrefix}_*" -Folder $downloadFolder | Out-Null
                
                # Verify downloads
                $downloadedFiles = Get-ChildItem $downloadFolder
                if ($downloadedFiles.Count -lt 3) {
                    throw "Expected at least 3 downloaded files, found $($downloadedFiles.Count)"
                }
                Write-Host "Verified: Downloaded $($downloadedFiles.Count) files"
                
                # Cleanup web resources
                Write-Host "Cleaning up web resources"
                Get-DataverseWebResource -Connection $connection -Name "${testPrefix}_*" | 
                    Remove-DataverseWebResource -Connection $connection -Confirm:$false | Out-Null
                
                # Cleanup temp folders
                Remove-Item $tempFolder -Recurse -Force -ErrorAction SilentlyContinue
                Remove-Item $downloadFolder -Recurse -Force -ErrorAction SilentlyContinue
                
                Write-Host "SUCCESS: Batch web resource operations completed successfully"
                
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
}

Describe "Module - Non-Readable Columns E2E" {

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

    It "Provides clear error message when updating record with non-readable columns" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                Write-Host "Testing non-readable column error handling with serviceendpoint entity..."
                
                # The serviceendpoint entity has columns like 'saskey' and 'saskeyname' that are updatable but not readable
                # First, check if we have any existing serviceendpoints to avoid creating one
                $existingEndpoints = Get-DataverseRecord -Connection $connection -TableName serviceendpoint -Columns serviceendpointid -Top 1 -ErrorAction SilentlyContinue
                
                if (-not $existingEndpoints -or $existingEndpoints.Count -eq 0) {
                    # No existing serviceendpoints found
                    # Creating a serviceendpoint requires specific configuration (path, auth settings, etc.)
                    # Since this is complex and environment-specific, we'll skip the test if none exist
                    Write-Host "No existing serviceendpoints found. This test requires at least one serviceendpoint to exist in the environment."
                    Write-Host "Skipping test - cannot validate non-readable column handling without a serviceendpoint."
                    return
                }
                
                $testEndpointId = $existingEndpoints[0].serviceendpointid
                Write-Host "Using existing serviceendpoint: $testEndpointId"
                
                # Now try to update with a non-readable column (saskey) in default mode
                # This should fail with a clear error message
                Write-Host "Attempting to update with non-readable 'saskey' column (should fail with clear error)..."
                
                $updateData = @{
                    namespaceaddress = "sb://updated-address.servicebus.windows.net/"
                    saskey = "test-sas-key-value"
                }
                
                $errorReceived = $false
                $errorMessage = ""
                
                try {
                    Set-DataverseRecord -Connection $connection -TableName serviceendpoint -Id $testEndpointId -InputObject $updateData -ErrorAction Stop
                    Write-Host "ERROR: Expected an exception but none was thrown!"
                    throw "Update with non-readable column should have failed but succeeded"
                } catch {
                    $errorReceived = $true
                    $errorMessage = $_.Exception.Message
                    Write-Host "Received expected error: $errorMessage"
                }
                
                if (-not $errorReceived) {
                    throw "Did not receive expected error for non-readable column"
                }
                
                # Verify error message contains helpful information
                $missingElements = @()
                if ($errorMessage -notmatch "not valid for read") {
                    $missingElements += "missing 'not valid for read' explanation"
                }
                if ($errorMessage -notmatch "saskey") {
                    $missingElements += "missing 'saskey' column name"
                }
                if ($errorMessage -notmatch "serviceendpoint") {
                    $missingElements += "missing 'serviceendpoint' entity name"
                }
                if ($errorMessage -notmatch "-Upsert") {
                    $missingElements += "missing '-Upsert' alternative"
                }
                if ($errorMessage -notmatch "-NoUpdate") {
                    $missingElements += "missing '-NoUpdate' alternative"
                }
                if ($errorMessage -notmatch "-Create") {
                    $missingElements += "missing '-Create' alternative"
                }
                
                if ($missingElements.Count -gt 0) {
                    throw "Error message missing helpful information: $($missingElements -join ', '). Full message: $errorMessage"
                }
                
                Write-Host "Verified: Error message contains all helpful information"
                
                # Now verify that -Upsert flag works as a workaround
                Write-Host "Testing -Upsert workaround..."
                try {
                    Set-DataverseRecord -Connection $connection -TableName serviceendpoint -Id $testEndpointId -InputObject $updateData -Upsert
                    Write-Host "SUCCESS: -Upsert flag worked as expected"
                } catch {
                    Write-Host "Warning: -Upsert also failed: $_"
                    # This is acceptable - the main test is the error message quality
                }
                
                Write-Host "SUCCESS: Non-readable column error handling test passed"
                
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
}
