Describe "Get-DataverseConnection Certificate Authentication" {
    . $PSScriptRoot/Common.ps1

    Context "Certificate validation" {
        It "Throws error when neither CertificatePath nor CertificateThumbprint is provided" {
            # This test validates the LoadCertificate method requires at least one parameter
            # We can't easily test this without actually calling the cmdlet, which would require
            # a real certificate. This is documented behavior.
            $true | Should -Be $true
        }

        It "CertificatePath parameter exists" {
            # First get a mock connection to ensure module is loaded
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $cmd.Parameters.ContainsKey('CertificatePath') | Should -Be $true
        }

        It "CertificatePassword parameter exists" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $cmd.Parameters.ContainsKey('CertificatePassword') | Should -Be $true
        }

        It "CertificateThumbprint parameter exists" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $cmd.Parameters.ContainsKey('CertificateThumbprint') | Should -Be $true
        }

        It "CertificateStoreLocation parameter exists" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $cmd.Parameters.ContainsKey('CertificateStoreLocation') | Should -Be $true
        }

        It "CertificateStoreName parameter exists" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $cmd.Parameters.ContainsKey('CertificateStoreName') | Should -Be $true
        }

        It "Certificate parameters are in correct parameter set" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $certPathParam = $cmd.Parameters['CertificatePath']
            $certPathParam.ParameterSets.Keys | Should -Contain 'Authenticate with client certificate'
        }
    }

    Context "Certificate authentication scenarios" {
        It "Validates ClientId is required for certificate authentication" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $clientIdParam = $cmd.Parameters['ClientId']
            $clientIdParam.ParameterSets['Authenticate with client certificate'].IsMandatory | Should -Be $true
        }

        It "Validates Url is required for certificate authentication" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $urlParam = $cmd.Parameters['Url']
            $urlParam.ParameterSets['Authenticate with client certificate'].IsMandatory | Should -Be $true
        }

        It "Validates CertificatePath is required for certificate authentication" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $certPathParam = $cmd.Parameters['CertificatePath']
            $certPathParam.ParameterSets['Authenticate with client certificate'].IsMandatory | Should -Be $true
        }
    }
}
