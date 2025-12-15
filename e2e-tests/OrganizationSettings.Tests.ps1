$ErrorActionPreference = "Stop"

Describe "OrganizationSettings" -Skip:$false {

    BeforeAll {

        if ($env:TESTMODULEPATH) {
            $source = $env:TESTMODULEPATH
        }
        else {
            $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
        }

        $tempmodulefolder = "$([IO.Path]::GetTempPath())/$([Guid]::NewGuid())"
        New-Item -ItemType Directory $tempmodulefolder | Out-Null
        Copy-Item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
        $env:PSModulePath = $tempmodulefolder
        $env:ChildProcessPSModulePath = $tempmodulefolder
    }

    It "Can get organization record from a real environment" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Get organization record (without -OrgDbOrgSettings)
                $org = Get-DataverseOrganizationSettings -Connection $connection
                
                # Verify we got results
                if (-not $org) {
                    throw "No organization record returned"
                }
                
                # Verify basic properties exist
                if (-not $org.Id) {
                    throw "Organization record missing Id property"
                }
                
                if (-not $org.name) {
                    throw "Organization record missing name property"
                }
                
                Write-Host "Successfully retrieved organization record for: $($org.name)"
                Write-Host "Organization ID: $($org.Id)"
                
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
    
    It "Can get OrgDbOrgSettings from a real environment" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Get OrgDbOrgSettings only
                $settings = Get-DataverseOrganizationSettings -Connection $connection -OrgDbOrgSettings
                
                # Verify we got results
                if ($null -eq $settings) {
                    throw "No OrgDbOrgSettings returned"
                }
                
                Write-Host "Successfully retrieved OrgDbOrgSettings"
                $settingCount = ($settings.PSObject.Properties | Measure-Object).Count
                Write-Host "Found $settingCount parsed settings"
                
                # Show a few settings if they exist
                if ($settings.PSObject.Properties['MaxUploadFileSize']) {
                    Write-Host "MaxUploadFileSize: $($settings.MaxUploadFileSize)"
                }
                
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
    
    It "Can update organization table columns in a real environment" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Get current organization record to read original value
                $org = Get-DataverseOrganizationSettings -Connection $connection
                
                if (-not $org) {
                    throw "No organization record returned"
                }
                
                Write-Host "Current organization name: $($org.name)"
                
                # Test update with PassThru (use the same value to avoid side effects)
                # This tests the cmdlet works without actually changing anything
                $updateObj = [PSCustomObject]@{
                    name = $org.name
                }
                
                $result = Set-DataverseOrganizationSettings -Connection $connection -InputObject $updateObj -PassThru -Confirm:$false -Verbose
                
                if (-not $result) {
                    throw "Set-DataverseOrganizationSettings with PassThru returned no result"
                }
                
                if ($result.name -ne $org.name) {
                    throw "Organization name does not match expected value"
                }
                
                Write-Host "Successfully tested updating organization table columns (no-op update)"
                Write-Host "Organization name: $($result.name)"
                
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
    
    It "Can update OrgDbOrgSettings in a real environment" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Get current OrgDbOrgSettings
                $currentSettings = Get-DataverseOrganizationSettings -Connection $connection -OrgDbOrgSettings
                
                # Pick a setting to test with (MaxUploadFileSize is common)
                if ($currentSettings.PSObject.Properties['MaxUploadFileSize']) {
                    $originalValue = $currentSettings.MaxUploadFileSize
                    Write-Host "Current MaxUploadFileSize: $originalValue"
                    
                    # Update with the same value (no-op to avoid side effects)
                    $updateObj = [PSCustomObject]@{
                        MaxUploadFileSize = $originalValue
                    }
                    
                    Set-DataverseOrganizationSettings -Connection $connection -InputObject $updateObj -OrgDbOrgSettings -Confirm:$false -Verbose
                    
                    # Verify it stayed the same
                    $updatedSettings = Get-DataverseOrganizationSettings -Connection $connection -OrgDbOrgSettings
                    if ($updatedSettings.MaxUploadFileSize -ne $originalValue) {
                        throw "MaxUploadFileSize changed unexpectedly"
                    }
                    
                    Write-Host "Successfully tested updating OrgDbOrgSettings (no-op update)"
                    Write-Host "MaxUploadFileSize: $($updatedSettings.MaxUploadFileSize)"
                } else {
                    Write-Host "MaxUploadFileSize setting not found, testing cmdlet without verification"
                    
                    # Just test the cmdlet runs without error
                    $updateObj = [PSCustomObject]@{
                        TestSetting = "TestValue"
                    }
                    Set-DataverseOrganizationSettings -Connection $connection -InputObject $updateObj -OrgDbOrgSettings -Confirm:$false -Verbose
                    Write-Host "Successfully tested updating OrgDbOrgSettings"
                }
                
            } catch {
                # Format error with full details using Format-List with large width to avoid truncation
                $errorDetails = "Failed: " + ($_ | Format-List * -Force | Out-String -Width 10000)
                throw $errorDetails
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
}
