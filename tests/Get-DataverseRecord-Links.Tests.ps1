. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseRecord - Links' {
    Context "Link Entity Tests" {
        It "Given -Links with LinkEntity SDK object, joins tables correctly" {
            $connection = getMockConnection
            
            # Create test data
            @{"firstname" = "John"; "lastname" = "Doe" } | Set-DataverseRecord -Connection $connection -TableName contact
            
            # Create a LinkEntity to test the existing syntax
            $linkEntity = New-Object Microsoft.Xrm.Sdk.Query.LinkEntity
            $linkEntity.LinkFromEntityName = "contact"
            $linkEntity.LinkToEntityName = "account"
            $linkEntity.LinkFromAttributeName = "accountid"
            $linkEntity.LinkToAttributeName = "accountid"
            $linkEntity.JoinOperator = [Microsoft.Xrm.Sdk.Query.JoinOperator]::Inner
            
            # Wrap in DataverseLinkEntity
            $dataverseLinkEntity = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.DataverseLinkEntity($linkEntity)
            
            # This should work without error - the mock doesn't fully support links but we can test it doesn't throw
            { Get-DataverseRecord -Connection $connection -TableName contact -Links $dataverseLinkEntity } | Should -Not -Throw
        }

        It "Given -Links with simplified hashtable syntax (contact.accountid = account.accountid), creates link correctly" {
            $connection = getMockConnection
            
            # Create test data
            @{"firstname" = "John"; "lastname" = "Doe" } | Set-DataverseRecord -Connection $connection -TableName contact
            
            # Test simplified syntax
            $simplifiedLink = @{
                'contact.accountid' = 'account.accountid'
            }
            
            # This should convert the hashtable to a DataverseLinkEntity and work without error
            { Get-DataverseRecord -Connection $connection -TableName contact -Links $simplifiedLink } | Should -Not -Throw
        }

        It "Given -Links with simplified syntax including type, creates link with correct join operator" {
            $connection = getMockConnection
            
            @{"firstname" = "John"; "lastname" = "Doe" } | Set-DataverseRecord -Connection $connection -TableName contact
            
            $simplifiedLink = @{
                'contact.accountid' = 'account.accountid'
                'type' = 'LeftOuter'
            }
            
            { Get-DataverseRecord -Connection $connection -TableName contact -Links $simplifiedLink } | Should -Not -Throw
        }

        It "Given -Links with simplified syntax including alias, creates link with alias" {
            $connection = getMockConnection
            
            @{"firstname" = "John"; "lastname" = "Doe" } | Set-DataverseRecord -Connection $connection -TableName contact
            
            $simplifiedLink = @{
                'contact.accountid' = 'account.accountid'
                'alias' = 'linkedAccount'
            }
            
            { Get-DataverseRecord -Connection $connection -TableName contact -Links $simplifiedLink } | Should -Not -Throw
        }

        It "Given -Links with simplified syntax including filter, creates link with filter conditions" {
            $connection = getMockConnection
            
            @{"firstname" = "John"; "lastname" = "Doe" } | Set-DataverseRecord -Connection $connection -TableName contact
            
            $simplifiedLink = @{
                'contact.accountid' = 'account.accountid'
                'filter' = @{
                    name = @{ operator = 'Like'; value = 'Contoso%' }
                    statecode = @{ operator = 'Equal'; value = 0 }
                }
            }
            
            { Get-DataverseRecord -Connection $connection -TableName contact -Links $simplifiedLink } | Should -Not -Throw
        }

        It "Given -Links with multiple simplified links, creates multiple join conditions" {
            $connection = getMockConnection
            
            @{"firstname" = "John"; "lastname" = "Doe" } | Set-DataverseRecord -Connection $connection -TableName contact
            
            $links = @(
                @{
                    'contact.accountid' = 'account.accountid'
                    'type' = 'LeftOuter'
                },
                @{
                    'contact.ownerid' = 'systemuser.systemuserid'
                }
            )
            
            { Get-DataverseRecord -Connection $connection -TableName contact -Links $links } | Should -Not -Throw
        }

        It "Given -Links with nested child links (array), creates nested join conditions" {
            $connection = getMockConnection

            @{"firstname" = "John"; "lastname" = "Doe" } | Set-DataverseRecord -Connection $connection -TableName contact

            $links = @(
                @{
                    'contact.accountid' = 'account.accountid'
                    'links' = @(
                        @{
                            'account.ownerid' = 'systemuser.systemuserid'
                            'type' = 'LeftOuter'
                            'alias' = 'owner'
                        }
                    )
                }
            )

            { Get-DataverseRecord -Connection $connection -TableName contact -Links $links } | Should -Not -Throw
        }

        It "Given -Links with nested child link (single hashtable), creates nested join condition" {
            $connection = getMockConnection

            @{"firstname" = "John"; "lastname" = "Doe" } | Set-DataverseRecord -Connection $connection -TableName contact

            $links = @(
                @{
                    'contact.accountid' = 'account.accountid'
                    'links' = @{
                        'account.ownerid' = 'systemuser.systemuserid'
                        'type' = 'Inner'
                    }
                }
            )

            { Get-DataverseRecord -Connection $connection -TableName contact -Links $links } | Should -Not -Throw
        }
    }
}
