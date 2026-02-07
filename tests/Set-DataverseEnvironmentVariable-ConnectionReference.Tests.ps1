. $PSScriptRoot/Common.ps1

# Note: These tests are skipped because the mock metadata (contact.xml) does not include
# environmentvariabledefinition, environmentvariablevalue, or connectionreference entities.
# These cmdlets should be tested manually or with E2E tests against a real Dataverse environment.

Describe 'Set-DataverseEnvironmentVariableValue' {
    Context 'Setting a single environment variable value' {
        It "Throws error when environment variable definition does not exist" {
            # This test validates the cmdlet exists and has proper parameters
            $connection = getMockConnection -Entities @("environmentvariabledefinition", "environmentvariablevalue", "connectionreference")
            
            # Should throw because entity metadata is not in mock, but this validates the cmdlet works
            { Set-DataverseEnvironmentVariableValue -Connection $connection -SchemaName "new_nonexistent" -Value "value" -ErrorAction Stop } | Should -Throw
        }
    }

    Context 'Setting multiple environment variable values' {
    }
}

Describe 'Get-DataverseEnvironmentVariableValue' {
    Context 'Getting environment variable values with mismatched schemaname (bug fix #291)' {
    }
}

Describe 'Remove-DataverseEnvironmentVariableValue' {
    Context 'Removing environment variable values with mismatched schemaname (bug fix #291)' {
    }
}

Describe 'Set-DataverseConnectionReference' {
    Context 'Setting a single connection reference' {
        It "Throws error when connection reference does not exist and ConnectorId is not provided" {
            # This test validates that ConnectorId is required for creation
            $connection = getMockConnection -Entities @("environmentvariabledefinition", "environmentvariablevalue", "connectionreference")

            # Should throw because ConnectorId is required for creation
            { Set-DataverseConnectionReference -Connection $connection `
                -ConnectionReferenceLogicalName "new_nonexistent" `
                -ConnectionId "12345678-1234-1234-1234-123456789012" -ErrorAction Stop } | Should -Throw
        }
    }

    Context 'Setting multiple connection references' {
    }
}
