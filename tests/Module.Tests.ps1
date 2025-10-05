Describe "Module" {
    . $PSScriptRoot/Common.ps1

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

    It "Get-DataverseConnection has SelectEnvironment parameter set" {
        . $PSScriptRoot/Common.ps1
        
        # Import module if not already loaded
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        $cmd = Get-Command Get-DataverseConnection
        $cmd | Should -Not -BeNull
        
        # Check that the parameter set exists
        $parameterSets = $cmd.ParameterSets | Where-Object { $_.Name -eq "Authenticate interactively with environment selection" }
        $parameterSets | Should -Not -BeNull
        $parameterSets | Should -HaveCount 1
        
        # Check that SelectEnvironment parameter exists
        $selectEnvParam = $cmd.Parameters["SelectEnvironment"]
        $selectEnvParam | Should -Not -BeNull
        $selectEnvParam.ParameterType.Name | Should -Be "SwitchParameter"
        
        # Verify URL is not required for this parameter set
        $urlParam = $cmd.Parameters["Url"]
        $urlParam | Should -Not -BeNull
        $urlParamInSet = $parameterSets[0].Parameters | Where-Object { $_.Name -eq "Url" }
        $urlParamInSet | Should -BeNull
    }

}