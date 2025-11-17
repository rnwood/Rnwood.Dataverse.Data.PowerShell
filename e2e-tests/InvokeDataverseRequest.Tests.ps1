$ErrorActionPreference = "Stop"

Describe "Invoke-DataverseRequest E2E Tests" {

    BeforeAll {
        if ($env:TESTMODULEPATH) {
            $source = $env:TESTMODULEPATH
        }
        else {
            $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
        }

        $tempmodulefolder = "$([IO.Path]::GetTempPath())/$([Guid]::NewGuid())"
        new-item -ItemType Directory $tempmodulefolder
        copy-item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
        $env:PSModulePath = $tempmodulefolder;
        $env:ChildProcessPSModulePath = $tempmodulefolder

        Import-Module Rnwood.Dataverse.Data.PowerShell
    }

    It "Can invoke WhoAmI request using NameAndInputs parameter set" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            
            # Test WhoAmI using NameAndInputs parameter set
            $response = Invoke-DataverseRequest -Connection $connection -RequestName "WhoAmI" -Parameters @{}
            
            # Verify response has expected properties
            if (-not $response.UserId) {
                throw "Response missing UserId property"
            }
            
            if (-not $response.OrganizationId) {
                throw "Response missing OrganizationId property"
            }
            
            Write-Host "✓ WhoAmI request succeeded with UserId: $($response.UserId)"
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Can invoke WhoAmI request using Request parameter set" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            
            # Test WhoAmI using Request parameter set
            $request = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
            $response = Invoke-DataverseRequest -Connection $connection -Request $request
            
            # Verify response has expected properties (raw response)
            if (-not $response.Results["UserId"]) {
                throw "Response missing UserId in Results"
            }
            
            if (-not $response.Results["OrganizationId"]) {
                throw "Response missing OrganizationId in Results"
            }
            
            Write-Host "✓ WhoAmI request succeeded with UserId: $($response.Results['UserId'])"
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Can invoke WhoAmI request using NameAndInputs with -Raw parameter" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            
            # Test WhoAmI using NameAndInputs with -Raw
            $response = Invoke-DataverseRequest -Connection $connection -RequestName "WhoAmI" -Parameters @{} -Raw
            
            # Verify response has expected properties (raw response)
            if (-not $response.Results["UserId"]) {
                throw "Response missing UserId in Results"
            }
            
            Write-Host "✓ WhoAmI request with -Raw succeeded"
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Can invoke REST API with simple resource name" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true
            
            # Test REST API with simple resource name (without any slashes)
            # Note: The REST parameter set is intended for custom actions and simple resource names
            # For full Web API operations, use the NameAndInputs or Request parameter sets
            $response = Invoke-DataverseRequest -Connection $connection -Method Get -Path "WhoAmI"
            
            # Verify response structure
            if (-not $response.UserId) {
                throw "Response missing 'UserId' property"
            }
            
            Write-Host "✓ REST API with simple resource name succeeded"
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Throws error when path contains forward slash" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true
            
            # This should throw an error because the path contains '/'
            $errorThrown = $false
            try {
                $response = Invoke-DataverseRequest -Connection $connection -Method Get -Path "/api/data/v9.2/systemusers"
            }
            catch {
                $errorThrown = $true
                $errorMessage = $_.Exception.Message
                
                # Verify the error message is helpful
                if ($errorMessage -notlike "*should not contain*/*") {
                    throw "Error message does not contain expected guidance. Message: $errorMessage"
                }
                
                Write-Host "✓ Correctly threw error with helpful message: $errorMessage"
            }
            
            if (-not $errorThrown) {
                throw "Expected an error to be thrown for path containing '/'"
            }
        }
        catch {
            # If the error is from our validation, this is expected
            if ($_.Exception.Message -like "*should not contain*/*") {
                Write-Host "✓ Validation correctly prevents paths with forward slashes"
            }
            else {
                Write-Host "ERROR: $($_ | Out-String)"
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }
    }

    It "Allows forward slash in query string" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true
            
            # This should succeed because '/' is in the query string portion, not the resource name
            # Testing with a hypothetical filter that contains '/' in the value
            $response = Invoke-DataverseRequest -Connection $connection -Method Get -Path "WhoAmI?test=value/with/slashes"
            
            # Verify response (WhoAmI should ignore query parameters and still work)
            if (-not $response.UserId) {
                throw "Response missing expected property"
            }
            
            Write-Host "✓ Forward slash in query string is correctly allowed"
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Can invoke custom action using REST parameter set" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true
            
            # Test REST API with a custom action name (no slashes)
            # For this test, we'll use WhoAmI as a representative custom action
            $response = Invoke-DataverseRequest -Connection $connection -Method Get -Path "WhoAmI"
            
            # Verify response
            if (-not $response.UserId) {
                throw "Response missing expected property"
            }
            
            Write-Host "✓ REST API custom action call succeeded"
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Can invoke batch requests using NameAndInputs parameter set" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            
            # Test batch processing with multiple WhoAmI requests
            $responses = @()
            1..3 | ForEach-Object {
                $response = Invoke-DataverseRequest -Connection $connection -RequestName "WhoAmI" -Parameters @{} -BatchSize 3
                $responses += $response
            }
            
            # Verify we got responses
            if ($responses.Count -ne 3) {
                throw "Expected 3 responses, got $($responses.Count)"
            }
            
            Write-Host "✓ Batch request processing succeeded"
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }
}
