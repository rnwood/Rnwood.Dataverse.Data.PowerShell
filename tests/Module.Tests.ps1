. $PSScriptRoot/Common.ps1

Describe "Module" {
    It "Given the module was not already loaded, it can be loaded successfully" {
        Get-Module Rnwood.Dataverse.Data.PowerShell | Should -HaveCount 0

        Import-Module Rnwood.Dataverse.Data.PowerShell

        Get-Module Rnwood.Dataverse.Data.PowerShell | Should -HaveCount 1
    }

    It "Given SDK assemblies are already loaded, it can be loaded successfully" {

        Get-Module Rnwood.Dataverse.Data.PowerShell | Should -HaveCount 0

        $modulefolder = (Get-Module -ListAvailable Rnwood.Dataverse.Data.PowerShell).ModuleBase

        if ($PSVersionTable.PSEdition -eq "Core") {
            add-type -AssemblyName $modulefolder/net6.0/Microsoft.Xrm.Sdk.dll
        } else {
            add-type -AssemblyName $modulefolder/net462/Microsoft.Xrm.Sdk.dll
        }

        Import-Module Rnwood.Dataverse.Data.PowerShell

        Get-Module Rnwood.Dataverse.Data.PowerShell | Should -HaveCount 1
    }
}