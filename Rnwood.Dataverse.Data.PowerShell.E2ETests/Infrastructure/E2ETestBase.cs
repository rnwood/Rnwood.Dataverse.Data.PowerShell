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
        /// Gets the PowerShell statement to import the module.
        /// Uses TESTMODULEPATH environment variable if set (for CI/testing newly built module),
        /// otherwise imports by name (for testing installed module).
        /// </summary>
        protected string GetModuleImportStatement()
        {
            var modulePath = Environment.GetEnvironmentVariable("TESTMODULEPATH");
            
            if (!string.IsNullOrWhiteSpace(modulePath))
            {
                // Import from specific path (for testing newly built module)
                var manifestPath = System.IO.Path.Combine(modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1");
                return $"Import-Module '{manifestPath}' -Force -ErrorAction Stop";
            }
            else
            {
                // Import by name (for testing installed module)
                return "Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop";
            }
        }

        /// <summary>
        /// Creates a PowerShell script that imports the module and establishes a connection.
        /// Includes a centralized Invoke-WithRetry function for handling transient errors.
        /// </summary>
        protected string GetConnectionScript(string additionalScript = "")
        {
            var importStatement = GetModuleImportStatement();
            
            return $@"
{importStatement}

$connection = Get-DataverseConnection -Url '{E2ETestsUrl}' -ClientId '{E2ETestsClientId}' -ClientSecret '{E2ETestsClientSecret}' -ErrorAction Stop

function Write-ErrorDetails {{
    param([Parameter(Mandatory = $true)]$ErrorRecord)
    
    Write-Host '========================================' -ForegroundColor Red
    Write-Host 'ERROR DETAILS' -ForegroundColor Red
    Write-Host '========================================' -ForegroundColor Red
    Write-Host ""Exception Type: $($ErrorRecord.Exception.GetType().FullName)"" -ForegroundColor Red
    Write-Host ""Message: $($ErrorRecord.Exception.Message)"" -ForegroundColor Red
    
    if ($ErrorRecord.Exception.InnerException) {{
        Write-Host ""Inner Exception: $($ErrorRecord.Exception.InnerException.GetType().FullName)"" -ForegroundColor Red
        Write-Host ""Inner Message: $($ErrorRecord.Exception.InnerException.Message)"" -ForegroundColor Red
        
        # Check for nested inner exceptions (e.g., AggregateException)
        $currentInner = $ErrorRecord.Exception.InnerException
        $depth = 1
        while ($currentInner.InnerException -and $depth -lt 5) {{
            $depth++
            $currentInner = $currentInner.InnerException
            Write-Host ""  Inner Exception (depth $depth): $($currentInner.GetType().FullName)"" -ForegroundColor Red
            Write-Host ""  Message: $($currentInner.Message)"" -ForegroundColor Red
        }}
    }}
    
    Write-Host ""Script Line: $($ErrorRecord.InvocationInfo.ScriptLineNumber)"" -ForegroundColor Red
    Write-Host ""Position: $($ErrorRecord.InvocationInfo.PositionMessage)"" -ForegroundColor Red
    Write-Host '========================================' -ForegroundColor Red
    Write-Host 'Stack Trace:' -ForegroundColor Yellow
    Write-Host $ErrorRecord.ScriptStackTrace -ForegroundColor Yellow
    Write-Host '========================================' -ForegroundColor Red
    
    # For AggregateException, show all inner exceptions
    if ($ErrorRecord.Exception -is [System.AggregateException]) {{
        Write-Host 'AggregateException Inner Exceptions:' -ForegroundColor Magenta
        $aggEx = [System.AggregateException]$ErrorRecord.Exception
        $i = 0
        foreach ($inner in $aggEx.InnerExceptions) {{
            $i++
            Write-Host ""  [$i] $($inner.GetType().FullName): $($inner.Message)"" -ForegroundColor Magenta
        }}
        Write-Host '========================================' -ForegroundColor Red
    }}
}}

function Invoke-WithRetry {{
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$ScriptBlock,
        [int]$MaxRetries = 20,
        [int]$DelaySeconds = 10
    )

    $attempt = 0
    $startTime = Get-Date

    while ($attempt -lt $MaxRetries) {{
        try {{
            $attempt++
            Write-Verbose ""Attempt $attempt of $MaxRetries""
            & $ScriptBlock
            return
        }}
        catch {{
            $errorMessage = $_.Exception.Message
            
            # Check if this is a CustomizationLockException that should be retried for up to 30 minutes
            $isCustomizationLock = $errorMessage -match 'CustomizationLockException' -or 
                                   $errorMessage -match 'Cannot start another.*because there is a previous.*running' -or
                                   $errorMessage -match 'solution installation or removal failed'
            
            if ($isCustomizationLock) {{
                $elapsedMinutes = ((Get-Date) - $startTime).TotalMinutes
                $maxMinutes = 30
                
                if ($elapsedMinutes -lt $maxMinutes) {{
                    $retryDelay = 30  # Use 30-second delay for lock exceptions
                    Write-Warning ""CustomizationLockException detected (elapsed: $([int]$elapsedMinutes)m / ${{maxMinutes}}m). Retrying in $retryDelay seconds...""
                    Write-Warning ""Error: $errorMessage""
                    Start-Sleep -Seconds $retryDelay
                    continue
                }} else {{
                    Write-Host ""CustomizationLockException persisted for $maxMinutes minutes. Giving up."" -ForegroundColor Red
                    Write-ErrorDetails $_
                    throw
                }}
            }}
            
            # For other errors, use standard retry logic
            if ($attempt -eq $MaxRetries) {{
                Write-Host ""All $MaxRetries attempts failed."" -ForegroundColor Red
                Write-ErrorDetails $_
                throw
            }}

            Write-Warning ""Attempt $attempt failed: $errorMessage. Retrying in $DelaySeconds seconds...""
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
        /// Timeout is set to 30 minutes.
        /// </summary>
        protected Tests.Infrastructure.PowerShellProcessResult RunScript(string script)
        {
            return Tests.Infrastructure.PowerShellProcessRunner.Run(script, usePowerShellCore: null, timeoutSeconds: 1800, importModule: false);
        }
    }
}
