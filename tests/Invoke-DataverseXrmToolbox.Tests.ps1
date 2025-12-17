. "$PSScriptRoot/Common.ps1"

Describe 'Invoke-DataverseXrmToolbox' {
    Context 'Cmdlet availability' {
        It 'Cmdlet should be available' {
            Get-Command Invoke-DataverseXrmToolbox | Should -Not -BeNull
        }
        
        It 'Should have PackageName parameter' {
            (Get-Command Invoke-DataverseXrmToolbox).Parameters.ContainsKey('PackageName') | Should -Be $true
        }
        
        It 'Should have Version parameter' {
            (Get-Command Invoke-DataverseXrmToolbox).Parameters.ContainsKey('Version') | Should -Be $true
        }
        
        It 'Should have Force parameter' {
            (Get-Command Invoke-DataverseXrmToolbox).Parameters.ContainsKey('Force') | Should -Be $true
        }
        
        It 'Should have CacheDirectory parameter' {
            (Get-Command Invoke-DataverseXrmToolbox).Parameters.ContainsKey('CacheDirectory') | Should -Be $true
        }
    }
    
    Context 'Parameter validation' {
        It 'PackageName should be mandatory' {
            (Get-Command Invoke-DataverseXrmToolbox).Parameters['PackageName'].Attributes | 
                Where-Object { $_ -is [System.Management.Automation.ParameterAttribute] } |
                Select-Object -ExpandProperty Mandatory | Should -Contain $true
        }
    }
    
    Context 'Basic functionality' {
        BeforeAll {
            # Create a mock connection for testing
            $global:TestConnection = getMockConnection
        }
        
        It 'Should fail without connection' {
            # This test verifies the cmdlet requires a connection
            # We expect it to fail during processing when it tries to use the connection
            { Invoke-DataverseXrmToolbox -PackageName "TestPackage" -ErrorAction Stop } | Should -Throw
        }
        
        It 'Should accept Connection parameter' {
            # This test just verifies the parameter accepts a connection object
            # We're not actually running the cmdlet since it would try to download packages
            $cmd = Get-Command Invoke-DataverseXrmToolbox
            $cmd.Parameters['Connection'].ParameterType.Name | Should -Be 'ServiceClient'
        }
    }
    
    Context 'Help documentation' {
        It 'Should have help content' {
            $help = Get-Help Invoke-DataverseXrmToolbox
            $help.Synopsis | Should -Not -BeNullOrEmpty
        }
        
        It 'Should have examples in markdown documentation' {
            # Check that markdown documentation exists with examples
            $docFile = "$PSScriptRoot/../docs/Invoke-DataverseXrmToolbox.md"
            Test-Path $docFile | Should -Be $true
            (Get-Content $docFile -Raw) -match '## Examples' | Should -Be $true
        }
        
        It 'Should document all parameters' {
            $help = Get-Help Invoke-DataverseXrmToolbox -Parameter *
            $help | Should -Not -BeNullOrEmpty
        }
    }
}
