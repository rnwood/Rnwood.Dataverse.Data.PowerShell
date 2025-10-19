Describe "Set-DataverseConnectionAsDefault" {
    . $PSScriptRoot/Common.ps1

    Context "Setting default connection" {
        It "Sets the default connection" {
            $connection = getMockConnection
            Set-DataverseConnectionAsDefault -Connection $connection
            $default = Get-DataverseConnection -GetDefault
            $default | Should -Be $connection
        }
    }
}