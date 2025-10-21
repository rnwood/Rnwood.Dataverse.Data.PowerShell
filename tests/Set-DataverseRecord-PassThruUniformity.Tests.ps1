Describe 'Set-DataverseRecord -PassThru Uniformity' {

    . $PSScriptRoot/Common.ps1

    Context 'PassThru always returns PSObject' {
        It "Returns PSObject when input is a hashtable" {
            $connection = getMockConnection
            
            # Input is a hashtable
            $hashtableInput = @{
                firstname = "John"
                lastname = "Doe"
                emailaddress1 = "john@example.com"
            }
            
            $result = $hashtableInput | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify result is a PSObject (not a hashtable)
            $result | Should -Not -BeNullOrEmpty
            $result.GetType().FullName | Should -Be 'System.Management.Automation.PSCustomObject'
            $result.Id | Should -BeOfType [Guid]
            $result.Id | Should -Not -Be ([Guid]::Empty)
            $result.firstname | Should -Be "John"
            $result.lastname | Should -Be "Doe"
            $result.emailaddress1 | Should -Be "john@example.com"
        }

        It "Returns PSObject when input is an SDK Entity object" {
            $connection = getMockConnection
            
            # Input is an SDK Entity object
            $entityInput = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $entityInput["firstname"] = "Jane"
            $entityInput["lastname"] = "Smith"
            $entityInput["emailaddress1"] = "jane@example.com"
            
            $result = $entityInput | Set-DataverseRecord -Connection $connection -CreateOnly -PassThru
            
            # Verify result is a PSObject (not an Entity)
            $result | Should -Not -BeNullOrEmpty
            $result.GetType().FullName | Should -Be 'System.Management.Automation.PSCustomObject'
            $result.Id | Should -BeOfType [Guid]
            $result.Id | Should -Not -Be ([Guid]::Empty)
            $result.TableName | Should -Be "contact"
            $result.firstname | Should -Be "Jane"
            $result.lastname | Should -Be "Smith"
            $result.emailaddress1 | Should -Be "jane@example.com"
        }

        It "Returns PSObject when input is already a PSObject" {
            $connection = getMockConnection
            
            # Input is a PSObject
            $psObjectInput = [PSCustomObject]@{
                TableName = "contact"
                firstname = "Bob"
                lastname = "Johnson"
                emailaddress1 = "bob@example.com"
            }
            
            $result = $psObjectInput | Set-DataverseRecord -Connection $connection -CreateOnly -PassThru
            
            # Verify result is a PSObject
            $result | Should -Not -BeNullOrEmpty
            $result.GetType().FullName | Should -Be 'System.Management.Automation.PSCustomObject'
            $result.Id | Should -BeOfType [Guid]
            $result.Id | Should -Not -Be ([Guid]::Empty)
            $result.TableName | Should -Be "contact"
            $result.firstname | Should -Be "Bob"
            $result.lastname | Should -Be "Johnson"
            $result.emailaddress1 | Should -Be "bob@example.com"
        }

        It "Returns PSObject for updates with hashtable input" {
            $connection = getMockConnection
            
            # Create a record first
            $initialRecord = @{
                firstname = "Original"
                lastname = "Name"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update using hashtable - must explicitly provide TableName parameter
            $updateHash = @{
                firstname = "Updated"
            }
            
            $result = $updateHash | Set-DataverseRecord -Connection $connection -TableName contact -Id $initialRecord.Id -PassThru
            
            # Verify result is a PSObject
            $result | Should -Not -BeNullOrEmpty
            $result.GetType().FullName | Should -Be 'System.Management.Automation.PSCustomObject'
            $result.Id | Should -Be $initialRecord.Id
            $result.firstname | Should -Be "Updated"
        }

        It "Returns PSObject for updates with Entity input" {
            $connection = getMockConnection
            
            # Create a record first
            $initialRecord = @{
                firstname = "Original"
                lastname = "Name"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update using Entity object
            $updateEntity = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $updateEntity.Id = $updateEntity["contactid"] = $initialRecord.Id
            $updateEntity["firstname"] = "UpdatedViaEntity"
            
            $result = $updateEntity | Set-DataverseRecord -Connection $connection -PassThru
            
            # Verify result is a PSObject
            $result | Should -Not -BeNullOrEmpty
            $result.GetType().FullName | Should -Be 'System.Management.Automation.PSCustomObject'
            $result.Id | Should -Be $initialRecord.Id
            $result.TableName | Should -Be "contact"
            $result.firstname | Should -Be "UpdatedViaEntity"
        }

        It "Returns PSObject with all input properties preserved (hashtable)" {
            $connection = getMockConnection
            
            $hashtableInput = @{
                firstname = "Test"
                lastname = "User"
                emailaddress1 = "test@example.com"
                telephone1 = "555-1234"
                description = "Test description"
            }
            
            $result = $hashtableInput | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # All properties should be preserved
            $result.firstname | Should -Be "Test"
            $result.lastname | Should -Be "User"
            $result.emailaddress1 | Should -Be "test@example.com"
            $result.telephone1 | Should -Be "555-1234"
            $result.description | Should -Be "Test description"
            $result.Id | Should -BeOfType [Guid]
        }

        It "Returns PSObject batch operations with mixed input types" {
            $connection = getMockConnection
            
            $inputs = @(
                @{ firstname = "Hash1"; lastname = "User1" },
                [PSCustomObject]@{ TableName = "contact"; firstname = "PS1"; lastname = "User2" }
            )
            
            $results = $inputs | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            $results | Should -HaveCount 2
            $results | ForEach-Object {
                $_.GetType().FullName | Should -Be 'System.Management.Automation.PSCustomObject'
                $_.Id | Should -BeOfType [Guid]
            }
        }

        It "Returns PSObject when no update needed (no changes)" {
            $connection = getMockConnection
            
            # Create a record
            $initial = @{
                firstname = "NoChange"
                lastname = "User"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Try to update with same values (should be skipped) - must explicitly provide TableName parameter
            $noChangeInput = @{
                firstname = "NoChange"
                lastname = "User"
            }
            
            $result = $noChangeInput | Set-DataverseRecord -Connection $connection -TableName contact -Id $initial.Id -PassThru
            
            # Should still return PSObject even though nothing changed
            $result | Should -Not -BeNullOrEmpty
            $result.GetType().FullName | Should -Be 'System.Management.Automation.PSCustomObject'
            $result.Id | Should -Be $initial.Id
            $result.firstname | Should -Be "NoChange"
            $result.lastname | Should -Be "User"
        }
    }
}
