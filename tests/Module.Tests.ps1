. $PSScriptRoot/Common.ps1

Describe "Module" {    It "Given the module is installed, it is listed as available" -Skip {
        # SKIPPED: This test is flaky due to PowerShell's module caching behavior.
        # Get-Module -ListAvailable may not immediately reflect newly added modules to PSModulePath.
        # The module loading is tested in other tests.
        
        # Check that module is found in temp directory (our test module path)
        $modules = @(Get-Module -ListAvailable Rnwood.Dataverse.Data.PowerShell | Where-Object { $_.ModuleBase -like "*Temp*" })
        $modules | Should -HaveCount 1
        $modules[0].Path | Should -Exist
    }

    It "Given the module was not already loaded, it can be loaded successfully" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            if ([appdomain]::CurrentDomain.GetAssemblies() | Where-Object{ $_.GetName().Name -eq "Microsoft.Xrm.Sdk"}) {
                throw "Expected assembly to not be already loaded"
            }

            if (Get-Module Rnwood.Dataverse.Data.PowerShell) {
                throw "Expected module to not be loaded"
            }

            Import-Module Rnwood.Dataverse.Data.PowerShell

            if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
                throw "Expected module to be loaded"
            }

            if (-not ([appdomain]::CurrentDomain.GetAssemblies() | Where-Object{ $_.GetName().Name -eq "Microsoft.Xrm.Sdk"})) {
                throw "Expected assembly to be loaded"
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

    It "Given SDK assemblies are already loaded, it can be loaded successfully" {

        newPwsh {
            $env:PSModulePath = $env:ChildProcessPSModulePath
            if ([appdomain]::CurrentDomain.GetAssemblies() | Where-Object{ $_.GetName().Name -eq "Microsoft.Xrm.Sdk"}) {
                throw "Expected assembly to not be already loaded"
            }

            if (Get-Module Rnwood.Dataverse.Data.PowerShell) {
                throw "Expected module to not be loaded"
            }

            $modulefolder = (Get-Module -ListAvailable Rnwood.Dataverse.Data.PowerShell).ModuleBase

            if ($PSVersionTable.PSEdition -eq "Core") {
                add-type -AssemblyName $modulefolder/cmdlets/net8.0/Microsoft.Xrm.Sdk.dll
            } else {
                add-type -AssemblyName $modulefolder/cmdlets/net462/Microsoft.Xrm.Sdk.dll
            }

            Import-Module Rnwood.Dataverse.Data.PowerShell

            if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
                throw "Expected module to be loaded"
            }

            if (-not ([appdomain]::CurrentDomain.GetAssemblies() | Where-Object{ $_.GetName().Name -eq "Microsoft.Xrm.Sdk"})) {
                throw "Expected assembly to be loaded"
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }

}
