using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Request
{
    /// <summary>
    /// Tests for Invoke-DataverseRequest cmdlet.
    /// Converted from e2e-tests/InvokeDataverseRequest.Tests.ps1
    /// </summary>
    public class InvokeDataverseRequestTests : E2ETestBase
    {
        [Fact]
        public void CanInvokeWhoAmIRequestUsingNameAndInputsParameterSet()
        {


            var script = GetConnectionScript(@"
$response = Invoke-DataverseRequest -Connection $connection -RequestName 'WhoAmI' -Parameters @{}

if (-not $response.UserId) {
    throw 'Response missing UserId property'
}

if (-not $response.OrganizationId) {
    throw 'Response missing OrganizationId property'
}

Write-Host ""✓ WhoAmI request succeeded with UserId: $($response.UserId)""
Write-Host 'Success'
");

            var result = RunScript(script, timeoutSeconds: 60);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void CanInvokeWhoAmIRequestUsingRequestParameterSet()
        {


            var script = GetConnectionScript(@"
$request = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
$response = Invoke-DataverseRequest -Connection $connection -Request $request

if (-not $response.Results['UserId']) {
    throw 'Response missing UserId in Results'
}

if (-not $response.Results['OrganizationId']) {
    throw 'Response missing OrganizationId in Results'
}

Write-Host ""✓ WhoAmI request succeeded with UserId: $($response.Results['UserId'])""
Write-Host 'Success'
");

            var result = RunScript(script, timeoutSeconds: 60);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void CanInvokeWhoAmIRequestUsingNameAndInputsWithRawParameter()
        {


            var script = GetConnectionScript(@"
$response = Invoke-DataverseRequest -Connection $connection -RequestName 'WhoAmI' -Parameters @{} -Raw

if (-not $response.Results['UserId']) {
    throw 'Response missing UserId in Results'
}

Write-Host '✓ WhoAmI request with -Raw succeeded'
Write-Host 'Success'
");

            var result = RunScript(script, timeoutSeconds: 60);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void CanInvokeRestApiWithSimpleResourceName()
        {


            var script = GetConnectionScript(@"
$connection.EnableAffinityCookie = $true

$response = Invoke-DataverseRequest -Connection $connection -Method Get -Path 'WhoAmI'

if (-not $response.UserId) {
    throw ""Response missing 'UserId' property""
}

Write-Host '✓ REST API with simple resource name succeeded'
Write-Host 'Success'
");

            var result = RunScript(script, timeoutSeconds: 60);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void ThrowsErrorWhenPathStartsWithApiOrSlashApi()
        {


            var script = GetConnectionScript(@"
$connection.EnableAffinityCookie = $true

$errorThrown = $false
try {
    $response = Invoke-DataverseRequest -Connection $connection -Method Get -Path '/api/data/v9.2/systemusers'
}
catch {
    $errorThrown = $true
    $errorMessage = $_.Exception.Message
    
    if ($errorMessage -notlike '*should not start with*') {
        throw ""Error message does not contain expected guidance. Message: $errorMessage""
    }
    
    Write-Host ""✓ Correctly threw error with helpful message""
}

if (-not $errorThrown) {
    throw 'Expected an error to be thrown for path starting with /api/'
}

Write-Host 'Success'
");

            var result = RunScript(script, timeoutSeconds: 60);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void AllowsForwardSlashInQueryString()
        {


            var script = GetConnectionScript(@"
$connection.EnableAffinityCookie = $true

$response = Invoke-DataverseRequest -Connection $connection -Method Get -Path 'WhoAmI?test=value/with/slashes'

if (-not $response.UserId) {
    throw 'Response missing expected property'
}

Write-Host '✓ Forward slash in query string is correctly allowed'
Write-Host 'Success'
");

            var result = RunScript(script, timeoutSeconds: 60);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void CanInvokeCustomActionUsingRestParameterSet()
        {


            var script = GetConnectionScript(@"
$connection.EnableAffinityCookie = $true

$response = Invoke-DataverseRequest -Connection $connection -Method Get -Path 'WhoAmI'

if (-not $response.UserId) {
    throw 'Response missing expected property'
}

Write-Host '✓ REST API custom action call succeeded'
Write-Host 'Success'
");

            var result = RunScript(script, timeoutSeconds: 60);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void CanInvokeBatchRequestsUsingNameAndInputsParameterSet()
        {


            var script = GetConnectionScript(@"
$responses = @()
1..3 | ForEach-Object {
    $response = Invoke-DataverseRequest -Connection $connection -RequestName 'WhoAmI' -Parameters @{} -BatchSize 3
    $responses += $response
}

if ($responses.Count -ne 3) {
    throw ""Expected 3 responses, got $($responses.Count)""
}

Write-Host '✓ Batch request processing succeeded'
Write-Host 'Success'
");

            var result = RunScript(script, timeoutSeconds: 60);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }
    }
}
