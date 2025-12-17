. $PSScriptRoot/Common.ps1

# Note: These unit tests are skipped because they require full organization metadata
# which is complex to mock. The e2e tests in e2e-tests/OrganizationSettings.Tests.ps1
# provide proper validation against a real Dataverse environment.

Describe 'Get-DataverseOrganizationSettings' -Skip {
    It "Gets the single organization record and converts it to PSObject" {
        $connection = getMockConnection -Entities @("organization")
        
        # Create an organization record
        $org = New-Object Microsoft.Xrm.Sdk.Entity("organization")
        $org.Id = [Guid]::NewGuid()
        $org["organizationid"] = $org.Id
        $org["name"] = "Test Organization"
        
        $connection.Create($org) | Out-Null
        
        $result = Get-DataverseOrganizationSettings -Connection $connection
        
        $result | Should -Not -BeNullOrEmpty
        $result.TableName | Should -Be "organization"
        $result.name | Should -Be "Test Organization"
    }

    It "Returns warning when no organization record exists" {
        $connection = getMockConnection -Entities @("organization")
        
        # Don't create any organization record
        $warnings = @()
        $result = Get-DataverseOrganizationSettings -Connection $connection -WarningVariable warnings
        
        $result | Should -BeNullOrEmpty
        $warnings | Should -Not -BeNullOrEmpty
    }

    It "Parses OrgDbOrgSettings XML and creates OrgDbOrgSettingsParsed property" {
        $connection = getMockConnection -Entities @("organization")
        
        # Create an organization record with OrgDbOrgSettings XML
        $org = New-Object Microsoft.Xrm.Sdk.Entity("organization")
        $org.Id = [Guid]::NewGuid()
        $org["organizationid"] = $org.Id
        $org["name"] = "Test Organization"
        $org["orgdborgsettings"] = @"
<OrgSettings>
    <EnableBingMapsIntegration>true</EnableBingMapsIntegration>
    <MaxUploadFileSize>5242880</MaxUploadFileSize>
    <DefaultRecurrenceEndRangeType>NoEndDate</DefaultRecurrenceEndRangeType>
</OrgSettings>
"@
        
        $connection.Create($org) | Out-Null
        
        $result = Get-DataverseOrganizationSettings -Connection $connection
        
        $result | Should -Not -BeNullOrEmpty
        $result.OrgDbOrgSettingsParsed | Should -Not -BeNullOrEmpty
        $result.OrgDbOrgSettingsParsed.EnableBingMapsIntegration | Should -Be $true
        $result.OrgDbOrgSettingsParsed.MaxUploadFileSize | Should -Be 5242880
        $result.OrgDbOrgSettingsParsed.DefaultRecurrenceEndRangeType | Should -Be "NoEndDate"
    }

    It "Removes raw XML when IncludeRawXml is not specified" {
        $connection = getMockConnection -Entities @("organization")
        
        # Create an organization record with OrgDbOrgSettings XML
        $org = New-Object Microsoft.Xrm.Sdk.Entity("organization")
        $org.Id = [Guid]::NewGuid()
        $org["organizationid"] = $org.Id
        $org["orgdborgsettings"] = "<OrgSettings><TestSetting>value</TestSetting></OrgSettings>"
        
        $connection.Create($org) | Out-Null
        
        $result = Get-DataverseOrganizationSettings -Connection $connection
        
        $result.PSObject.Properties["orgdborgsettings"] | Should -BeNullOrEmpty
    }

    It "Includes raw XML when IncludeRawXml is specified" {
        $connection = getMockConnection -Entities @("organization")
        
        # Create an organization record with OrgDbOrgSettings XML
        $org = New-Object Microsoft.Xrm.Sdk.Entity("organization")
        $org.Id = [Guid]::NewGuid()
        $org["organizationid"] = $org.Id
        $org["orgdborgsettings"] = "<OrgSettings><TestSetting>value</TestSetting></OrgSettings>"
        
        $connection.Create($org) | Out-Null
        
        $result = Get-DataverseOrganizationSettings -Connection $connection -IncludeRawXml
        
        $result.orgdborgsettings | Should -Not -BeNullOrEmpty
        $result.orgdborgsettings | Should -BeLike "*<TestSetting>*"
    }
}

