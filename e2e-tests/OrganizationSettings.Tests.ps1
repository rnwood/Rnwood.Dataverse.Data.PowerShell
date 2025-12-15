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

    It "Can get organization settings from a real environment" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Get organization settings
                $orgSettings = Get-DataverseOrganizationSettings -Connection $connection
                
                # Verify we got results
                if (-not $orgSettings) {
                    throw "No organization settings returned"
                }
                
                # Verify basic properties exist
                if (-not $orgSettings.Id) {
                    throw "Organization settings missing Id property"
                }
                
                if (-not $orgSettings.name) {
                    throw "Organization settings missing name property"
                }
                
                Write-Host "Successfully retrieved organization settings for: $($orgSettings.name)"
                Write-Host "Organization ID: $($orgSettings.Id)"
                
                # If OrgDbOrgSettingsParsed exists, show a few settings
                if ($orgSettings.OrgDbOrgSettingsParsed) {
                    Write-Host "OrgDbOrgSettings parsed successfully"
                    $settingCount = ($orgSettings.OrgDbOrgSettingsParsed.PSObject.Properties | Measure-Object).Count
                    Write-Host "Found $settingCount parsed settings"
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
    
    It "Can update organization settings in a real environment" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Get current organization settings to read original value
                $orgSettings = Get-DataverseOrganizationSettings -Connection $connection
                
                if (-not $orgSettings) {
                    throw "No organization settings returned"
                }
                
                Write-Host "Current organization name: $($orgSettings.name)"
                
                # Test update with PassThru (use a safe field that doesn't affect operations)
                # Note: We're just testing the cmdlet works, not actually changing important settings
                # Using the same value to avoid side effects
                $updateObj = [PSCustomObject]@{
                    name = $orgSettings.name
                }
                
                $result = Set-DataverseOrganizationSettings -Connection $connection -InputObject $updateObj -PassThru -Confirm:$false
                
                if (-not $result) {
                    throw "Set-DataverseOrganizationSettings with PassThru returned no result"
                }
                
                if ($result.name -ne $orgSettings.name) {
                    throw "Updated organization name does not match expected value"
                }
                
                Write-Host "Successfully updated organization settings (no-op update to verify cmdlet works)"
                Write-Host "Organization name after update: $($result.name)"
                
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
