. $PSScriptRoot/Common.ps1

Describe 'Remove-DataverseAppModuleComponent' {
    BeforeEach {
        $connection = getMockConnection
    }

    Context "Parameter validation" {
        It "Should have proper parameter sets defined" {
            $cmdlet = Get-Command Remove-DataverseAppModuleComponent
            $cmdlet | Should -Not -BeNullOrEmpty
            $cmdlet.Verb | Should -Be "Remove"
            $cmdlet.Noun | Should -Be "DataverseAppModuleComponent"
        }

        It "Should support ById parameter set" {
            $cmdlet = Get-Command Remove-DataverseAppModuleComponent
            $cmdlet.Parameters.Keys | Should -Contain "Id"
            $cmdlet.Parameters.Keys | Should -Contain "IfExists"
        }

        It "Should support ByAppModuleUniqueName parameter set" {
            $cmdlet = Get-Command Remove-DataverseAppModuleComponent
            $cmdlet.Parameters.Keys | Should -Contain "AppModuleUniqueName"
            $cmdlet.Parameters.Keys | Should -Contain "ObjectId"
        }

        It "Should support ByAppModuleId parameter set" {
            $cmdlet = Get-Command Remove-DataverseAppModuleComponent
            $cmdlet.Parameters.Keys | Should -Contain "AppModuleId"
            $cmdlet.Parameters.Keys | Should -Contain "ObjectId"
        }
    }

    Context "WhatIf and Confirm support" {
        It "Should support WhatIf parameter" {
            $componentId = [Guid]::NewGuid()

            # Test WhatIf support - should not actually execute
            { Remove-DataverseAppModuleComponent -Connection $connection -Id $componentId -WhatIf } | Should -Not -Throw
        }

        It "Should support Confirm parameter" {
            $componentId = [Guid]::NewGuid()

            # Test Confirm support with confirmation disabled
            try {
                Remove-DataverseAppModuleComponent -Connection $connection -Id $componentId -Confirm:$false
            } catch {
                # Expected to fail due to mock limitations, but should not be a parameter binding issue
                $_.Exception.Message | Should -Not -Match "Cannot bind parameter.*Confirm"
            }
        }
    }

    Context "IfExists flag" {
        It "Should skip removal when IfExists is specified and component does not exist" {
            $componentId = [Guid]::NewGuid()

            # Test IfExists flag - should exit gracefully without throwing
            { Remove-DataverseAppModuleComponent -Connection $connection -Id $componentId -IfExists } | Should -Not -Throw
        }
    }
}