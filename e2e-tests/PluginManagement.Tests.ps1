Describe "Plugin Management Cmdlets" {

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

    It "Can query plugin assemblies from a real environment" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Get all plugin assemblies (should work without specifying -All switch with new default parameter set)
                $assemblies = Get-DataversePluginAssembly -Connection $connection
                
                Write-Host "Found $($assemblies.Count) plugin assemblies"
                
                # Verify we got results
                if ($null -eq $assemblies) {
                    throw "Expected to get plugin assemblies, but got null"
                }
                
                # If there are any assemblies, verify they have expected properties
                if ($assemblies.Count -gt 0) {
                    $firstAssembly = $assemblies[0]
                    if (-not $firstAssembly.name) {
                        throw "Assembly missing 'name' property"
                    }
                    if (-not $firstAssembly.Id) {
                        throw "Assembly missing 'Id' property"
                    }
                    Write-Host "Successfully retrieved plugin assembly with name: $($firstAssembly.name)"
                }
                
                Write-Host "Plugin assembly query test passed"
            } catch {
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Can query plugin types from a real environment" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Get all plugin types
                $types = Get-DataversePluginType -Connection $connection
                
                Write-Host "Found $($types.Count) plugin types"
                
                # Verify we got results
                if ($null -eq $types) {
                    throw "Expected to get plugin types, but got null"
                }
                
                # If there are any types, verify they have expected properties
                if ($types.Count -gt 0) {
                    $firstType = $types[0]
                    if (-not $firstType.typename) {
                        throw "Plugin type missing 'typename' property"
                    }
                    if (-not $firstType.Id) {
                        throw "Plugin type missing 'Id' property"
                    }
                    Write-Host "Successfully retrieved plugin type: $($firstType.typename)"
                }
                
                Write-Host "Plugin type query test passed"
            } catch {
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Can query plugin steps from a real environment" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Get all plugin steps
                $steps = Get-DataversePluginStep -Connection $connection
                
                Write-Host "Found $($steps.Count) plugin steps"
                
                # Verify we got results
                if ($null -eq $steps) {
                    throw "Expected to get plugin steps, but got null"
                }
                
                # If there are any steps, verify they have expected properties
                if ($steps.Count -gt 0) {
                    $firstStep = $steps[0]
                    if (-not $firstStep.name) {
                        throw "Plugin step missing 'name' property"
                    }
                    if (-not $firstStep.Id) {
                        throw "Plugin step missing 'Id' property"
                    }
                    Write-Host "Successfully retrieved plugin step: $($firstStep.name)"
                }
                
                Write-Host "Plugin step query test passed"
            } catch {
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Enum parameters work correctly with tab completion support" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                # Verify that the enum types exist and can be used
                $isolationModeType = [Rnwood.Dataverse.Data.PowerShell.Commands.PluginAssemblyIsolationMode]
                $sourceTypeType = [Rnwood.Dataverse.Data.PowerShell.Commands.PluginAssemblySourceType]
                $stageType = [Rnwood.Dataverse.Data.PowerShell.Commands.PluginStepStage]
                $modeType = [Rnwood.Dataverse.Data.PowerShell.Commands.PluginStepMode]
                $imageTypeType = [Rnwood.Dataverse.Data.PowerShell.Commands.PluginStepImageType]
                $deploymentType = [Rnwood.Dataverse.Data.PowerShell.Commands.PluginStepDeployment]
                
                # Verify enum values
                if ([int]$isolationModeType::Sandbox -ne 1) {
                    throw "Expected Sandbox isolation mode to be 1, got $([int]$isolationModeType::Sandbox)"
                }
                
                if ([int]$stageType::PreOperation -ne 20) {
                    throw "Expected PreOperation stage to be 20, got $([int]$stageType::PreOperation)"
                }
                
                if ([int]$modeType::Synchronous -ne 0) {
                    throw "Expected Synchronous mode to be 0, got $([int]$modeType::Synchronous)"
                }
                
                if ([int]$imageTypeType::PreImage -ne 0) {
                    throw "Expected PreImage type to be 0, got $([int]$imageTypeType::PreImage)"
                }
                
                Write-Host "All enum types and values are correct"
            } catch {
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Can query plugin packages from a real environment" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath

            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}

                # Get all plugin packages
                $packages = Get-DataversePluginPackage -Connection $connection
                
                Write-Host "Found $($packages.Count) plugin packages"
                
                # Verify we got results (could be null or empty if no packages exist)
                if ($null -eq $packages) {
                    Write-Host "No plugin packages found - this is acceptable"
                }
                else {
                    # If there are any packages, verify they have expected properties
                    if ($packages.Count -gt 0) {
                        $firstPackage = $packages[0]
                        if (-not $firstPackage.uniquename) {
                            throw "Package missing 'uniquename' property"
                        }
                        if (-not $firstPackage.Id) {
                            throw "Package missing 'Id' property"
                        }
                        Write-Host "Successfully retrieved plugin package with unique name: $($firstPackage.uniquename)"
                    }
                }
                
                Write-Host "Plugin package query test passed"
            } catch {
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Can query a specific plugin package by unique name" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath

            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}

                # First, get all packages to see if any exist
                $allPackages = Get-DataversePluginPackage -Connection $connection
                
                if ($null -ne $allPackages -and $allPackages.Count -gt 0) {
                    $testPackageName = $allPackages[0].uniquename
                    Write-Host "Testing query by unique name: $testPackageName"
                    
                    # Query by unique name
                    $package = Get-DataversePluginPackage -Connection $connection -UniqueName $testPackageName
                    
                    if ($null -eq $package) {
                        throw "Expected to get package by unique name '$testPackageName', but got null"
                    }
                    
                    if ($package.uniquename -ne $testPackageName) {
                        throw "Expected package unique name to be '$testPackageName', but got '$($package.uniquename)'"
                    }
                    
                    Write-Host "Successfully retrieved package by unique name: $($package.uniquename)"
                }
                else {
                    Write-Host "No plugin packages found to test query by unique name - skipping this validation"
                }
                
                Write-Host "Plugin package by name query test passed"
            } catch {
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Can query a specific plugin package by ID" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath

            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}

                # First, get all packages to see if any exist
                $allPackages = Get-DataversePluginPackage -Connection $connection
                
                if ($null -ne $allPackages -and $allPackages.Count -gt 0) {
                    $testPackageId = $allPackages[0].Id
                    Write-Host "Testing query by ID: $testPackageId"
                    
                    # Query by ID
                    $package = Get-DataversePluginPackage -Connection $connection -Id $testPackageId
                    
                    if ($null -eq $package) {
                        throw "Expected to get package by ID '$testPackageId', but got null"
                    }
                    
                    if ($package.Id -ne $testPackageId) {
                        throw "Expected package ID to be '$testPackageId', but got '$($package.Id)'"
                    }
                    
                    Write-Host "Successfully retrieved package by ID: $($package.Id)"
                }
                else {
                    Write-Host "No plugin packages found to test query by ID - skipping this validation"
                }
                
                Write-Host "Plugin package by ID query test passed"
            } catch {
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
}
