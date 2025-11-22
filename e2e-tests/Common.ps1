# Common utilities for E2E tests

function Invoke-WithRetry {
    <#
    .SYNOPSIS
    Executes a script block with automatic retry logic for transient errors.
    
    .DESCRIPTION
    Retries the provided script block up to MaxRetries times with exponential backoff.
    Handles special cases for known transient errors that should not count against retry attempts.
    
    .PARAMETER ScriptBlock
    The script block to execute with retry logic.
    
    .PARAMETER MaxRetries
    Maximum number of retry attempts. Default is 5.
    
    .PARAMETER InitialDelaySeconds
    Initial delay in seconds before first retry. Doubles with each retry (exponential backoff). Default is 10.
    
    .EXAMPLE
    Invoke-WithRetry { Get-DataverseRecord -Connection $conn -TableName contact }
    #>
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$ScriptBlock,
        [int]$MaxRetries = 5,
        [int]$InitialDelaySeconds = 10
    )
        
    $attempt = 0
    $delay = $InitialDelaySeconds
        
    while ($attempt -lt $MaxRetries) {
        try {
            $attempt++
            Write-Verbose "Attempt $attempt of $MaxRetries"
            & $ScriptBlock
            return  # Success, exit function
        }
        catch {
            # Check for transient errors that should not count against retry attempts
            $shouldNotCountAsAttempt = $false
            
            # EntityCustomization operation conflicts - wait 2 minutes without counting attempt
            if ($_.Exception.Message -like "*Cannot start the requested operation*EntityCustomization*") {
                Write-Warning "EntityCustomization operation conflict detected. Waiting 2 minutes before retry without incrementing attempt count..."
                $shouldNotCountAsAttempt = $true
                $attempt--  # Don't count this as a retry attempt
                Start-Sleep -Seconds 120
                continue
            }
            
            # SQL transient errors - wait and retry without counting attempt
            if ($_.Exception.Message -like "*System.Data.SqlClient.SqlException (0x80131904)*") {
                Write-Warning "SQL transient error detected. Waiting before retry without incrementing attempt count..."
                $shouldNotCountAsAttempt = $true
                $attempt--  # Don't count this as a retry attempt
                Start-Sleep -Seconds 60
                continue
            }
            
            # Metadata mapping errors - wait and retry without counting attempt
            if ($_.Exception.Message -like "*Mapping failed for Microsoft.Crm.Metadata*") {
                Write-Warning "Metadata mapping error detected. Waiting before retry without incrementing attempt count..."
                $shouldNotCountAsAttempt = $true
                $attempt--  # Don't count this as a retry attempt
                Start-Sleep -Seconds 60
                continue
            }
            
            if ($attempt -eq $MaxRetries) {
                Write-Error "All $MaxRetries attempts failed. Last error: $_"
                throw
            }
                
            Write-Warning "Attempt $attempt failed: $_. Retrying in $delay seconds..."
            Start-Sleep -Seconds $delay
            $delay = $delay * 2  # Exponential backoff
        }
    }
}
