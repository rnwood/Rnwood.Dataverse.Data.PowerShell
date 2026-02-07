. $PSScriptRoot/Common.ps1

Describe 'Remove-DataverseRecord - WhatIf and Confirm Support' {
    Context 'WhatIf Support' {
        It "Delete with -WhatIf does not delete records" {
            $connection = getMockConnection
            
            # Create test records
            $records = @(
                @{ firstname = "WhatIf1"; lastname = "Test" }
                @{ firstname = "WhatIf2"; lastname = "Test" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Delete with WhatIf
            Remove-DataverseRecord -Connection $connection -TableName contact -Id $records[0].Id -WhatIf
            
            # Verify NO records were deleted
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 2
            ($allContacts | Where-Object { $_.Id -eq $records[0].Id }) | Should -HaveCount 1
        }

        It "Pipeline delete with -WhatIf does not delete records" {
            $connection = getMockConnection
            
            # Create test records
            $records = @(
                @{ firstname = "Pipe1"; lastname = "WhatIf" }
                @{ firstname = "Pipe2"; lastname = "WhatIf" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Pipeline delete with WhatIf - TableName is required when piping PSObjects
            $records | Remove-DataverseRecord -Connection $connection -TableName contact -WhatIf
            
            # Verify NO records were deleted
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 2
        }

        It "Batch delete with -WhatIf does not delete any records" {
            $connection = getMockConnection
            
            # Create test records
            $records = @(
                @{ firstname = "Batch1"; lastname = "WhatIf" }
                @{ firstname = "Batch2"; lastname = "WhatIf" }
                @{ firstname = "Batch3"; lastname = "WhatIf" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Batch delete with WhatIf - iterate over IDs
            $records.Id | ForEach-Object {
                Remove-DataverseRecord -Connection $connection -TableName contact -Id $_ -WhatIf
            }
            
            # Verify NO records were deleted
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 3
        }
    }

    Context 'Confirm Support' {
        It "Delete with -Confirm:`$false deletes records" {
            $connection = getMockConnection
            
            # Create test record
            $record = @{
                firstname = "Confirm"
                lastname = "False"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Delete with Confirm:$false (should delete without prompting)
            Remove-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Confirm:$false
            
            # Verify record WAS deleted
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns contactid
            $result | Should -BeNullOrEmpty
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -BeNullOrEmpty
        }
    }

    Context 'Combined WhatIf and Confirm Scenarios' {
        It "WhatIf overrides Confirm:`$false (no deletion)" {
            $connection = getMockConnection
            
            # Create test record
            $record = @{
                firstname = "Both"
                lastname = "Flags"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Delete with both WhatIf and Confirm:$false (WhatIf takes precedence)
            Remove-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -WhatIf -Confirm:$false
            
            # Verify record was NOT deleted (WhatIf prevents deletion)
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns contactid
            $result | Should -Not -BeNullOrEmpty
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }
    }
}
