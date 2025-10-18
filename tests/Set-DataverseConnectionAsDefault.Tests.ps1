Describe "Set-DataverseConnectionAsDefault" {
    BeforeAll {
        $ModulePath = (Resolve-Path "Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0").Path
        $env:TESTMODULEPATH = $ModulePath
        Import-Module $ModulePath -Force
    }

    Context "Setting default connection" {
        It "Sets the default connection" {
            $mockConnection = New-MockObject -Type 'Microsoft.PowerPlatform.Dataverse.Client.ServiceClient'
            Set-DataverseConnectionAsDefault -Connection $mockConnection
            $default = Get-DataverseConnection -GetDefault
            $default | Should -Be $mockConnection
        }
    }
}