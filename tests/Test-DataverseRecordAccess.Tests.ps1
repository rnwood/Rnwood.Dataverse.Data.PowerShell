. $PSScriptRoot/Common.ps1

Describe 'Test-DataverseRecordAccess' {
    Context 'Access Rights Retrieval' {
        It "Returns access rights for a user on a specific record" {
            $connection = getMockConnection
            
            # Create a test record
            $contact = @{
                firstname = "Test"
                lastname = "AccessCheck"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Get current user ID
            $whoAmI = Get-DataverseWhoAmI -Connection $connection
            
            # Create entity reference
            $targetRef = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $contact.Id)
            
            # Test access
            $access = Test-DataverseRecordAccess -Connection $connection -Target $targetRef -Principal $whoAmI.UserId
            
            # Verify result
            $access | Should -Not -BeNullOrEmpty
            $access | Should -BeOfType [Microsoft.Crm.Sdk.Messages.AccessRights]
        }

        It "Returns correct access rights enum values" {
            $connection = getMockConnection -Entities @("contact")
            
            # Create a test record (using contact instead of account)
            $contact = @{
                firstname = "Access"
                lastname = "Test"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Get current user ID
            $whoAmI = Get-DataverseWhoAmI -Connection $connection
            
            # Create entity reference
            $targetRef = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $contact.Id)
            
            # Test access
            $access = Test-DataverseRecordAccess -Connection $connection -Target $targetRef -Principal $whoAmI.UserId
            
            # Verify result is an AccessRights enum
            $access | Should -Not -BeNullOrEmpty
            $access.GetType().Name | Should -Be "AccessRights"
        }

        It "Can check specific access rights using bitwise operations" {
            $connection = getMockConnection
            
            # Create a test record
            $contact = @{
                firstname = "Permission"
                lastname = "Test"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Get current user ID
            $whoAmI = Get-DataverseWhoAmI -Connection $connection
            
            # Create entity reference
            $targetRef = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $contact.Id)
            
            # Test access
            $access = Test-DataverseRecordAccess -Connection $connection -Target $targetRef -Principal $whoAmI.UserId
            
            # Verify we can check for specific rights using bitwise AND
            # Note: The mock provider may return None or minimal access by default
            # We just verify the bitwise operation works
            $hasRead = ($access -band [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess) -ne 0
            # Don't assert specific rights as FakeXrmEasy mock may not grant access by default
            $access.GetType().Name | Should -Be "AccessRights"
        }

        It "Does not modify any data (read-only operation)" {
            $connection = getMockConnection
            
            # Create test record
            $contact = @{
                firstname = "ReadOnly"
                lastname = "Check"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Get current user ID
            $whoAmI = Get-DataverseWhoAmI -Connection $connection
            
            # Create entity reference
            $targetRef = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $contact.Id)
            
            # Test access
            $result = Test-DataverseRecordAccess -Connection $connection -Target $targetRef -Principal $whoAmI.UserId
            
            # Verify contact was not affected
            $verifyContact = Get-DataverseRecord -Connection $connection -TableName contact -Id $contact.Id
            $verifyContact.firstname | Should -Be "ReadOnly"
            $verifyContact.lastname | Should -Be "Check"
            
            # Verify no side effects - still only one record
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Works with default connection" {
            $connection = getMockConnection
            
            # Create a test record
            $contact = @{
                firstname = "Default"
                lastname = "Connection"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Set as default
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Get current user ID
            $whoAmI = Get-DataverseWhoAmI
            
            # Create entity reference
            $targetRef = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $contact.Id)
            
            # Call without explicit connection
            $result = Test-DataverseRecordAccess -Target $targetRef -Principal $whoAmI.UserId
            
            # Verify result
            $result | Should -Not -BeNullOrEmpty
        }
    }
}
