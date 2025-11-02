. $PSScriptRoot/Common.ps1

Describe "Set-DataverseConnectionAsDefault" {    Context "Setting default connection" {
        It "Sets the default connection" {
            $connection = getMockConnection
            Set-DataverseConnectionAsDefault -Connection $connection
            $default = Get-DataverseConnection -GetDefault
            $default | Should -Be $connection
        }
    }
}
