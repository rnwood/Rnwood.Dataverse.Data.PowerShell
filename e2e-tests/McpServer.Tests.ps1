$ErrorActionPreference = "Stop"

Describe "MCP Server" {

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

    It "Can save connection, start server, and execute scripts" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            try {
                Write-Host "Step 1: Creating and saving a test connection..."
                # Generate unique connection name for this test
                $testConnectionName = "E2ETest-$([guid]::NewGuid().ToString('N').Substring(0, 8))"
                Write-Host "Using connection name: $testConnectionName"
                
                # Save a connection
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET} -Name $testConnectionName -SetAsDefault
                
                if (-not $connection) {
                    throw "Failed to create connection"
                }
                Write-Host "Connection saved successfully"
                
                # Verify connection was saved
                $savedConnection = Get-DataverseConnection -Name $testConnectionName
                if (-not $savedConnection) {
                    throw "Connection was not saved properly"
                }
                Write-Host "Connection verified in saved connections"
                
                Write-Host "Step 2: Starting MCP server..."
                # Find the MCP server executable
                $mcpServerPath = "$env:ChildProcessPSModulePath/Rnwood.Dataverse.Data.PowerShell/../../Rnwood.Dataverse.Data.PowerShell.McpServer/bin/Debug/net8.0/Rnwood.Dataverse.Data.PowerShell.McpServer.dll"
                $mcpServerPath = [System.IO.Path]::GetFullPath($mcpServerPath)
                
                if (-not (Test-Path $mcpServerPath)) {
                    throw "MCP Server not found at: $mcpServerPath"
                }
                Write-Host "MCP Server found at: $mcpServerPath"
                
                # Start the MCP server process
                $psi = New-Object System.Diagnostics.ProcessStartInfo
                $psi.FileName = "dotnet"
                $psi.Arguments = "$mcpServerPath --connection $testConnectionName"
                $psi.UseShellExecute = $false
                $psi.RedirectStandardInput = $true
                $psi.RedirectStandardOutput = $true
                $psi.RedirectStandardError = $true
                $psi.CreateNoWindow = $true
                
                $process = New-Object System.Diagnostics.Process
                $process.StartInfo = $psi
                
                try {
                    $process.Start() | Out-Null
                    Write-Host "MCP Server started with PID: $($process.Id)"
                    
                    # Give server time to initialize
                    Start-Sleep -Seconds 3
                    
                    if ($process.HasExited) {
                        $stderr = $process.StandardError.ReadToEnd()
                        throw "MCP Server exited unexpectedly. StdErr: $stderr"
                    }
                    
                    Write-Host "Step 3: Testing MCP protocol communication..."
                    
                    # Send initialization request
                    $initRequest = @{
                        jsonrpc = "2.0"
                        id = 1
                        method = "initialize"
                        params = @{
                            protocolVersion = "2024-11-05"
                            capabilities = @{}
                            clientInfo = @{
                                name = "test-client"
                                version = "1.0.0"
                            }
                        }
                    } | ConvertTo-Json -Depth 10 -Compress
                    
                    Write-Host "Sending initialize request..."
                    $process.StandardInput.WriteLine($initRequest)
                    $process.StandardInput.Flush()
                    
                    # Read response with timeout
                    $timeout = 10
                    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
                    $initResponse = $null
                    
                    while ($stopwatch.Elapsed.TotalSeconds -lt $timeout -and -not $initResponse) {
                        if (-not $process.StandardOutput.EndOfStream) {
                            $line = $process.StandardOutput.ReadLine()
                            if ($line) {
                                Write-Host "Received: $line"
                                try {
                                    $initResponse = $line | ConvertFrom-Json
                                    if ($initResponse.id -eq 1) {
                                        break
                                    }
                                } catch {
                                    # Not JSON or not our response, keep reading
                                }
                            }
                        }
                        Start-Sleep -Milliseconds 100
                    }
                    
                    if (-not $initResponse) {
                        throw "Did not receive initialization response within timeout"
                    }
                    
                    Write-Host "Received initialization response"
                    
                    # Send initialized notification
                    $initializedNotif = @{
                        jsonrpc = "2.0"
                        method = "notifications/initialized"
                    } | ConvertTo-Json -Compress
                    
                    $process.StandardInput.WriteLine($initializedNotif)
                    $process.StandardInput.Flush()
                    
                    Write-Host "Step 4: Creating a session..."
                    $createSessionRequest = @{
                        jsonrpc = "2.0"
                        id = 2
                        method = "tools/call"
                        params = @{
                            name = "CreateSession"
                            arguments = @{}
                        }
                    } | ConvertTo-Json -Depth 10 -Compress
                    
                    $process.StandardInput.WriteLine($createSessionRequest)
                    $process.StandardInput.Flush()
                    
                    # Read session creation response
                    $stopwatch.Restart()
                    $sessionResponse = $null
                    
                    while ($stopwatch.Elapsed.TotalSeconds -lt $timeout -and -not $sessionResponse) {
                        if (-not $process.StandardOutput.EndOfStream) {
                            $line = $process.StandardOutput.ReadLine()
                            if ($line) {
                                Write-Host "Received: $line"
                                try {
                                    $response = $line | ConvertFrom-Json
                                    if ($response.id -eq 2) {
                                        $sessionResponse = $response
                                        break
                                    }
                                } catch {
                                    # Not our response, keep reading
                                }
                            }
                        }
                        Start-Sleep -Milliseconds 100
                    }
                    
                    if (-not $sessionResponse -or -not $sessionResponse.result) {
                        throw "Failed to create session"
                    }
                    
                    $sessionId = ($sessionResponse.result.content[0].text | ConvertFrom-Json).sessionId
                    Write-Host "Session created with ID: $sessionId"
                    
                    Write-Host "Step 5: Running a script in the session..."
                    $runScriptRequest = @{
                        jsonrpc = "2.0"
                        id = 3
                        method = "tools/call"
                        params = @{
                            name = "RunScriptInSession"
                            arguments = @{
                                sessionId = $sessionId
                                script = 'Get-DataverseWhoAmI | ConvertTo-Json'
                            }
                        }
                    } | ConvertTo-Json -Depth 10 -Compress
                    
                    $process.StandardInput.WriteLine($runScriptRequest)
                    $process.StandardInput.Flush()
                    
                    # Read script execution response
                    $stopwatch.Restart()
                    $scriptResponse = $null
                    
                    while ($stopwatch.Elapsed.TotalSeconds -lt $timeout -and -not $scriptResponse) {
                        if (-not $process.StandardOutput.EndOfStream) {
                            $line = $process.StandardOutput.ReadLine()
                            if ($line) {
                                Write-Host "Received: $line"
                                try {
                                    $response = $line | ConvertFrom-Json
                                    if ($response.id -eq 3) {
                                        $scriptResponse = $response
                                        break
                                    }
                                } catch {
                                    # Not our response, keep reading
                                }
                            }
                        }
                        Start-Sleep -Milliseconds 100
                    }
                    
                    if (-not $scriptResponse -or -not $scriptResponse.result) {
                        throw "Failed to run script"
                    }
                    
                    $scriptExecutionId = ($scriptResponse.result.content[0].text | ConvertFrom-Json).scriptExecutionId
                    Write-Host "Script execution started with ID: $scriptExecutionId"
                    
                    Write-Host "Step 6: Getting script output..."
                    # Wait a bit for script to complete
                    Start-Sleep -Seconds 2
                    
                    $getOutputRequest = @{
                        jsonrpc = "2.0"
                        id = 4
                        method = "tools/call"
                        params = @{
                            name = "GetScriptOutput"
                            arguments = @{
                                sessionId = $sessionId
                                scriptExecutionId = $scriptExecutionId
                                onlyNew = $false
                            }
                        }
                    } | ConvertTo-Json -Depth 10 -Compress
                    
                    $process.StandardInput.WriteLine($getOutputRequest)
                    $process.StandardInput.Flush()
                    
                    # Read output response
                    $stopwatch.Restart()
                    $outputResponse = $null
                    
                    while ($stopwatch.Elapsed.TotalSeconds -lt $timeout -and -not $outputResponse) {
                        if (-not $process.StandardOutput.EndOfStream) {
                            $line = $process.StandardOutput.ReadLine()
                            if ($line) {
                                Write-Host "Received: $line"
                                try {
                                    $response = $line | ConvertFrom-Json
                                    if ($response.id -eq 4) {
                                        $outputResponse = $response
                                        break
                                    }
                                } catch {
                                    # Not our response, keep reading
                                }
                            }
                        }
                        Start-Sleep -Milliseconds 100
                    }
                    
                    if (-not $outputResponse -or -not $outputResponse.result) {
                        throw "Failed to get script output"
                    }
                    
                    $outputResult = $outputResponse.result.content[0].text | ConvertFrom-Json
                    Write-Host "Script output received. IsComplete: $($outputResult.isComplete)"
                    Write-Host "Output: $($outputResult.output)"
                    
                    # Verify output contains expected data (UserId from WhoAmI)
                    if ($outputResult.output -notmatch '"UserId"') {
                        throw "Script output does not contain expected WhoAmI data"
                    }
                    
                    Write-Host "Step 7: Ending session..."
                    $endSessionRequest = @{
                        jsonrpc = "2.0"
                        id = 5
                        method = "tools/call"
                        params = @{
                            name = "EndSession"
                            arguments = @{
                                sessionId = $sessionId
                            }
                        }
                    } | ConvertTo-Json -Depth 10 -Compress
                    
                    $process.StandardInput.WriteLine($endSessionRequest)
                    $process.StandardInput.Flush()
                    
                    Write-Host "SUCCESS: MCP Server test completed successfully"
                    
                } finally {
                    # Clean up: stop the MCP server process
                    if (-not $process.HasExited) {
                        Write-Host "Stopping MCP Server process..."
                        $process.Kill()
                        $process.WaitForExit(5000)
                    }
                    $process.Dispose()
                    
                    # Clean up: remove test connection
                    try {
                        Write-Host "Cleaning up test connection..."
                        # Connection cleanup - the cmdlet doesn't have a delete method, so we'll just note it
                        Write-Host "Test connection will remain: $testConnectionName (cleanup would require manual removal)"
                    } catch {
                        Write-Host "Warning: Failed to cleanup connection: $_"
                    }
                }
                
            } catch {
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
}
