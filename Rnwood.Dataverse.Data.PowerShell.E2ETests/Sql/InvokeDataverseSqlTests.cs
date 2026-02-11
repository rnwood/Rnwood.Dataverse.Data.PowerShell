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

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("records");
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

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("additional connections");
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

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Cross-datasource query");
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

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("alternate");
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

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Correctly threw error");
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

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("DataSourceName");
        }
    }
}
