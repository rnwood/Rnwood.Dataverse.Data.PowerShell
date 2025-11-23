$ErrorActionPreference = "Stop"

Describe "Form Library and Event Handler E2E Tests" {

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

    It "Can create, read, update, and delete form libraries and event handlers" {
            
        $ErrorActionPreference = "Stop"
        $ConfirmPreference = 'None'  # Suppress all confirmation prompts in non-interactive mode
        $VerbosePreference = 'Continue'  # Enable verbose output
        
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
                return  # Success, exit function
            }
            catch {
                # Check if this is an EntityCustomization operation error
                if ($_.Exception.Message -like "*Cannot start the requested operation*EntityCustomization*") {
                    Write-Warning "EntityCustomization operation conflict detected. Waiting 2 minutes before retry without incrementing attempt count..."
                    $attempt--  # Don't count this as a retry attempt
                    Start-Sleep -Seconds 120
                    continue
                }
                
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
            # Connect to Dataverse
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}

            # Enable affinity cookie
            $connection.EnableAffinityCookie = $true
            
            # Generate unique test identifier
            $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
            $entityName = "contact"  # Using contact entity which typically has forms
            
            Write-Host "=========================================="
            Write-Host "Starting Form Library and Event Handler E2E Test"
            Write-Host "Test Run ID: $testRunId"
            Write-Host "=========================================="
            
            # ============================================================
            # STEP 1: Get an existing form to work with
            # ============================================================
            Write-Host ""
            Write-Host "Step 1: Getting a contact form to work with..."
            
            $form = Get-DataverseForm -Connection $connection -Entity $entityName -Published | Select-Object -First 1
            if (-not $form) {
                throw "No contact forms found"
            }
            $formId = $form.FormId
            Write-Host "  Using form: $($form.Name) (ID: $formId)"
            Write-Host "✓ Form selected"
            
            # ============================================================
            # STEP 2: Create test web resources
            # ============================================================
            Write-Host ""
            Write-Host "Step 2: Creating test web resources..."
            
            $webResourceName1 = "new_e2etest_$testRunId/library1.js"
            $webResourceName2 = "new_e2etest_$testRunId/library2.js"
            
            # Create temporary JavaScript files
            $tempFile1 = [System.IO.Path]::GetTempFileName()
            $tempJsFile1 = [System.IO.Path]::ChangeExtension($tempFile1, ".js")
            Move-Item $tempFile1 $tempJsFile1 -Force
            "function testFunction1() { console.log('test'); }" | Out-File -FilePath $tempJsFile1 -NoNewline -Encoding utf8
            
            $tempFile2 = [System.IO.Path]::GetTempFileName()
            $tempJsFile2 = [System.IO.Path]::ChangeExtension($tempFile2, ".js")
            Move-Item $tempFile2 $tempJsFile2 -Force
            "function testFunction2() { console.log('test2'); }" | Out-File -FilePath $tempJsFile2 -NoNewline -Encoding utf8
            
            Invoke-WithRetry {
                # Create first web resource using Set-DataverseWebResource
                $wr1 = Set-DataverseWebResource -Connection $connection `
                    -Name $webResourceName1 `
                    -Path $tempJsFile1 `
                    -DisplayName "E2E Test Library 1" `
                    -PassThru `
                    -Confirm:$false
                $script:webResourceId1 = $wr1.webresourceid
                Write-Host "  Created web resource 1: $webResourceName1"
                
                # Create second web resource using Set-DataverseWebResource
                $wr2 = Set-DataverseWebResource -Connection $connection `
                    -Name $webResourceName2 `
                    -Path $tempJsFile2 `
                    -DisplayName "E2E Test Library 2" `
                    -PassThru `
                    -Confirm:$false
                $script:webResourceId2 = $wr2.webresourceid
                Write-Host "  Created web resource 2: $webResourceName2"
            }
            
            # Cleanup temporary files
            Remove-Item $tempJsFile1 -Force -ErrorAction SilentlyContinue
            Remove-Item $tempJsFile2 -Force -ErrorAction SilentlyContinue
            
            Write-Host "✓ Web resources created"
            
            # ============================================================
            # STEP 3: Get initial form state (libraries and handlers)
            # ============================================================
            Write-Host ""
            Write-Host "Step 3: Getting initial form state..."
            
            $initialLibraries = Get-DataverseFormLibrary -Connection $connection -FormId $formId -ErrorAction SilentlyContinue
            $initialLibraryCount = if ($initialLibraries) { @($initialLibraries).Count } else { 0 }
            Write-Host "  Initial libraries count: $initialLibraryCount"
            
            $initialHandlers = Get-DataverseFormEventHandler -Connection $connection -FormId $formId -ErrorAction SilentlyContinue
            $initialHandlerCount = if ($initialHandlers) { @($initialHandlers).Count } else { 0 }
            Write-Host "  Initial handlers count: $initialHandlerCount"
            Write-Host "✓ Initial state captured"
            
            # ============================================================
            # STEP 4: Add libraries to form
            # ============================================================
            Write-Host ""
            Write-Host "Step 4: Adding libraries to form..."
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $script:library1 = Set-DataverseFormLibrary -Connection $connection -FormId $formId -LibraryName $webResourceName1 -SkipPublish -Confirm:$false
                Write-Host "  Added library 1: $($library1.Name)"
                
                $script:library2 = Set-DataverseFormLibrary -Connection $connection -FormId $formId -LibraryName $webResourceName2 -SkipPublish -Confirm:$false
                Write-Host "  Added library 2: $($library2.Name)"
            }
            Write-Host "✓ Libraries added"
            
            # ============================================================
            # STEP 5: Verify libraries were added
            # ============================================================
            Write-Host ""
            Write-Host "Step 5: Verifying libraries were added..."
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $libraries = Get-DataverseFormLibrary -Connection $connection -FormId $formId
                $libraryCount = @($libraries).Count
                Write-Host "  Current libraries count: $libraryCount"
                
                if ($libraryCount -ne ($initialLibraryCount + 2)) {
                    throw "Expected $($initialLibraryCount + 2) libraries, found $libraryCount"
                }
                
                $lib1Found = $libraries | Where-Object { $_.Name -eq $webResourceName1 }
                $lib2Found = $libraries | Where-Object { $_.Name -eq $webResourceName2 }
                
                if (-not $lib1Found -or -not $lib2Found) {
                    throw "One or both libraries not found in form"
                }
                Write-Host "  Both libraries found in form"
            }
            Write-Host "✓ Libraries verified"
            
            # ============================================================
            # STEP 6: Add form-level event handler
            # ============================================================
            Write-Host ""
            Write-Host "Step 6: Adding form-level event handler..."
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $script:formHandler = Set-DataverseFormEventHandler -Connection $connection `
                    -FormId $formId `
                    -EventName "onload" `
                    -FunctionName "E2ETestOnLoad_$testRunId" `
                    -LibraryName $webResourceName1 `
                    -SkipPublish `
                    -Confirm:$false
                Write-Host "  Added form handler: $($formHandler.FunctionName)"
            }
            Write-Host "✓ Form handler added"
            
            # ============================================================
            # STEP 7: Verify form handler was added
            # ============================================================
            Write-Host ""
            Write-Host "Step 7: Verifying form handler was added..."
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $handlers = Get-DataverseFormEventHandler -Connection $connection -FormId $formId -EventName "onload"
                $handlerCount = @($handlers).Count
                Write-Host "  Current onload handlers count: $handlerCount"
                
                $ourHandler = $handlers | Where-Object { $_.FunctionName -eq "E2ETestOnLoad_$testRunId" }
                if (-not $ourHandler) {
                    throw "Form handler not found"
                }
                Write-Host "  Handler found: $($ourHandler.FunctionName)"
            }
            Write-Host "✓ Form handler verified"
            
            # ============================================================
            # STEP 8: Remove form handler
            # ============================================================
            Write-Host ""
            Write-Host "Step 8: Removing form handler..."
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Remove-DataverseFormEventHandler -Connection $connection `
                    -FormId $formId `
                    -EventName "onload" `
                    -FunctionName "E2ETestOnLoad_$testRunId" `
                    -LibraryName $webResourceName1 `
                    -SkipPublish `
                    -Confirm:$false
                Write-Host "  Removed form handler"
            }
            Write-Host "✓ Form handler removed"
            
            # ============================================================
            # STEP 9: Verify form handler was removed
            # ============================================================
            Write-Host ""
            Write-Host "Step 9: Verifying form handler was removed..."
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $handlers = Get-DataverseFormEventHandler -Connection $connection -FormId $formId -EventName "onload" -ErrorAction SilentlyContinue
                $ourHandler = $handlers | Where-Object { $_.FunctionName -eq "E2ETestOnLoad_$testRunId" }
                if ($ourHandler) {
                    throw "Handler still exists after deletion"
                }
                Write-Host "  Handler confirmed removed"
            }
            Write-Host "✓ Handler removal verified"
            
            # ============================================================
            # STEP 10: Remove libraries
            # ============================================================
            Write-Host ""
            Write-Host "Step 10: Removing libraries from form..."
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Remove-DataverseFormLibrary -Connection $connection -FormId $formId -LibraryName $webResourceName1 -SkipPublish -Confirm:$false
                Write-Host "  Removed library 1"
                
                Remove-DataverseFormLibrary -Connection $connection -FormId $formId -LibraryName $webResourceName2 -SkipPublish -Confirm:$false
                Write-Host "  Removed library 2"
            }
            Write-Host "✓ Libraries removed"
            
            # ============================================================
            # STEP 11: Verify libraries were removed
            # ============================================================
            Write-Host ""
            Write-Host "Step 11: Verifying libraries were removed..."
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                $libraries = Get-DataverseFormLibrary -Connection $connection -FormId $formId -ErrorAction SilentlyContinue
                $libraryCount = if ($libraries) { @($libraries).Count } else { 0 }
                Write-Host "  Current libraries count: $libraryCount"
                
                if ($libraryCount -ne $initialLibraryCount) {
                    throw "Expected $initialLibraryCount libraries, found $libraryCount"
                }
                
                $lib1Found = $libraries | Where-Object { $_.Name -eq $webResourceName1 }
                $lib2Found = $libraries | Where-Object { $_.Name -eq $webResourceName2 }
                
                if ($lib1Found -or $lib2Found) {
                    throw "One or both libraries still exist after deletion"
                }
                Write-Host "  Both libraries confirmed removed"
            }
            Write-Host "✓ Library removal verified"
            
            # ============================================================
            # STEP 12: Cleanup - Delete test web resources
            # ============================================================
            Write-Host ""
            Write-Host "Step 12: Cleanup - Deleting test web resources..."
            
            Invoke-WithRetry {
                Wait-DataversePublish -Connection $connection -Verbose
                Remove-DataverseWebResource -Connection $connection -Id $webResourceId1 -Confirm:$false
                Write-Host "  Deleted web resource 1"
                
                Remove-DataverseWebResource -Connection $connection -Id $webResourceId2 -Confirm:$false
                Write-Host "  Deleted web resource 2"
            }
            Write-Host "✓ Web resources deleted"
            
            Write-Host ""
            Write-Host "=========================================="
            Write-Host "SUCCESS: All form library and event handler operations completed successfully"
            Write-Host "=========================================="
            
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }
}
