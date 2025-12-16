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

    It "Can perform complete plugin package CRUD operations" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath

            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $ErrorActionPreference = "Stop"
                $ConfirmPreference = 'None'

                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}

                # Generate unique test identifier
                $testPrefix = "e2etest_" + [Guid]::NewGuid().ToString("N").Substring(0, 8)
                $uniqueName = "${testPrefix}_TestPkg"
                Write-Host "Test package unique name: $uniqueName"

                # Create a minimal valid NuGet package
                $packageDir = Join-Path ([System.IO.Path]::GetTempPath()) "TestPluginPackage_$testPrefix"
                New-Item -ItemType Directory -Path $packageDir -Force | Out-Null
                New-Item -ItemType Directory -Path "$packageDir/lib/net462" -Force | Out-Null

                # Create .nuspec file
                $nuspecContent = @"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
  <metadata>
    <id>$uniqueName</id>
    <version>1.0.0</version>
    <authors>E2E Test</authors>
    <description>Test plugin package for E2E testing</description>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
  </metadata>
  <files>
    <file src="lib/**" target="lib" />
  </files>
</package>
"@
                $nuspecContent | Out-File -FilePath "$packageDir/$uniqueName.nuspec" -Encoding UTF8

                # Create a minimal dummy DLL
                "// Dummy plugin assembly for testing" | Out-File -FilePath "$packageDir/lib/net462/TestPlugin.dll" -Encoding ASCII

                # Create the .nupkg file (ZIP)
                Add-Type -Assembly System.IO.Compression.FileSystem
                $packagePath = Join-Path ([System.IO.Path]::GetTempPath()) "$uniqueName.1.0.0.nupkg"
                if (Test-Path $packagePath) { Remove-Item $packagePath -Force }
                [System.IO.Compression.ZipFile]::CreateFromDirectory($packageDir, $packagePath)
                Write-Host "Created test package at: $packagePath"

                $packageId = $null

                try {
                    # ==========================================
                    # Step 1: Create plugin package
                    # ==========================================
                    Write-Host "Step 1: Creating plugin package..."

                    $package = Set-DataversePluginPackage `
                        -Connection $connection `
                        -UniqueName $uniqueName `
                        -FilePath $packagePath `
                        -Version "1.0.0" `
                        -Description "E2E Test Package" `
                        -PassThru

                    $packageId = $package.Id
                    Write-Host "✓ Created package with ID: $packageId"

                    if ([string]::IsNullOrEmpty($package.uniquename)) {
                        throw "Created package missing 'uniquename' property"
                    }

                    if ($package.uniquename -ne $uniqueName) {
                        throw "Expected unique name '$uniqueName', got '$($package.uniquename)'"
                    }

                    # ==========================================
                    # Step 2: Query all packages
                    # ==========================================
                    Write-Host "Step 2: Querying all plugin packages..."

                    $allPackages = Get-DataversePluginPackage -Connection $connection
                    Write-Host "✓ Found $($allPackages.Count) total packages"

                    $foundPackage = $allPackages | Where-Object { $_.Id -eq $packageId }
                    if ($null -eq $foundPackage) {
                        throw "Created package not found in list of all packages"
                    }

                    # ==========================================
                    # Step 3: Query by unique name
                    # ==========================================
                    Write-Host "Step 3: Querying package by unique name..."

                    $packageByName = Get-DataversePluginPackage -Connection $connection -UniqueName $uniqueName

                    if ($null -eq $packageByName) {
                        throw "Expected to get package by unique name '$uniqueName', but got null"
                    }

                    if ($packageByName.Id -ne $packageId) {
                        throw "Expected package ID '$packageId', got '$($packageByName.Id)'"
                    }

                    Write-Host "✓ Successfully retrieved package by unique name"

                    # ==========================================
                    # Step 4: Query by ID
                    # ==========================================
                    Write-Host "Step 4: Querying package by ID..."

                    $packageById = Get-DataversePluginPackage -Connection $connection -Id $packageId

                    if ($null -eq $packageById) {
                        throw "Expected to get package by ID '$packageId', but got null"
                    }

                    if ($packageById.uniquename -ne $uniqueName) {
                        throw "Expected unique name '$uniqueName', got '$($packageById.uniquename)'"
                    }

                    Write-Host "✓ Successfully retrieved package by ID"

                    # ==========================================
                    # Step 5: Update package (new version)
                    # ==========================================
                    Write-Host "Step 5: Updating plugin package..."

                    # Create updated package with new version
                    $nuspecContentV2 = $nuspecContent -replace "1.0.0", "2.0.0"
                    $nuspecContentV2 | Out-File -FilePath "$packageDir/$uniqueName.nuspec" -Encoding UTF8 -Force

                    $packagePathV2 = Join-Path ([System.IO.Path]::GetTempPath()) "$uniqueName.2.0.0.nupkg"
                    if (Test-Path $packagePathV2) { Remove-Item $packagePathV2 -Force }
                    [System.IO.Compression.ZipFile]::CreateFromDirectory($packageDir, $packagePathV2)

                    $updatedPackage = Set-DataversePluginPackage `
                        -Connection $connection `
                        -Id $packageId `
                        -UniqueName $uniqueName `
                        -FilePath $packagePathV2 `
                        -Version "2.0.0" `
                        -Description "E2E Test Package Updated" `
                        -PassThru

                    if ($updatedPackage.version -ne "2.0.0") {
                        throw "Expected version '2.0.0', got '$($updatedPackage.version)'"
                    }

                    Write-Host "✓ Successfully updated package to version 2.0.0"

                    # ==========================================
                    # Step 6: Delete package
                    # ==========================================
                    Write-Host "Step 6: Deleting plugin package..."

                    Remove-DataversePluginPackage -Connection $connection -Id $packageId -Confirm:$false

                    Write-Host "✓ Successfully deleted package"

                    # ==========================================
                    # Step 7: Verify deletion
                    # ==========================================
                    Write-Host "Step 7: Verifying deletion..."

                    $deletedPackage = $null
                    try {
                        $deletedPackage = Get-DataversePluginPackage -Connection $connection -Id $packageId -ErrorAction SilentlyContinue
                    } catch {
                        # Expected - package should not exist
                    }

                    if ($null -ne $deletedPackage) {
                        throw "Package still exists after deletion"
                    }

                    Write-Host "✓ Verified package was deleted"

                    # ==========================================
                    # Step 8: Test IfExists flag
                    # ==========================================
                    Write-Host "Step 8: Testing IfExists flag..."

                    # Should not throw error
                    Remove-DataversePluginPackage -Connection $connection -Id $packageId -IfExists -Confirm:$false

                    Write-Host "✓ IfExists flag works correctly"

                    Write-Host ""
                    Write-Host "All plugin package CRUD operations completed successfully!"

                } finally {
                    # Cleanup: ensure package is deleted even if test fails
                    if ($null -ne $packageId) {
                        try {
                            Remove-DataversePluginPackage -Connection $connection -Id $packageId -IfExists -Confirm:$false -ErrorAction SilentlyContinue
                            Write-Host "Cleanup: Removed test package"
                        } catch {
                            Write-Host "Cleanup: Package already removed or error during cleanup"
                        }
                    }

                    # Cleanup temp files
                    if (Test-Path $packageDir) { Remove-Item -Recurse -Force $packageDir -ErrorAction SilentlyContinue }
                    if (Test-Path $packagePath) { Remove-Item $packagePath -ErrorAction SilentlyContinue }
                    if (Test-Path $packagePathV2) { Remove-Item $packagePathV2 -ErrorAction SilentlyContinue }
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
