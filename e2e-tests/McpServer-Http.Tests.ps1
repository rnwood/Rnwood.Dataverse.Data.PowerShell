$ErrorActionPreference = "Stop"

Describe "MCP Server HTTP Mode" {

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

    It "Can start HTTP server and handle JSON-RPC requests" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            
            Import-Module Rnwood.Dataverse.Data.PowerShell
            
            try {
                Write-Host "Step 1: Creating and saving a test connection..."
                # Generate unique connection name for this test
                $testConnectionName = "E2ETest-HTTP-$([guid]::NewGuid().ToString('N').Substring(0, 8))"
                Write-Host "Using connection name: $testConnectionName"
                
                # Save a connection
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET} -Name $testConnectionName -SetAsDefault
                
                if (-not $connection) {
                    throw "Failed to create connection"
                }
                Write-Host "Connection saved successfully"
                
                Write-Host "Step 2: Starting MCP server in HTTP mode..."
                # Find the MCP server executable
                $mcpServerPath = "$env:ChildProcessPSModulePath/Rnwood.Dataverse.Data.PowerShell/../../Rnwood.Dataverse.Data.PowerShell.McpServer/bin/Debug/net8.0/Rnwood.Dataverse.Data.PowerShell.McpServer.dll"
                $mcpServerPath = [System.IO.Path]::GetFullPath($mcpServerPath)
                
                if (-not (Test-Path $mcpServerPath)) {
                    throw "MCP Server not found at: $mcpServerPath"
                }
                Write-Host "MCP Server found at: $mcpServerPath"
                
                # Start the MCP server process in HTTP mode on a random available port
                $port = Get-Random -Minimum 5000 -Maximum 6000
                $url = "http://localhost:$port"
                
                $psi = New-Object System.Diagnostics.ProcessStartInfo
                $psi.FileName = "dotnet"
                $psi.Arguments = "$mcpServerPath --connection $testConnectionName --http --urls $url"
                $psi.UseShellExecute = $false
                $psi.RedirectStandardOutput = $true
                $psi.RedirectStandardError = $true
                $psi.CreateNoWindow = $true
                
                $process = New-Object System.Diagnostics.Process
                $process.StartInfo = $psi
                
                try {
                    $process.Start() | Out-Null
                    Write-Host "MCP Server started with PID: $($process.Id) on $url"
                    
                    # Give server time to initialize
                    Start-Sleep -Seconds 5
                    
                    if ($process.HasExited) {
                        $stderr = $process.StandardError.ReadToEnd()
                        throw "MCP Server exited unexpectedly. StdErr: $stderr"
                    }
                    
                    Write-Host "Step 3: Testing HTTP JSON-RPC communication..."
                    
                    # Test initialize endpoint
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
                    } | ConvertTo-Json -Depth 10
                    
                    Write-Host "Sending initialize request to $url/mcp..."
                    $initResponse = Invoke-RestMethod -Uri "$url/mcp" -Method Post -Body $initRequest -ContentType "application/json" -ErrorAction Stop
                    
                    Write-Host "Received initialize response: $($initResponse | ConvertTo-Json -Compress)"
                    
                    if ($initResponse.result.serverInfo.name -ne "dataverse-powershell-mcp") {
                        throw "Invalid server info in response"
                    }
                    
                    Write-Host "Step 4: Creating a session via HTTP..."
                    $createSessionRequest = @{
                        jsonrpc = "2.0"
                        id = 2
                        method = "tools/call"
                        params = @{
                            name = "CreateSession"
                            arguments = @{}
                        }
                    } | ConvertTo-Json -Depth 10
                    
                    $sessionResponse = Invoke-RestMethod -Uri "$url/mcp" -Method Post -Body $createSessionRequest -ContentType "application/json" -ErrorAction Stop
                    
                    Write-Host "Session response: $($sessionResponse | ConvertTo-Json -Compress)"
                    
                    if (-not $sessionResponse.result) {
                        throw "Failed to create session via HTTP"
                    }
                    
                    $sessionId = ($sessionResponse.result | ConvertFrom-Json).sessionId
                    Write-Host "Session created with ID: $sessionId"
                    
                    Write-Host "Step 5: Running a script via HTTP..."
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
                    } | ConvertTo-Json -Depth 10
                    
                    $scriptResponse = Invoke-RestMethod -Uri "$url/mcp" -Method Post -Body $runScriptRequest -ContentType "application/json" -ErrorAction Stop
                    
                    Write-Host "Script response: $($scriptResponse | ConvertTo-Json -Compress)"
                    
                    if (-not $scriptResponse.result) {
                        throw "Failed to run script via HTTP"
                    }
                    
                    $scriptExecutionId = ($scriptResponse.result | ConvertFrom-Json).scriptExecutionId
                    Write-Host "Script execution started with ID: $scriptExecutionId"
                    
                    Write-Host "Step 6: Getting script output via HTTP..."
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
                    } | ConvertTo-Json -Depth 10
                    
                    $outputResponse = Invoke-RestMethod -Uri "$url/mcp" -Method Post -Body $getOutputRequest -ContentType "application/json" -ErrorAction Stop
                    
                    Write-Host "Output response: $($outputResponse | ConvertTo-Json -Compress)"
                    
                    if (-not $outputResponse.result) {
                        throw "Failed to get script output via HTTP"
                    }
                    
                    $outputResult = $outputResponse.result | ConvertFrom-Json
                    Write-Host "Script output received. IsComplete: $($outputResult.IsComplete)"
                    Write-Host "Output: $($outputResult.Output)"
                    
                    # Verify output contains expected data (UserId from WhoAmI)
                    if ($outputResult.Output -notmatch '"UserId"') {
                        throw "Script output does not contain expected WhoAmI data"
                    }
                    
                    Write-Host "Step 7: Ending session via HTTP..."
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
                    } | ConvertTo-Json -Depth 10
                    
                    $endResponse = Invoke-RestMethod -Uri "$url/mcp" -Method Post -Body $endSessionRequest -ContentType "application/json" -ErrorAction Stop
                    Write-Host "End session response: $($endResponse | ConvertTo-Json -Compress)"
                    
                    Write-Host "SUCCESS: MCP Server HTTP mode test completed successfully"
                    
                } finally {
                    # Clean up: stop the MCP server process
                    if (-not $process.HasExited) {
                        Write-Host "Stopping MCP Server process..."
                        $process.Kill()
                        $process.WaitForExit(5000)
                    }
                    $process.Dispose()
                    
                    Write-Host "Test connection will remain: $testConnectionName (cleanup would require manual removal)"
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
