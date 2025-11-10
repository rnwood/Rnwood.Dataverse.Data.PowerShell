. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseRecord - Advanced Filter Grouping (AND/OR/NOT/XOR)' {
    It "Given grouped filter values using 'and' key, results are filtered accordingly" {
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Rob"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        # Use a grouping hashtable to require firstname=Rob AND lastname=One
        $result = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{"and" = @(@{"firstname" = "Rob"}, @{ "lastname" = "One" }) }
        $result | Should -HaveCount 1
        $result[0].firstname | Should -Be "Rob"
        $result[0].lastname | Should -Be "One"
    }

    It "Given nested grouped filter values with 'or' inside 'and', supports infinite depth" {
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Mary"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        # ( (firstname = Rob OR firstname = Joe) AND lastname = One ) => matches Rob One and Joe One
        $filter = @{
            "and" = @(
                @{ "or" = @(@{ "firstname" = "Rob" }, @{ "firstname" = "Joe" }) },
                @{ "lastname" = "One" }
            )
        }

        $result = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues $filter
        $result | Should -HaveCount 2
        $result.firstname | Should -Be "Rob", "Joe"
    }

    It "Given grouped exclude filter values, exclusions honour grouping semantics" {
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Mary"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        # Exclude where firstname = Rob OR lastname = Two
        $result = Get-DataverseRecord -Connection $connection -TableName contact -ExcludeFilterValues @{"or" = @(@{"firstname" = "Rob"}, @{ "lastname" = "Two" }) }
        $result | Should -HaveCount 1
        $result[0].firstname | Should -Be "Joe"
    }

    It "Given 'not' grouping with single field, results are negated appropriately" {
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Mary"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        # NOT(firstname = Rob) -> should return everyone except Rob
        $result = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ 'not' = @{ firstname = 'Rob' } }
        $result | Should -HaveCount 2
        $result.firstname | Should -Be 'Joe', 'Mary'
    }

    It "Given 'not' grouping with multi-field single hashtable, negates the combined expression" {
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Mary"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        # NOT(firstname = Rob AND lastname = One) -> excludes only Rob One
        $result = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ 'not' = @{ firstname = 'Rob'; lastname = 'One' } }
        $result | Should -HaveCount 2
        $result.firstname | Should -Be 'Joe', 'Mary'
    }

    It "Given 'not' wrapping an 'or', results are negated accordingly (NOT(A OR B) -> NOT A AND NOT B)" {
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Mary"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        # NOT(firstname = Rob OR firstname = Joe) -> only Mary Two remains
        $result = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ 'not' = @{ 'or' = @(@{ firstname = 'Rob' }, @{ firstname = 'Joe' }) } }
        $result | Should -HaveCount 1
        $result[0].firstname | Should -Be 'Mary'
    }

    It "Given 'not' used inside another group, behaves correctly" {
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Mary"; "lastname" = "One" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        # (NOT firstname=Rob) AND lastname=One -> matches Joe One and Mary One
        $result = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ 'and' = @(@{ 'not' = @{ firstname = 'Rob' } }, @{ lastname = 'One' }) }
        $result | Should -HaveCount 2
        $result.firstname | Should -Be 'Joe', 'Mary'
    }

    It "Given 'xor' grouping, matches records where exactly one subfilter is true" {
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Mary"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        # XOR(firstname=Rob, firstname=Joe) -> matches Rob One and Joe One
        $result = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ 'xor' = @(@{ firstname = 'Rob' }, @{ firstname = 'Joe' }) }
        $result | Should -HaveCount 2
        $result.firstname | Should -Be 'Rob', 'Joe'
    }

    It "Given 'xor' used in ExcludeFilterValues, excludes records matching exactly one subfilter" {
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Mary"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        # Exclude XOR(firstname=Rob, firstname=Joe) -> removes Rob One and Joe One -> leaves Mary
        $result = Get-DataverseRecord -Connection $connection -TableName contact -ExcludeFilterValues @{ 'xor' = @(@{ firstname = 'Rob' }, @{ firstname = 'Joe' }) }
        $result | Should -HaveCount 1
        $result[0].firstname | Should -Be 'Mary'
    }

    It "Given 'xor' nested inside other groups, behaves as expected" {
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Mary"; "lastname" = "One" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        # (xor(firstname=Rob, firstname=Joe) AND lastname=One) -> matches Rob One and Joe One but not Mary One
        $result = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ 'and' = @(@{ 'xor' = @(@{ firstname = 'Rob' }, @{ firstname = 'Joe' }) }, @{ lastname = 'One' }) }
        $result | Should -HaveCount 2
        $result.firstname | Should -Be 'Rob', 'Joe'
    }

    It "Given exclude filter values with no operator, results are excluded using equals" {
        
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Rob"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        $result = Get-DataverseRecord -Connection $connection -TableName contact -ExcludeFilterValues @{"firstname" = "Rob" }
        
        $result | SHould -HaveCount 1
        $result.firstname | Should -be "Joe"
    }

    It "Given multiple exclude filter values with no operator, results are excluded using equals using OR semantics" {
        
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "Two" },
        @{"firstname" = "Rob"; "lastname" = "Three" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        $result = Get-DataverseRecord -Connection $connection -TableName contact `
            -ExcludeFilterValues `
                @{"lastname" = "One" },
                @{"lastname" = "Three" }

        $result | Should -HaveCount 1
        $result.firstname | Should -be "Joe"
    }

    It "Given exclude filter values with an operator, results are filtered using that operator" {
        
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Rob"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        $result = Get-DataverseRecord -Connection $connection -TableName contact -ExcludeFilterValues @{"firstname" = @{operator="Equal"; value="Rob"} }
        
        $result | SHould -HaveCount 1
        $result.firstname | Should -be "Joe"
    }
}
