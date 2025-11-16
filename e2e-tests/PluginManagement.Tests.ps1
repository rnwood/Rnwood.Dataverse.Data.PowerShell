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
}
