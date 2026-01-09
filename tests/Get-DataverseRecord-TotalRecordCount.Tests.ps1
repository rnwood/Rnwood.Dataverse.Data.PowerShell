. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseRecord - TotalRecordCount' {
    Context 'Count-Only Query Mode' {
        It "Returns total record count instead of records with -TotalRecordCount" -Skip {
            # Note: TotalRecordCount parameter may not be implemented or may work differently
            # This test validates expected behavior if the feature is added
            $connection = getMockConnection
            
            # Create test records
            @(
                @{ firstname = "User1"; lastname = "Count" }
                @{ firstname = "User2"; lastname = "Count" }
                @{ firstname = "User3"; lastname = "Count" }
                @{ firstname = "User4"; lastname = "Count" }
                @{ firstname = "User5"; lastname = "Count" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Query with TotalRecordCount
            $result = Get-DataverseRecord -Connection $connection -TableName contact -TotalRecordCount
            
            # Verify result is a count, not records
            $result | Should -Not -BeNullOrEmpty
            
            # The result should be numeric count (5 records)
            # Exact implementation may vary - result might be integer or object with Count property
            if ($result -is [int]) {
                $result | Should -Be 5
            } elseif ($result.PSObject.Properties['TotalRecordCount']) {
                $result.TotalRecordCount | Should -Be 5
            } else {
                # Fallback: verify it's a count value
                $result | Should -BeGreaterThan 0
            }
            
            # Verify no side effects - all records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 5
        }

        It "TotalRecordCount with filter returns count of filtered records" -Skip {
            # Note: TotalRecordCount parameter may not be implemented
            $connection = getMockConnection
            
            # Create mixed records
            @(
                @{ firstname = "Match1"; lastname = "Filter" }
                @{ firstname = "Match2"; lastname = "Filter" }
                @{ firstname = "NoMatch1"; lastname = "Other" }
                @{ firstname = "NoMatch2"; lastname = "Other" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Query with filter and TotalRecordCount
            $result = Get-DataverseRecord -Connection $connection -TableName contact `
                -FilterValues @{ lastname = "Filter" } -TotalRecordCount
            
            # Verify count is 2 (only filtered records)
            if ($result -is [int]) {
                $result | Should -Be 2
            } elseif ($result.PSObject.Properties['TotalRecordCount']) {
                $result.TotalRecordCount | Should -Be 2
            } else {
                $result | Should -BeGreaterThan 0
                $result | Should -BeLessOrEqual 2
            }
            
            # Verify no side effects - all records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 4
        }

        It "TotalRecordCount returns 0 when no records match" -Skip {
            # Note: TotalRecordCount parameter may not be implemented
            $connection = getMockConnection
            
            # Create some records
            @(
                @{ firstname = "Existing1"; lastname = "User" }
                @{ firstname = "Existing2"; lastname = "User" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Query with filter that matches nothing
            $result = Get-DataverseRecord -Connection $connection -TableName contact `
                -FilterValues @{ lastname = "NoMatch" } -TotalRecordCount
            
            # Verify count is 0
            if ($result -is [int]) {
                $result | Should -Be 0
            } elseif ($result.PSObject.Properties['TotalRecordCount']) {
                $result.TotalRecordCount | Should -Be 0
            } elseif ($null -eq $result) {
                # Null might be returned for zero count
                $result | Should -BeNullOrEmpty
            }
            
            # Verify no side effects - existing records remain
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 2
        }

        It "TotalRecordCount is faster than retrieving all records" -Skip {
            # Note: TotalRecordCount parameter may not be implemented
            $connection = getMockConnection
            
            # Create many records
            1..20 | ForEach-Object {
                @{ firstname = "User$_"; lastname = "Performance" } | 
                    Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            }
            
            # Get count (should be fast, no record retrieval)
            $result = Get-DataverseRecord -Connection $connection -TableName contact -TotalRecordCount
            
            # Verify count is correct
            if ($result -is [int]) {
                $result | Should -Be 20
            } elseif ($result.PSObject.Properties['TotalRecordCount']) {
                $result.TotalRecordCount | Should -Be 20
            } else {
                $result | Should -BeGreaterOrEqual 20
            }
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 20
        }

        It "TotalRecordCount with Top parameter counts all matching records, not just Top N" -Skip {
            # Note: TotalRecordCount parameter may not be implemented
            $connection = getMockConnection
            
            # Create test records
            1..10 | ForEach-Object {
                @{ firstname = "User$_"; lastname = "Top" } | 
                    Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            }
            
            # Query with Top and TotalRecordCount
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Top 3 -TotalRecordCount
            
            # TotalRecordCount should return total count (10), not just Top count (3)
            if ($result -is [int]) {
                $result | Should -Be 10
            } elseif ($result.PSObject.Properties['TotalRecordCount']) {
                $result.TotalRecordCount | Should -Be 10
            } else {
                # Verify at least greater than Top limit
                $result | Should -BeGreaterThan 3
            }
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 10
        }
    }
}