Describe 'Set-DataverseOrganizationSettings' -Skip {
    It "Updates organization record properties" {
        $connection = getMockConnection -Entities @("organization")
        
        # Create an organization record
        $org = New-Object Microsoft.Xrm.Sdk.Entity("organization")
        $org.Id = [Guid]::NewGuid()
        $org["organizationid"] = $org.Id
        $org["name"] = "Original Name"
        
        $connection.Create($org) | Out-Null
        
        # Update the name
        $updateObj = [PSCustomObject]@{
            name = "Updated Name"
        }
        
        Set-DataverseOrganizationSettings -Connection $connection -InputObject $updateObj -Confirm:$false
        
        # Verify the update
        $result = Get-DataverseOrganizationSettings -Connection $connection
        $result.name | Should -Be "Updated Name"
    }

    It "Returns updated record when PassThru is specified" {
        $connection = getMockConnection -Entities @("organization")
        
        # Create an organization record
        $org = New-Object Microsoft.Xrm.Sdk.Entity("organization")
        $org.Id = [Guid]::NewGuid()
        $org["organizationid"] = $org.Id
        $org["name"] = "Original Name"
        
        $connection.Create($org) | Out-Null
        
        # Update with PassThru
        $updateObj = [PSCustomObject]@{
            name = "Updated Name"
        }
        
        $result = Set-DataverseOrganizationSettings -Connection $connection -InputObject $updateObj -PassThru -Confirm:$false
        
        $result | Should -Not -BeNullOrEmpty
        $result.name | Should -Be "Updated Name"
    }

    It "Updates OrgDbOrgSettings XML using OrgDbOrgSettingsUpdate property" {
        $connection = getMockConnection -Entities @("organization")
        
        # Create an organization record with initial XML
        $org = New-Object Microsoft.Xrm.Sdk.Entity("organization")
        $org.Id = [Guid]::NewGuid()
        $org["organizationid"] = $org.Id
        $org["orgdborgsettings"] = "<OrgSettings><Setting1>value1</Setting1></OrgSettings>"
        
        $connection.Create($org) | Out-Null
        
        # Update a setting
        $updateObj = [PSCustomObject]@{
            OrgDbOrgSettingsUpdate = [PSCustomObject]@{
                Setting1 = "newvalue1"
                Setting2 = "value2"
            }
        }
        
        Set-DataverseOrganizationSettings -Connection $connection -InputObject $updateObj -Confirm:$false
        
        # Verify the XML was updated
        $result = Get-DataverseOrganizationSettings -Connection $connection -IncludeRawXml
        $result.orgdborgsettings | Should -BeLike "*<Setting1>newvalue1</Setting1>*"
        $result.orgdborgsettings | Should -BeLike "*<Setting2>value2</Setting2>*"
    }

    It "Respects WhatIf parameter" {
        $connection = getMockConnection -Entities @("organization")
        
        # Create an organization record
        $org = New-Object Microsoft.Xrm.Sdk.Entity("organization")
        $org.Id = [Guid]::NewGuid()
        $org["organizationid"] = $org.Id
        $org["name"] = "Original Name"
        
        $connection.Create($org) | Out-Null
        
        # Try to update with WhatIf
        $updateObj = [PSCustomObject]@{
            name = "Updated Name"
        }
        
        Set-DataverseOrganizationSettings -Connection $connection -InputObject $updateObj -WhatIf
        
        # Verify the record was NOT updated
        $result = Get-DataverseOrganizationSettings -Connection $connection
        $result.name | Should -Be "Original Name"
    }

    It "Throws error when no organization record exists" {
        $connection = getMockConnection -Entities @("organization")
        
        # Don't create any organization record
        $updateObj = [PSCustomObject]@{
            name = "Test Name"
        }
        
        { Set-DataverseOrganizationSettings -Connection $connection -InputObject $updateObj -Confirm:$false -ErrorAction Stop } | Should -Throw
    }

    It "Handles empty InputObject gracefully" {
        $connection = getMockConnection -Entities @("organization")
        
        # Create an organization record
        $org = New-Object Microsoft.Xrm.Sdk.Entity("organization")
        $org.Id = [Guid]::NewGuid()
        $org["organizationid"] = $org.Id
        $org["name"] = "Original Name"
        
        $connection.Create($org) | Out-Null
        
        # Try to update with empty object
        $updateObj = [PSCustomObject]@{}
        
        $warnings = @()
        Set-DataverseOrganizationSettings -Connection $connection -InputObject $updateObj -WarningVariable warnings -Confirm:$false
        
        $warnings | Should -Not -BeNullOrEmpty
        $warnings[0] | Should -BeLike "*No valid properties*"
    }
}
