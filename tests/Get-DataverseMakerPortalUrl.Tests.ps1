. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseMakerPortalUrl' {
    Context 'Maker Portal URL Generation' {
        It "Generates URL for maker portal home page by default" {
            $connection = getMockConnection
            
            $url = Get-DataverseMakerPortalUrl -Connection $connection
            
            # Verify URL structure
            $url | Should -Not -BeNullOrEmpty
            $url | Should -BeLike "https://make.powerapps.com/environments/*"
            $url | Should -BeLike "*/home"
        }

        It "Generates URL for solutions page" {
            $connection = getMockConnection
            
            $url = Get-DataverseMakerPortalUrl -Connection $connection -Page "solutions"
            
            # Verify URL points to solutions
            $url | Should -BeLike "*/solutions"
        }

        It "Generates URL for tables page" {
            $connection = getMockConnection
            
            $url = Get-DataverseMakerPortalUrl -Connection $connection -Page "tables"
            
            # Verify URL points to entities (tables)
            $url | Should -BeLike "*/entities"
        }

        It "Generates URL for entities page (alias for tables)" {
            $connection = getMockConnection
            
            $url = Get-DataverseMakerPortalUrl -Connection $connection -Page "entities"
            
            # Verify URL points to entities
            $url | Should -BeLike "*/entities"
        }

        It "Generates URL for apps page" {
            $connection = getMockConnection
            
            $url = Get-DataverseMakerPortalUrl -Connection $connection -Page "apps"
            
            # Verify URL points to apps
            $url | Should -BeLike "*/apps"
        }

        It "Generates URL for flows page" {
            $connection = getMockConnection
            
            $url = Get-DataverseMakerPortalUrl -Connection $connection -Page "flows"
            
            # Verify URL points to flows
            $url | Should -BeLike "*/flows"
        }

        It "Generates URL for chatbots page" {
            $connection = getMockConnection
            
            $url = Get-DataverseMakerPortalUrl -Connection $connection -Page "chatbots"
            
            # Verify URL points to chatbots
            $url | Should -BeLike "*/chatbots"
        }

        It "Generates URL for connections page" {
            $connection = getMockConnection
            
            $url = Get-DataverseMakerPortalUrl -Connection $connection -Page "connections"
            
            # Verify URL points to connections
            $url | Should -BeLike "*/connections"
        }

        It "Generates URL for dataflows page" {
            $connection = getMockConnection
            
            $url = Get-DataverseMakerPortalUrl -Connection $connection -Page "dataflows"
            
            # Verify URL points to dataflows
            $url | Should -BeLike "*/dataflows"
        }

        It "Includes environment ID from connection" {
            $connection = getMockConnection
            
            # Get the organization ID (which is the environment ID)
            $whoami = Get-DataverseWhoAmI -Connection $connection
            $envId = $whoami.OrganizationId
            
            $url = Get-DataverseMakerPortalUrl -Connection $connection
            
            # Verify URL includes the environment ID
            $url | Should -BeLike "*/environments/$envId/*"
        }

        It "Works with default connection" {
            $connection = getMockConnection
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Call without explicit connection
            $url = Get-DataverseMakerPortalUrl
            
            # Verify URL generated
            $url | Should -Not -BeNullOrEmpty
            $url | Should -BeLike "https://make.powerapps.com/*"
        }

        It "Handles case-insensitive page parameter" {
            $connection = getMockConnection
            
            $url1 = Get-DataverseMakerPortalUrl -Connection $connection -Page "Solutions"
            $url2 = Get-DataverseMakerPortalUrl -Connection $connection -Page "SOLUTIONS"
            $url3 = Get-DataverseMakerPortalUrl -Connection $connection -Page "solutions"
            
            # All should produce the same URL
            $url1 | Should -Be $url2
            $url2 | Should -Be $url3
        }
    }
}
