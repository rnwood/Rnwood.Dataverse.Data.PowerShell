. $PSScriptRoot/Common.ps1

Describe 'Invoke-DataverseSql - TDS Endpoint Support' {
    Context 'AccessTokenProvider Configuration' {
        It "Module loads successfully with Sql4Cds submodule" {
            # This test verifies that the module loads correctly with the Sql4Cds submodule
            $connection = getMockConnection
            
            # Verify the cmdlet exists
            $cmdlet = Get-Command Invoke-DataverseSql -ErrorAction SilentlyContinue
            $cmdlet | Should -Not -BeNullOrEmpty
            $cmdlet.Name | Should -Be 'Invoke-DataverseSql'
        }
        
        It "UseTdsEndpoint parameter exists and accepts switch values" {
            # Verify the parameter exists and has correct type
            $cmdlet = Get-Command Invoke-DataverseSql
            $param = $cmdlet.Parameters['UseTdsEndpoint']
            
            $param | Should -Not -BeNullOrEmpty
            $param.ParameterType.Name | Should -Be 'SwitchParameter'
        }
    }
    
    Context 'Date Manipulation Query Compatibility' {
    }
}
