. $PSScriptRoot/Common.ps1

Describe "Module" {

    It "Given the module is installed, it is listed as available" {
        Get-Module -ListAvailable Rnwood.Dataverse.Data.PowerShell | Should -HaveCount 1
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
                add-type -AssemblyName $modulefolder/cmdlets/net6.0/Microsoft.Xrm.Sdk.dll
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
# Moved to tests/sdk/Module.Tests.ps1
# Module tests relocated to tests/sdk to mirror cmdlet layout.