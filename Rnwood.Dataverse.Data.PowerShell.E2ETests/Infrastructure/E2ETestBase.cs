using System;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure
{
    /// <summary>
    /// Base class for E2E tests that require real Dataverse environment credentials.
    /// </summary>
    public class E2ETestBase
    {
        protected string E2ETestsUrl { get; }
        protected string E2ETestsClientId { get; }
        protected string E2ETestsClientSecret { get; }

        protected bool SkipE2ETests { get; }

        protected E2ETestBase()
        {
            E2ETestsUrl = Environment.GetEnvironmentVariable("E2ETESTS_URL") ?? string.Empty;
            E2ETestsClientId = Environment.GetEnvironmentVariable("E2ETESTS_CLIENTID") ?? string.Empty;
            E2ETestsClientSecret = Environment.GetEnvironmentVariable("E2ETESTS_CLIENTSECRET") ?? string.Empty;

            // Skip tests if any required environment variable is missing
            SkipE2ETests = string.IsNullOrWhiteSpace(E2ETestsUrl) ||
                          string.IsNullOrWhiteSpace(E2ETestsClientId) ||
                          string.IsNullOrWhiteSpace(E2ETestsClientSecret);

            if (SkipE2ETests)
            {
                Skip.If(true, "E2E tests skipped: E2ETESTS_URL, E2ETESTS_CLIENTID, or E2ETESTS_CLIENTSECRET environment variables not set");
            }
        }

        /// <summary>
        /// Creates a PowerShell script that imports the module and establishes a connection.
        /// Includes a centralized Invoke-WithRetry function for handling transient errors.
        /// </summary>
        protected string GetConnectionScript(string additionalScript = "")
        {
            // Get module path from environment variable or use default
            var modulePath = Environment.GetEnvironmentVariable("TESTMODULEPATH");
            string importStatement;
            
            if (!string.IsNullOrWhiteSpace(modulePath))
            {
                // Import from specific path (for testing newly built module)
                var manifestPath = System.IO.Path.Combine(modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1");
                importStatement = $"Import-Module '{manifestPath}' -Force -ErrorAction Stop";
            }
            else
            {
                // Import by name (for testing installed module)
                importStatement = "Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop";
            }
            
            return $@"
{importStatement}

$connection = Get-DataverseConnection -Url '{E2ETestsUrl}' -ClientId '{E2ETestsClientId}' -ClientSecret '{E2ETestsClientSecret}' -ErrorAction Stop

function Invoke-WithRetry {{
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$ScriptBlock,
        [int]$MaxRetries = 20,
        [int]$DelaySeconds = 10
    )

    $attempt = 0

    while ($attempt -lt $MaxRetries) {{
        try {{
            $attempt++
            Write-Verbose ""Attempt $attempt of $MaxRetries""
            & $ScriptBlock
            return
        }}
        catch {{
            
            if ($attempt -eq $MaxRetries) {{
                Write-Error ""All $MaxRetries attempts failed. Last error: $_""
                throw
            }}

            Write-Warning ""Attempt $attempt failed: $_. Retrying in $DelaySeconds seconds...""
            Start-Sleep -Seconds $DelaySeconds

        }}
    }}
}}

{additionalScript}
";
        }

        /// <summary>
        /// Runs a PowerShell script using PowerShellProcessRunner with importModule set to false
        /// (since E2E test scripts include Import-Module statements themselves).
        /// Uses auto-detection to select the correct PowerShell executable based on target framework.
        /// </summary>
        protected Tests.Infrastructure.PowerShellProcessResult RunScript(string script, int timeoutSeconds = 60)
        {
            return Tests.Infrastructure.PowerShellProcessRunner.Run(script, usePowerShellCore: null, timeoutSeconds: timeoutSeconds, importModule: false);
        }
    }
}
