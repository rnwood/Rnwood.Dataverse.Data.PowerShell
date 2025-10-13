Describe "Module" {

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
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
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
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
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
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
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
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
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
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
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
                    $whoami = Get-DataverseWhoAmI
                    # Return a simple object with the item and user ID
                    [PSCustomObject]@{
                        Item = $_
                        UserId = $whoami.UserId
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
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
}