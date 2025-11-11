. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseAdminPortalUrl' {
    Context 'Admin Portal URL Generation' {
        It "Generates URL for environment page by default" {
            $connection = getMockConnection
            
            $url = Get-DataverseAdminPortalUrl -Connection $connection
            
            # Verify URL structure
            $url | Should -Not -BeNullOrEmpty
            $url | Should -BeLike "https://admin.powerplatform.microsoft.com/environments/*"
            $url | Should -BeLike "*/hub"
        }

        It "Generates URL for analytics page" {
            $connection = getMockConnection
            
            $url = Get-DataverseAdminPortalUrl -Connection $connection -Page "analytics"
            
            # Verify URL points to analytics
            $url | Should -BeLike "https://admin.powerplatform.microsoft.com/analytics"
        }

        It "Generates URL for resources page" {
            $connection = getMockConnection
            
            $url = Get-DataverseAdminPortalUrl -Connection $connection -Page "resources"
            
            # Verify URL points to resources
            $url | Should -BeLike "https://admin.powerplatform.microsoft.com/resources"
        }

        It "Generates URL for data integration page" {
            $connection = getMockConnection
            
            $url = Get-DataverseAdminPortalUrl -Connection $connection -Page "dataintegration"
            
            # Verify URL points to data integration
            $url | Should -BeLike "https://admin.powerplatform.microsoft.com/dataintegration"
        }

        It "Generates URL for data policies page" {
            $connection = getMockConnection
            
            $url = Get-DataverseAdminPortalUrl -Connection $connection -Page "datapolicies"
            
            # Verify URL points to data policies
            $url | Should -BeLike "https://admin.powerplatform.microsoft.com/datapolicies"
        }

        It "Generates URL for help and support page" {
            $connection = getMockConnection
            
            $url = Get-DataverseAdminPortalUrl -Connection $connection -Page "helpandsupport"
            
            # Verify URL points to support
            $url | Should -BeLike "https://admin.powerplatform.microsoft.com/support"
        }

        It "Generates URL for home page" {
            $connection = getMockConnection
            
            $url = Get-DataverseAdminPortalUrl -Connection $connection -Page "home"
            
            # Verify URL points to home
            $url | Should -BeLike "https://admin.powerplatform.microsoft.com/home"
        }

        It "Includes environment ID for environments page" {
            $connection = getMockConnection
            
            # Get the organization ID (which is the environment ID)
            $whoami = Get-DataverseWhoAmI -Connection $connection
            $envId = $whoami.OrganizationId
            
            $url = Get-DataverseAdminPortalUrl -Connection $connection -Page "environments"
            
            # Verify URL includes the environment ID
            $url | Should -BeLike "*/environments/$envId/hub"
        }

        It "Does not include environment ID for non-environment pages" {
            $connection = getMockConnection
            
            $url = Get-DataverseAdminPortalUrl -Connection $connection -Page "analytics"
            
            # Verify URL does not include /environments/ path
            $url | Should -Not -BeLike "*/environments/*"
        }

        It "Works with default connection" {
            $connection = getMockConnection
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Call without explicit connection
            $url = Get-DataverseAdminPortalUrl
            
            # Verify URL generated
            $url | Should -Not -BeNullOrEmpty
            $url | Should -BeLike "https://admin.powerplatform.microsoft.com/*"
        }

        It "Handles case-insensitive page parameter" {
            $connection = getMockConnection
            
            $url1 = Get-DataverseAdminPortalUrl -Connection $connection -Page "Analytics"
            $url2 = Get-DataverseAdminPortalUrl -Connection $connection -Page "ANALYTICS"
            $url3 = Get-DataverseAdminPortalUrl -Connection $connection -Page "analytics"
            
            # All should produce the same URL
            $url1 | Should -Be $url2
            $url2 | Should -Be $url3
        }
    }
}
