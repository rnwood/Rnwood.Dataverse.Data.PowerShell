$ErrorActionPreference = "Stop"

Describe "View Manipulation" {

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

    It "Can perform full lifecycle of view manipulation including empty layoutxml update" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

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
                        return  # Success
                    }
                    catch {
                        if ($attempt -eq $MaxRetries) {
                            Write-Error "All $MaxRetries attempts failed. Last error: $_"
                            throw
                        }

                        Write-Warning "Attempt $attempt failed: $_. Retrying in $delay seconds..."
                        Start-Sleep -Seconds $delay
                        $delay = $delay * 2
                    }
                }
            }

            try {
                # Connect to environment
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Generate unique test identifiers to avoid conflicts with parallel CI runs
                $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
                $viewName = "E2E Test View $testRunId"
                
                Write-Host "Using test view name: $viewName"
                
                # --- CLEANUP: Remove any previously failed test views ---
                Write-Host "Cleaning up any existing test views from previous failed runs..."
                $existingViews = Invoke-WithRetry {
                    Get-DataverseRecord -Connection $connection -TableName savedquery -FilterValues @{
                        "name:Like" = "E2E Test View %"
                        "returnedtypecode" = "contact"
                    } -Columns savedqueryid, name -ErrorAction SilentlyContinue
                }
                
                if ($existingViews) {
                    foreach ($view in $existingViews) {
                        Write-Host "  Removing old view: $($view.name) (ID: $($view.savedqueryid))"
                        try {
                            Invoke-WithRetry {
                                Remove-DataverseView -Connection $connection -Id $view.savedqueryid -ViewType "System" -Confirm:$false -IfExists
                            }
                        } catch {
                            Write-Warning "Failed to remove old view $($view.name): $_"
                        }
                    }
                }
                Write-Host "Cleanup complete."
                
                # --- TEST 1: Create a new view with FetchXML ---
                Write-Host "`nTest 1: Creating new view with FetchXML..."
                $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="fullname" />
    <attribute name="emailaddress1" />
    <attribute name="telephone1" />
    <order attribute="fullname" descending="false" />
  </entity>
</fetch>
"@
                
                $viewId = Invoke-WithRetry {
                    Set-DataverseView -Connection $connection `
                        -Name $viewName `
                        -TableName contact `
                        -ViewType "System" `
                        -FetchXml $fetchXml `
                        -Description "E2E test view created by automated tests" `
                        -PassThru `
                        -Confirm:$false
                }
                
                if (-not $viewId) {
                    throw "Failed to create view - no ID returned"
                }
                Write-Host "Created view with ID: $viewId"
                
                # --- TEST 2: Retrieve the created view ---
                Write-Host "`nTest 2: Retrieving view by ID..."
                $retrievedView = Invoke-WithRetry {
                    Get-DataverseView -Connection $connection -Id $viewId
                }
                
                if (-not $retrievedView) {
                    throw "Failed to retrieve view by ID"
                }
                if ($retrievedView.Name -ne $viewName) {
                    throw "Retrieved view Name mismatch. Expected: $viewName, Got: $($retrievedView.Name)"
                }
                Write-Host "Successfully retrieved view: $($retrievedView.Name)"
                
                # --- TEST 3: Update view columns (bug fix scenario - empty layoutxml) ---
                Write-Host "`nTest 3: Updating view columns (testing empty layoutxml bug fix)..."
                
                # First, get the current view to check its state
                $currentView = Invoke-WithRetry {
                    Get-DataverseRecord -Connection $connection -TableName savedquery -Id $viewId -Columns fetchxml, layoutxml
                }
                
                Write-Host "Current view state - FetchXML length: $($currentView.fetchxml.Length), LayoutXML length: $($currentView.layoutxml.Length)"
                
                # Now update with Columns parameter - this is the bug fix scenario
                # Even if layoutxml is empty or malformed, this should work
                Invoke-WithRetry {
                    Set-DataverseView -Connection $connection `
                        -Id $viewId `
                        -ViewType "System" `
                        -Columns @(
                            @{ name = "fullname"; width = 200 },
                            @{ name = "emailaddress1"; width = 150 },
                            @{ name = "telephone1"; width = 100 }
                        ) `
                        -Confirm:$false
                }
                
                Write-Host "Successfully updated view columns (bug fix validated)"
                
                # --- TEST 4: Verify the columns were updated ---
                Write-Host "`nTest 4: Verifying column update..."
                $updatedView = Invoke-WithRetry {
                    Get-DataverseRecord -Connection $connection -TableName savedquery -Id $viewId -Columns layoutxml
                }
                
                if ([string]::IsNullOrWhiteSpace($updatedView.layoutxml)) {
                    throw "LayoutXML is empty after update"
                }
                
                # Check that layoutxml contains the expected columns
                if ($updatedView.layoutxml -notmatch "fullname" -or 
                    $updatedView.layoutxml -notmatch "emailaddress1" -or 
                    $updatedView.layoutxml -notmatch "telephone1") {
                    throw "LayoutXML does not contain expected columns"
                }
                
                Write-Host "Successfully verified column update in layoutxml"
                
                # --- TEST 5: Add columns to existing view ---
                Write-Host "`nTest 5: Adding columns to view..."
                Invoke-WithRetry {
                    Set-DataverseView -Connection $connection `
                        -Id $viewId `
                        -ViewType "System" `
                        -AddColumns @("createdon") `
                        -Confirm:$false
                }
                
                Write-Host "Successfully added columns to view"
                
                # --- TEST 6: Update view name and description ---
                Write-Host "`nTest 6: Updating view name and description..."
                $updatedViewName = "$viewName - Updated"
                Invoke-WithRetry {
                    Set-DataverseView -Connection $connection `
                        -Id $viewId `
                        -ViewType "System" `
                        -Name $updatedViewName `
                        -Description "Updated description" `
                        -Confirm:$false
                }
                
                Write-Host "Successfully updated view metadata"
                
                # Verify the update
                $verifyView = Invoke-WithRetry {
                    Get-DataverseView -Connection $connection -Id $viewId
                }
                
                if ($verifyView.Name -ne $updatedViewName) {
                    throw "View name was not updated. Expected: $updatedViewName, Got: $($verifyView.Name)"
                }
                
                Write-Host "Verified view name update"
                
                # --- CLEANUP: Remove the test view ---
                Write-Host "`nCleaning up: Removing test view..."
                Invoke-WithRetry {
                    Remove-DataverseView -Connection $connection -Id $viewId -ViewType "System" -Confirm:$false
                }
                
                Write-Host "Successfully removed test view"
                
                # Verify removal
                $removedView = Invoke-WithRetry {
                    Get-DataverseRecord -Connection $connection -TableName savedquery -Id $viewId -ErrorAction SilentlyContinue
                }
                
                if ($removedView) {
                    throw "View was not removed successfully"
                }
                
                Write-Host "`nAll tests completed successfully!"
                
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
