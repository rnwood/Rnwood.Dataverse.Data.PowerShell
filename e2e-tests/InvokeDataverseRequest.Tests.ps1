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

    It "Can invoke REST API with relative path" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true
            
            # Test REST API with relative path (without leading /)
            $response = Invoke-DataverseRequest -Connection $connection -Method Get -Path "api/data/v9.2/systemusers?`$select=fullname&`$top=1"
            
            # Verify response structure
            if (-not $response.value) {
                throw "Response missing 'value' property"
            }
            
            Write-Host "✓ REST API with relative path succeeded, returned $($response.value.Count) records"
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Can invoke REST API with absolute path" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true
            
            # Test REST API with absolute path (with leading /)
            $response = Invoke-DataverseRequest -Connection $connection -Method Get -Path "/api/data/v9.2/systemusers?`$select=fullname&`$top=1"
            
            # Verify response structure
            if (-not $response.value) {
                throw "Response missing 'value' property"
            }
            
            Write-Host "✓ REST API with absolute path succeeded, returned $($response.value.Count) records"
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Can invoke REST API POST with body" {
        $ErrorActionPreference = "Stop"
        $ConfirmPreference = 'None'
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true
            
            # Create a test contact using REST API
            $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
            $body = @{
                firstname = "Test"
                lastname = "InvokeRequest_$testRunId"
            }
            
            $createResponse = Invoke-DataverseRequest -Connection $connection -Method Post -Path "api/data/v9.2/contacts" -Body $body
            
            # Extract contact ID from response header (OData-EntityId)
            # For now, just verify the request succeeded
            Write-Host "✓ REST API POST with body succeeded"
            
            # Clean up - try to delete the created contact
            # We need to query for it first since we don't have the ID from the response
            $queryResponse = Invoke-DataverseRequest -Connection $connection -Method Get -Path "api/data/v9.2/contacts?`$filter=lastname eq 'InvokeRequest_$testRunId'&`$select=contactid"
            
            if ($queryResponse.value -and $queryResponse.value.Count -gt 0) {
                $contactId = $queryResponse.value[0].contactid
                Invoke-DataverseRequest -Connection $connection -Method Delete -Path "api/data/v9.2/contacts($contactId)" | Out-Null
                Write-Host "✓ Test contact cleaned up"
            }
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Can invoke REST API with custom headers" {
        $ErrorActionPreference = "Stop"
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true
            
            # Test REST API with custom headers
            # Prefer header controls the return representation preference
            $customHeaders = @{
                "Prefer" = "odata.include-annotations=*"
            }
            
            $response = Invoke-DataverseRequest -Connection $connection -Method Get -Path "api/data/v9.2/systemusers?`$select=fullname&`$top=1" -CustomHeaders $customHeaders
            
            # Verify response structure
            if (-not $response.value) {
                throw "Response missing 'value' property"
            }
            
            Write-Host "✓ REST API with custom headers succeeded"
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }

    It "Can invoke REST API PATCH operation" {
        $ErrorActionPreference = "Stop"
        $ConfirmPreference = 'None'
        
        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true
            
            # Create a test contact first
            $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
            $body = @{
                firstname = "Test"
                lastname = "InvokeRequest_$testRunId"
            }
            
            Invoke-DataverseRequest -Connection $connection -Method Post -Path "api/data/v9.2/contacts" -Body $body | Out-Null
            
            # Query for the created contact
            $queryResponse = Invoke-DataverseRequest -Connection $connection -Method Get -Path "api/data/v9.2/contacts?`$filter=lastname eq 'InvokeRequest_$testRunId'&`$select=contactid,firstname"
            
            if ($queryResponse.value -and $queryResponse.value.Count -gt 0) {
                $contactId = $queryResponse.value[0].contactid
                
                # Update the contact using PATCH
                $updateBody = @{
                    firstname = "Updated"
                }
                
                Invoke-DataverseRequest -Connection $connection -Method Patch -Path "api/data/v9.2/contacts($contactId)" -Body $updateBody | Out-Null
                
                Write-Host "✓ REST API PATCH operation succeeded"
                
                # Verify the update
                $verifyResponse = Invoke-DataverseRequest -Connection $connection -Method Get -Path "api/data/v9.2/contacts($contactId)?`$select=firstname"
                
                if ($verifyResponse.firstname -ne "Updated") {
                    throw "Update verification failed: expected 'Updated', got '$($verifyResponse.firstname)'"
                }
                
                Write-Host "✓ Update verified successfully"
                
                # Clean up
                Invoke-DataverseRequest -Connection $connection -Method Delete -Path "api/data/v9.2/contacts($contactId)" | Out-Null
                Write-Host "✓ Test contact cleaned up"
            } else {
                throw "Failed to find created contact for PATCH test"
            }
        }
        catch {
            Write-Host "ERROR: $($_ | Out-String)"
            throw "Failed: " + ($_ | Format-Table -force * | Out-String)
        }
    }
}
