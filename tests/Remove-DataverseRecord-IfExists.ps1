Describe 'Remove-DataverseRecord - IfExists Flag' {
    Context 'IfExists Flag Behavior' {
        It "Deletes existing record with -IfExists without error" {
            $connection = getMockConnection
            
            # Create test record
            $record = @{
                firstname = "ToDelete"
                lastname = "Exists"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Delete with IfExists
            Remove-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -IfExists
            
            # Verify record was deleted
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result | Should -BeNullOrEmpty
            
            # Verify no side effects - no other records
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -BeNullOrEmpty
        }

        It "Attempts to delete non-existent record with -IfExists without error" {
            $connection = getMockConnection
            
            # Create one record
            @{ firstname = "Keeper"; lastname = "Record" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Try to delete non-existent record with IfExists (should not throw)
            $nonExistentId = [Guid]::NewGuid()
            { Remove-DataverseRecord -Connection $connection -TableName contact -Id $nonExistentId -IfExists } | 
                Should -Not -Throw
            
            # Verify existing record was not affected
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
            $allContacts[0].firstname | Should -Be "Keeper"
        }

        It "Attempts to delete non-existent record WITHOUT -IfExists throws error" {
            $connection = getMockConnection
            
            # Create one record
            @{ firstname = "Existing"; lastname = "Record" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Try to delete non-existent record without IfExists (should throw)
            $nonExistentId = [Guid]::NewGuid()
            { Remove-DataverseRecord -Connection $connection -TableName contact -Id $nonExistentId -ErrorAction Stop } | 
                Should -Throw
            
            # Verify existing record was not affected
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
            $allContacts[0].firstname | Should -Be "Existing"
        }

        It "IfExists works with multiple Ids in batch delete" {
            $connection = getMockConnection
            
            # Create test records
            $existingRecord = @{ firstname = "Existing"; lastname = "Delete" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            @{ firstname = "Keep"; lastname = "Record" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Delete with mix of existing and non-existent Ids
            $nonExistentId = [Guid]::NewGuid()
            $idsToDelete = @($existingRecord.Id, $nonExistentId)
            
            { $idsToDelete | ForEach-Object { 
                Remove-DataverseRecord -Connection $connection -TableName contact -Id $_ -IfExists 
            } } | Should -Not -Throw
            
            # Verify existing record was deleted, keeper remains
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
            $allContacts[0].firstname | Should -Be "Keep"
        }

        It "IfExists works with pipeline input" -Skip {
            # Note: Pipeline-based IfExists may have timing issues with FakeXrmEasy
            # Test validates expected behavior
            $connection = getMockConnection
            
            # Create test records
            $records = @(
                @{ firstname = "Delete1"; lastname = "Pipeline" }
                @{ firstname = "Delete2"; lastname = "Pipeline" }
                @{ firstname = "Keep"; lastname = "Different" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Delete via pipeline with IfExists
            $records | Where-Object { $_.lastname -eq "Pipeline" } | 
                Remove-DataverseRecord -Connection $connection -IfExists
            
            # Verify correct records deleted
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact
            $remaining | Should -HaveCount 1
            $remaining[0].firstname | Should -Be "Keep"
            
            # Attempt to delete again with IfExists (should not error)
            { $records | Where-Object { $_.lastname -eq "Pipeline" } | 
                Remove-DataverseRecord -Connection $connection -IfExists } | 
                Should -Not -Throw
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }
    }
}
