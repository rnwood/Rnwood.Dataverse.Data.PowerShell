using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Sql
{
    /// <summary>
    /// Tests for Invoke-DataverseSql with AdditionalConnections.
    /// Converted from e2e-tests/InvokeDataverseSql.Tests.ps1
    /// </summary>
    public class InvokeDataverseSqlTests : E2ETestBase
    {
        [Fact]
        public void CanExecuteBasicSqlQueryWithSingleConnection()
        {


            var script = GetConnectionScript(@"
try {
    Invoke-WithRetry {
        $results = Invoke-DataverseSql -Connection $connection -Sql ""SELECT TOP 5 fullname FROM systemuser""

        if ($null -eq $results -or ($results | Measure-Object).Count -eq 0) {
            throw ""Query returned no results""
        }

        Write-Host ""Query returned $(($results | Measure-Object).Count) records""
    }
} catch {
    Write-ErrorDetails $_
    throw
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("records", because: result.GetFullOutput());
        }

        [Fact]
        public void CanExecuteSqlQueryWithAdditionalConnectionsUsingSameEnvironment()
        {


            var additionalScript = string.Format(@"
try {{
    Invoke-WithRetry {{
        # Create additional connections to the same environment with different names
        $connection2 = Get-DataverseConnection -Url '{0}' -ClientId '{1}' -ClientSecret '{2}' -ErrorAction Stop
        $connection3 = Get-DataverseConnection -Url '{0}' -ClientId '{1}' -ClientSecret '{2}' -ErrorAction Stop

        # Create hashtable with additional connections
        $additionalConnections = @{{
            ""secondary"" = $connection2
            ""tertiary"" = $connection3
        }}

        # Execute query that references data sources
        $results = Invoke-DataverseSql -Connection $connection -AdditionalConnections $additionalConnections -Sql ""SELECT TOP 3 fullname FROM systemuser""

        if ($null -eq $results -or ($results | Measure-Object).Count -eq 0) {{
            throw ""Query returned no results""
        }}

        Write-Host ""Query with additional connections returned $(($results | Measure-Object).Count) records""
    }}
}} catch {{
    Write-ErrorDetails $_
    throw
}}
", E2ETestsUrl, E2ETestsClientId, E2ETestsClientSecret);
            
            var script = GetConnectionScript(additionalScript);

            
            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("additional connections", because: result.GetFullOutput());
        }

        [Fact]
        public void CanExecuteCrossDatasourceQueryUsingAdditionalConnections()
        {


            var additionalScript = string.Format(@"
try {{
    Invoke-WithRetry {{
        # Create additional connection
        $connection2 = Get-DataverseConnection -Url '{0}' -ClientId '{1}' -ClientSecret '{2}' -ErrorAction Stop

        # Create hashtable with additional connections
        $additionalConnections = @{{
            ""secondary"" = $connection2
        }}

        # Get the primary data source name
        $orgName = $connection.ConnectedOrgUniqueName

        # Execute a cross-datasource query
        $sql = ""SELECT TOP 2 u1.fullname AS primary_name, u2.fullname AS secondary_name 
                FROM $orgName..systemuser u1 
                CROSS JOIN secondary..systemuser u2""

        $results = Invoke-DataverseSql -Connection $connection -AdditionalConnections $additionalConnections -Sql $sql

        if ($null -eq $results -or ($results | Measure-Object).Count -eq 0) {{
            throw ""Cross-datasource query returned no results""
        }}

        # Verify that results have both columns from different data sources
        $firstResult = $results | Select-Object -First 1
        if (-not $firstResult.PSObject.Properties[""primary_name""] -or -not $firstResult.PSObject.Properties[""secondary_name""]) {{
            throw ""Cross-datasource query did not return expected columns""
        }}

        Write-Host ""Cross-datasource query returned $(($results | Measure-Object).Count) records""
    }}
}} catch {{
    Write-ErrorDetails $_
    throw
}}
", E2ETestsUrl, E2ETestsClientId, E2ETestsClientSecret);

            var script = GetConnectionScript(additionalScript);

            
            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Cross-datasource query", because: result.GetFullOutput());
        }

        [Fact]
        public void CanExecuteQuerySelectingFromAdditionalDataSourceByName()
        {


            var additionalScript = string.Format(@"
try {{
    Invoke-WithRetry {{
        # Create additional connection
        $connection2 = Get-DataverseConnection -Url '{0}' -ClientId '{1}' -ClientSecret '{2}' -ErrorAction Stop

        # Create hashtable with additional connections
        $additionalConnections = @{{
            ""alternate"" = $connection2
        }}

        # Query directly from the additional data source
        $sql = ""SELECT TOP 3 fullname FROM alternate..systemuser""

        $results = Invoke-DataverseSql -Connection $connection -AdditionalConnections $additionalConnections -Sql $sql

        if ($null -eq $results -or ($results | Measure-Object).Count -eq 0) {{
            throw ""Query from additional data source returned no results""
        }}

        Write-Host ""Query from additional data source 'alternate' returned $(($results | Measure-Object).Count) records""
    }}
}} catch {{
    Write-ErrorDetails $_
    throw
}}
", E2ETestsUrl, E2ETestsClientId, E2ETestsClientSecret);

            var script = GetConnectionScript(additionalScript);

            
            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("alternate", because: result.GetFullOutput());
        }

        [Fact]
        public void ThrowsErrorWhenAdditionalConnectionsContainsInvalidValueType()
        {


            var script = GetConnectionScript(@"
# Create hashtable with invalid connection type
$additionalConnections = @{
    ""invalid"" = ""not a connection""
}

# This should throw an error
$errorThrown = $false
try {
    Invoke-DataverseSql -Connection $connection -AdditionalConnections $additionalConnections -Sql ""SELECT TOP 1 fullname FROM systemuser""
} catch {
    if ($_.Exception.Message -notlike ""*must be a ServiceClient or IOrganizationService instance*"") {
        throw ""Expected specific error message about invalid type, got: $($_.Exception.Message)""
    }
    $errorThrown = $true
    Write-Host ""Correctly threw error for invalid connection type""
}

if (-not $errorThrown) {
    throw ""Expected an error when passing invalid connection type, but none was thrown""
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Correctly threw error", because: result.GetFullOutput());
        }

        [Fact]
        public void CanUseDataSourceNameParameterToOverrideDefaultDataSourceName()
        {


            var additionalScript = string.Format(@"
# Create additional connection
$connection2 = Get-DataverseConnection -Url '{0}' -ClientId '{1}' -ClientSecret '{2}' -ErrorAction Stop

# Create hashtable with additional connections
$additionalConnections = @{{
    ""secondary"" = $connection2
}}

# Use explicit DataSourceName instead of default org unique name
# Note: Using 'main' instead of 'primary' because 'primary' is a SQL reserved keyword
$sql = ""SELECT TOP 2 u1.fullname AS main_name, u2.fullname AS secondary_name 
        FROM main..systemuser u1 
        CROSS JOIN secondary..systemuser u2""

$results = Invoke-DataverseSql -Connection $connection -DataSourceName ""main"" -AdditionalConnections $additionalConnections -Sql $sql

if ($null -eq $results -or ($results | Measure-Object).Count -eq 0) {{
    throw ""Query with DataSourceName returned no results""
}}

# Verify that results have both columns
$firstResult = $results | Select-Object -First 1
if (-not $firstResult.PSObject.Properties[""main_name""] -or -not $firstResult.PSObject.Properties[""secondary_name""]) {{
    throw ""Query with DataSourceName did not return expected columns""
}}

Write-Host ""Query with DataSourceName='main' returned $(($results | Measure-Object).Count) records""
", E2ETestsUrl, E2ETestsClientId, E2ETestsClientSecret);

            var script = GetConnectionScript(additionalScript);

            
            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("DataSourceName", because: result.GetFullOutput());
        }

        [Fact]
        public void CanUseDateFromPartsAndEoMonthViaTdsEndpoint()
        {
            var script = GetConnectionScript(@"
try {
    Invoke-WithRetry {
        # Test DATEFROMPARTS(2024, 3, 1) = 2024-03-01
        $dateResult = Invoke-DataverseSql -Connection $connection -UseTdsEndpoint -Sql ""SELECT DATEFROMPARTS(2024, 3, 1) AS testdate""
        if ($null -eq $dateResult) {
            throw ""DATEFROMPARTS returned no result""
        }
        $d = [datetime]$dateResult.testdate
        if ($d.Year -ne 2024 -or $d.Month -ne 3 -or $d.Day -ne 1) {
            throw ""DATEFROMPARTS returned unexpected value: $($dateResult.testdate)""
        }
        Write-Host ""DATEFROMPARTS result: $($dateResult.testdate)""

        # Test EOMONTH('2024-03-01') = 2024-03-31
        $eomResult = Invoke-DataverseSql -Connection $connection -UseTdsEndpoint -Sql ""SELECT EOMONTH('2024-03-01') AS endofmonth""
        if ($null -eq $eomResult) {
            throw ""EOMONTH returned no result""
        }
        $eom = [datetime]$eomResult.endofmonth
        if ($eom.Year -ne 2024 -or $eom.Month -ne 3 -or $eom.Day -ne 31) {
            throw ""EOMONTH returned unexpected value: $($eomResult.endofmonth)""
        }
        Write-Host ""EOMONTH result: $($eomResult.endofmonth)""

        # Test EOMONTH(DATEFROMPARTS(2024, 1, 15), 2) = 2024-03-31 (end of March, 2 months after January)
        $eomOffsetResult = Invoke-DataverseSql -Connection $connection -UseTdsEndpoint -Sql ""SELECT EOMONTH(DATEFROMPARTS(2024, 1, 15), 2) AS d""
        if ($null -eq $eomOffsetResult) {
            throw ""EOMONTH with offset returned no result""
        }
        $eomOffset = [datetime]$eomOffsetResult.d
        if ($eomOffset.Year -ne 2024 -or $eomOffset.Month -ne 3 -or $eomOffset.Day -ne 31) {
            throw ""EOMONTH with offset returned unexpected value: $($eomOffsetResult.d)""
        }
        Write-Host ""EOMONTH with offset result: $($eomOffsetResult.d)""

        # Test in a WHERE clause filtering real Dataverse records
        $filterResult = Invoke-DataverseSql -Connection $connection -UseTdsEndpoint -Sql ""SELECT TOP 1 fullname FROM systemuser WHERE createdon >= DATEFROMPARTS(2020, 1, 1) AND createdon <= EOMONTH(GETDATE())""
        Write-Host ""Filter with DATEFROMPARTS+EOMONTH returned records""
    }
} catch {
    Write-ErrorDetails $_
    throw
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("DATEFROMPARTS result:", because: result.GetFullOutput());
            result.StandardOutput.Should().Contain("EOMONTH result:", because: result.GetFullOutput());
            result.StandardOutput.Should().Contain("EOMONTH with offset result:", because: result.GetFullOutput());
        }
    }
}
