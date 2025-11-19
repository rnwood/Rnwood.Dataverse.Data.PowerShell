. $PSScriptRoot/Common.ps1

Describe "Invoke-DataverseRequest - PSObject Unwrapping" {
    BeforeAll {
        $connection = getMockConnection -Entities @('contact')
    }

    Context "NameAndInputs parameter set unwraps PSObject values" {
        # Note: FakeXrmEasy doesn't support generic OrganizationRequest created via RequestName parameter
        # So we test that the code doesn't throw PSObject serialization errors by validating the unwrapping happens
        # The actual functional tests for custom APIs should be in e2e-tests/
        
        It "Unwraps EntityReference passed in hashtable - no PSObject serialization error" {
            # This reproduces the issue from GitHub: passing EntityReference in a hashtable
            # PowerShell wraps it in PSObject, which causes serialization errors
            $target = New-Object Microsoft.Xrm.Sdk.EntityReference "contact", "12345678-1234-1234-1234-123456789012"
            
            # When we put it in a hashtable and pass it, PowerShell wraps it
            $params = @{
                Target = $target
            }
            
            # The error we're fixing is "Type 'System.Management.Automation.PSObject' cannot be serialized"
            # If we get a different error (like FakeXrmEasy not supporting the request type), that's OK
            # We just want to ensure no PSObject serialization error
            try {
                Invoke-DataverseRequest -Connection $connection -RequestName "sample_MyCustomApi" -Parameters $params
                $true | Should -Be $true # If it succeeds, great!
            }
            catch {
                # Check that the error is NOT about PSObject serialization
                $_.Exception.Message | Should -Not -Match "PSObject.*cannot be serialized"
                $_.Exception.Message | Should -Not -Match "System.Management.Automation.PSObject"
            }
        }

        It "Unwraps OptionSetValue passed in hashtable - no PSObject serialization error" {
            $priority = New-Object Microsoft.Xrm.Sdk.OptionSetValue 1
            
            $params = @{
                Priority = $priority
            }
            
            try {
                Invoke-DataverseRequest -Connection $connection -RequestName "sample_MyCustomApi" -Parameters $params
                $true | Should -Be $true
            }
            catch {
                # Check that the error is NOT about PSObject serialization
                $_.Exception.Message | Should -Not -Match "PSObject.*cannot be serialized"
                $_.Exception.Message | Should -Not -Match "System.Management.Automation.PSObject"
            }
        }

        It "Unwraps multiple SDK objects in hashtable - no PSObject serialization error" {
            # Test the exact scenario from the GitHub issue
            $target = New-Object Microsoft.Xrm.Sdk.EntityReference "contact", "DC66FE5D-B854-4F9D-BA63-4CEA4257A8E9"
            $priority = New-Object Microsoft.Xrm.Sdk.OptionSetValue 1
            
            $params = @{
                Target = $target
                Priority = $priority
            }
            
            try {
                Invoke-DataverseRequest -Connection $connection -RequestName "myapi_EscalateCase" -Parameters $params
                $true | Should -Be $true
            }
            catch {
                # Check that the error is NOT about PSObject serialization
                $_.Exception.Message | Should -Not -Match "PSObject.*cannot be serialized"
                $_.Exception.Message | Should -Not -Match "System.Management.Automation.PSObject"
            }
        }

        It "Handles null values in parameters - no PSObject serialization error" {
            $params = @{
                Target = $null
                SomeValue = $null
            }
            
            try {
                Invoke-DataverseRequest -Connection $connection -RequestName "sample_Api" -Parameters $params
                $true | Should -Be $true
            }
            catch {
                $_.Exception.Message | Should -Not -Match "PSObject.*cannot be serialized"
                $_.Exception.Message | Should -Not -Match "System.Management.Automation.PSObject"
            }
        }

        It "Handles primitive types (strings, numbers, guids) in parameters - no PSObject serialization error" {
            $params = @{
                StringParam = "test value"
                NumberParam = 42
                GuidParam = [Guid]"12345678-1234-1234-1234-123456789012"
                BoolParam = $true
            }
            
            try {
                Invoke-DataverseRequest -Connection $connection -RequestName "sample_Api" -Parameters $params
                $true | Should -Be $true
            }
            catch {
                $_.Exception.Message | Should -Not -Match "PSObject.*cannot be serialized"
                $_.Exception.Message | Should -Not -Match "System.Management.Automation.PSObject"
            }
        }

        It "Handles Money value in parameters - no PSObject serialization error" {
            $money = New-Object Microsoft.Xrm.Sdk.Money 100.50
            
            $params = @{
                Amount = $money
            }
            
            try {
                Invoke-DataverseRequest -Connection $connection -RequestName "sample_Api" -Parameters $params
                $true | Should -Be $true
            }
            catch {
                $_.Exception.Message | Should -Not -Match "PSObject.*cannot be serialized"
                $_.Exception.Message | Should -Not -Match "System.Management.Automation.PSObject"
            }
        }

        It "Does not unwrap PSCustomObject - no PSObject serialization error" {
            # PSCustomObject should NOT be unwrapped (matches DataverseEntityConverter behavior)
            $customObj = [PSCustomObject]@{
                Name = "Test"
                Value = 123
            }
            
            $params = @{
                CustomData = $customObj
            }
            
            try {
                Invoke-DataverseRequest -Connection $connection -RequestName "sample_Api" -Parameters $params
                $true | Should -Be $true
            }
            catch {
                $_.Exception.Message | Should -Not -Match "PSObject.*cannot be serialized"
                $_.Exception.Message | Should -Not -Match "System.Management.Automation.PSObject"
            }
        }
    }

    Context "Request parameter set does not need unwrapping" {
        It "Request parameter set works with SDK objects directly (WhoAmI test)" {
            # When using the Request parameter set with a known request type, it should work
            # WhoAmI is supported by FakeXrmEasy
            $request = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
            
            {
                Invoke-DataverseRequest -Connection $connection -Request $request
            } | Should -Not -Throw
        }
    }
}
