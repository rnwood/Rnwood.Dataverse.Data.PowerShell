. $PSScriptRoot/Common.ps1

Describe 'Invoke-DataverseSql - TDS Endpoint Parameter' -Skip {
    # Note: SQL4Cds (MarkMpn.Sql4Cds.Engine) does not fully support FakeXrmEasy mock
    # These tests validate that the -UseTdsEndpoint parameter is properly configured
    # Run these as E2E tests against real environment for full validation.
    Context 'UseTdsEndpoint Parameter' {
        It "Should accept -UseTdsEndpoint parameter without error" {
            $connection = getMockConnection
            
            # This test validates that the -UseTdsEndpoint parameter is properly set
            # The actual TDS endpoint functionality requires a real Dataverse environment
            # but we can verify the parameter doesn't cause initialization errors
            
            # Simple query that should work with mock connection
            $sql = "SELECT TOP 1 contactid FROM contact"
            
            # Execute with -UseTdsEndpoint flag - should not throw during initialization
            { Invoke-DataverseSql -Connection $connection -Sql $sql -UseTdsEndpoint -ErrorAction Stop } | 
                Should -Not -Throw
        }
        
        It "Should accept UseTdsEndpoint without value (as switch parameter)" {
            $connection = getMockConnection
            
            # Test that UseTdsEndpoint works as a switch parameter
            $sql = "SELECT TOP 1 contactid FROM contact"
            
            # This should work - switch parameter without explicit value
            { Invoke-DataverseSql -Connection $connection -Sql $sql -UseTdsEndpoint } | 
                Should -Not -Throw
        }
        
        It "Should work without -UseTdsEndpoint parameter (default behavior)" {
            $connection = getMockConnection
            
            # Test default behavior without the flag
            $sql = "SELECT TOP 1 contactid FROM contact"
            
            # This should work - default non-TDS mode
            { Invoke-DataverseSql -Connection $connection -Sql $sql } | 
                Should -Not -Throw
        }
        
        It "Should handle DATEDIFF queries gracefully with TDS endpoint" {
            $connection = getMockConnection
            
            # The issue reported was with DATEDIFF(QUARTER, 0, GETDATE())
            # With mock connection, this will still fail due to SQL4CDS limitations
            # but the error should be from SQL4CDS, not from parameter initialization
            
            $sql = @'
DECLARE @QuarterStart DATE = DATEADD(QUARTER, DATEDIFF(QUARTER, 0, GETDATE()) - 1, 0);
SELECT @QuarterStart as QuarterStart
'@
            
            # We expect this to fail with SQL4CDS/mock limitations, not parameter errors
            # The specific error message indicates whether our fix worked
            try {
                Invoke-DataverseSql -Connection $connection -Sql $sql -UseTdsEndpoint -ErrorAction Stop
                # If it succeeds (unlikely with mock), that's fine
            }
            catch {
                # We expect SQL4CDS errors, not parameter initialization errors
                # The error should not be about "false" or parameter issues
                $_.Exception.Message | Should -Not -Match "parameter.*false"
                $_.Exception.Message | Should -Not -Match "UseTDSEndpoint.*false"
            }
        }
    }
}
