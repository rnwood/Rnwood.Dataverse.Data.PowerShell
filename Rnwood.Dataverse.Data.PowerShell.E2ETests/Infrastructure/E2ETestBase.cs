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
<<<<<<< HEAD
        /// Gets the PowerShell statement to import the module.
        /// Uses TESTMODULEPATH environment variable if set (for CI/testing newly built module),
        /// otherwise imports by name (for testing installed module).
        /// </summary>
        protected string GetModuleImportStatement()
        {
            var modulePath = Environment.GetEnvironmentVariable("TESTMODULEPATH");
=======
        /// Creates a PowerShell script that imports the module and establishes a connection.
        /// </summary>
        protected string GetConnectionScript(string additionalScript = "")
        {
            // Get module path from environment variable or use default
            var modulePath = Environment.GetEnvironmentVariable("TESTMODULEPATH");
            string importStatement;
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
            
            if (!string.IsNullOrWhiteSpace(modulePath))
            {
                // Import from specific path (for testing newly built module)
                var manifestPath = System.IO.Path.Combine(modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1");
<<<<<<< HEAD
                return $"Import-Module '{manifestPath}' -Force -ErrorAction Stop";
=======
                importStatement = $"Import-Module '{manifestPath}' -Force -ErrorAction Stop";
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
            }
            else
            {
                // Import by name (for testing installed module)
<<<<<<< HEAD
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
=======
                importStatement = "Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop";
            }
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
            
            return $@"
{importStatement}

$connection = Get-DataverseConnection -Url '{E2ETestsUrl}' -ClientId '{E2ETestsClientId}' -ClientSecret '{E2ETestsClientSecret}' -ErrorAction Stop

<<<<<<< HEAD
function Write-ErrorDetails {{
    param([Parameter(Mandatory = $true)]$ErrorRecord)
    
    Write-Host '========================================' -ForegroundColor Red
    Write-Host 'COMPREHENSIVE ERROR DETAILS' -ForegroundColor Red
    Write-Host '========================================' -ForegroundColor Red
    
    # Output EVERYTHING from the error record
    Write-Host 'Full Error Record:' -ForegroundColor Cyan
    Write-Host ($ErrorRecord | Format-List * -Force | Out-String) -ForegroundColor White
    
    Write-Host '========================================' -ForegroundColor Red
    Write-Host 'Exception Details:' -ForegroundColor Cyan
    Write-Host ""Exception Type: $($ErrorRecord.Exception.GetType().FullName)"" -ForegroundColor Red
    Write-Host ""Message: $($ErrorRecord.Exception.Message)"" -ForegroundColor Red
    
    # Output full exception details
    Write-Host 'Full Exception Object:' -ForegroundColor Cyan
    Write-Host ($ErrorRecord.Exception | Format-List * -Force | Out-String) -ForegroundColor White
    
    if ($ErrorRecord.Exception.InnerException) {{
        Write-Host '========================================' -ForegroundColor Red
        Write-Host ""Inner Exception: $($ErrorRecord.Exception.InnerException.GetType().FullName)"" -ForegroundColor Red
        Write-Host ""Inner Message: $($ErrorRecord.Exception.InnerException.Message)"" -ForegroundColor Red
        Write-Host 'Full Inner Exception Object:' -ForegroundColor Cyan
        Write-Host ($ErrorRecord.Exception.InnerException | Format-List * -Force | Out-String) -ForegroundColor White
        
        # Check for nested inner exceptions (e.g., AggregateException)
        $currentInner = $ErrorRecord.Exception.InnerException
        $depth = 1
        while ($currentInner.InnerException -and $depth -lt 5) {{
            $depth++
            $currentInner = $currentInner.InnerException
            Write-Host ""  Inner Exception (depth $depth): $($currentInner.GetType().FullName)"" -ForegroundColor Red
            Write-Host ""  Message: $($currentInner.Message)"" -ForegroundColor Red
            Write-Host ($currentInner | Format-List * -Force | Out-String) -ForegroundColor White
        }}
    }}
    
    Write-Host '========================================' -ForegroundColor Red
    Write-Host ""Script Line: $($ErrorRecord.InvocationInfo.ScriptLineNumber)"" -ForegroundColor Red
    Write-Host ""Position: $($ErrorRecord.InvocationInfo.PositionMessage)"" -ForegroundColor Red
    
    Write-Host '========================================' -ForegroundColor Red
    Write-Host 'Stack Trace:' -ForegroundColor Yellow
    Write-Host $ErrorRecord.ScriptStackTrace -ForegroundColor Yellow
    
    Write-Host '========================================' -ForegroundColor Red
    Write-Host 'Invocation Info:' -ForegroundColor Cyan
    Write-Host ($ErrorRecord.InvocationInfo | Format-List * -Force | Out-String) -ForegroundColor White
    
    # For AggregateException, show all inner exceptions in detail
    if ($ErrorRecord.Exception -is [System.AggregateException]) {{
        Write-Host '========================================' -ForegroundColor Red
        Write-Host 'AggregateException Inner Exceptions (Detailed):' -ForegroundColor Magenta
        $aggEx = [System.AggregateException]$ErrorRecord.Exception
        $i = 0
        foreach ($inner in $aggEx.InnerExceptions) {{
            $i++
            Write-Host ""  ========== Inner Exception [$i] =========="" -ForegroundColor Magenta
            Write-Host ""  Type: $($inner.GetType().FullName)"" -ForegroundColor Magenta
            Write-Host ""  Message: $($inner.Message)"" -ForegroundColor Magenta
            Write-Host ""  Full Details:"" -ForegroundColor Magenta
            Write-Host ($inner | Format-List * -Force | Out-String) -ForegroundColor White
            
            # Check for inner exception within aggregate exception items
            if ($inner.InnerException) {{
                Write-Host ""  Has Inner Exception: $($inner.InnerException.GetType().FullName)"" -ForegroundColor Magenta
                Write-Host ""  Inner Message: $($inner.InnerException.Message)"" -ForegroundColor Magenta
                Write-Host ($inner.InnerException | Format-List * -Force | Out-String) -ForegroundColor White
            }}
        }}
    }}
    
    Write-Host '========================================' -ForegroundColor Red
    Write-Host 'Error Stream Output:' -ForegroundColor Cyan
    if ($Error -and $Error.Count -gt 0) {{
        Write-Host ""Recent errors in `$Error variable (last 5):"" -ForegroundColor Yellow
        $Error | Select-Object -First 5 | ForEach-Object {{
            Write-Host ""  - $($_.Exception.GetType().FullName): $($_.Exception.Message)"" -ForegroundColor Yellow
        }}
    }}
    Write-Host '========================================' -ForegroundColor Red
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

    while ($true) {{
        try {{
            $attempt++
            Write-Verbose ""Attempt $attempt of $MaxRetries""
            & $ScriptBlock
            return
        }}
        catch {{
            $errorMessage = $_.ToString()
            
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

=======
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
{additionalScript}
";
        }

        /// <summary>
        /// Runs a PowerShell script using PowerShellProcessRunner with importModule set to false
        /// (since E2E test scripts include Import-Module statements themselves).
        /// Uses auto-detection to select the correct PowerShell executable based on target framework.
<<<<<<< HEAD
        /// Timeout is set to 30 minutes.
        /// </summary>
        protected Tests.Infrastructure.PowerShellProcessResult RunScript(string script)
        {
            return Tests.Infrastructure.PowerShellProcessRunner.Run(script, usePowerShellCore: null, timeoutSeconds: 1800, importModule: false);
=======
        /// </summary>
        protected Tests.Infrastructure.PowerShellProcessResult RunScript(string script, int timeoutSeconds = 60)
        {
            return Tests.Infrastructure.PowerShellProcessRunner.Run(script, usePowerShellCore: null, timeoutSeconds: timeoutSeconds, importModule: false);
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        }
    }
}
