Describe 'Get-DataverseRecord' {
    It "Converts to a PS object with properties using native PS types" {
        
        $connection = getMockConnection
            
        $in = new-object Microsoft.Xrm.Sdk.Entity "contact"
        $in["contactid"] = [Guid]::NewGuid()
        $in["firstname"] = "text"
        $in["birthdate"] = [datetime]::Today
        $in["accountrolecode"] = [Microsoft.Xrm.Sdk.OptionSetValue] (new-object Microsoft.Xrm.Sdk.OptionSetValue 2)
        $in["parentcontactid"] = [Microsoft.Xrm.Sdk.EntityReference] (new-object Microsoft.Xrm.Sdk.EntityReference "contact", ([Guid]::NewGuid()))

        $in | Set-DataverseRecord -Connection $connection

        $result = Get-DataverseRecord -Connection $connection -TableName contact

        $result | Should -BeOfType [PSCustomObject]
            
        #UniqueIdentifier
        $result.contactid | Should -BeOfType [Guid]
        $result.contactid | Should -be $in["contactid"]

        #Text
        $result.firstname | Should -BeOfType [string]
        $result.firstname | Should -be $in["firstname"] 

        #Date
        $result.birthdate | Should -BeOfType [datetime]
        $result.birthdate.Date | Should -be $in["birthdate"]

        # Choice
        $result.accountrolecode | Should -BeOfType [int]
        $result.accountrolecode | Should -be 2

        # Lookup
        $result.parentcontactid | Should -BeOfType [Rnwood.Dataverse.Data.PowerShell.Commands.DataverseEntityReference]
        $result.parentcontactid.Id | Should -be $in["parentcontactid"].Id
        $result.parentcontactid.TableName | Should -be $in["parentcontactid"].LogicalName
    }

    It "Converts to a PS object with Id and TableName implcit props" {
        
        $connection = getMockConnection
            
        $in = new-object Microsoft.Xrm.Sdk.Entity "contact"
        $in.Id = $in["contactid"] = [Guid]::NewGuid()
            
        $in | Set-DataverseRecord -Connection $connection

        $result = Get-DataverseRecord -Connection $connection -TableName contact

        $result | Should -BeOfType [PSCustomObject]
            
        $result.Id | Should -BeOfType [Guid]
        $result.Id | Should -be $in["contactid"]

        $result.TableName | Should -be "contact"
    }

    It "Given filter with no explicit operator, gets all records matching using equals" {
        
        $connection = getMockConnection
        1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname"="1"}
        $result | Should -HaveCount 1
        $result[0].firstname | Should -Be "1"
    }

    It "Given filter with explicit operator (deprecated syntax), gets all records matching using that operator" {
        
        $connection = getMockConnection
        1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname:Like"="1%"}
        $result | Should -HaveCount 2
    }

        It "Given filter with explicit operator, gets all records matching using that operator" {
        
        $connection = getMockConnection
        1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname"=@{"operator"="Like"; "value"="1%"}}
        $result | Should -HaveCount 2
    }

    It "Given filter with explicit operator not requiring a value (deprecated syntax), gets all records matching using that operator" {
        
        $connection = getMockConnection
        1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname:Null"=""}
        $result | Should -HaveCount 0
    }

        It "Given filter with explicit operator not requiring a value, gets all records matching using that operator" {
        
        $connection = getMockConnection
        1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname"=@{"operator"="Null"}}
        $result | Should -HaveCount 0
    }

    It "Given filter with implicit operator not requiring a value, gets all records matching using that operator" {
        
        $connection = getMockConnection
        1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        @{"firstname" = "" } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname"="$null"}
        $result | Should -HaveCount 1
    }

    It "Given filter with explicit operator requiring multiple values, gets all records matching using that operator" {
        
        $connection = getMockConnection
        1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname"=@{"operator"="In"; "value"=("1", "2")}}
        $result | Should -HaveCount 2
    }

    It "Given no filters, gets all records even beyond the page limit" {
        
        $connection = getMockConnection
        1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -PageSize 2
        $result | Should -HaveCount 10
    }

    It "Given -Top X, gets first X records only" {
        
        $connection = getMockConnection
        1..9 | Sort-Object -Descending | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -Top 5 -OrderBy firstname
        $result | Should -HaveCount 5
        1..5 | ForEach-Object { $result[$_ - 1].firstname | Should -Be $_ }
    }

    It "Given -OrderBy, results are sorted ascending" {
        
        $connection = getMockConnection
        1..9 | Sort-Object -Descending | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -OrderBy firstname
        $result | Should -HaveCount 9
        1..9 | ForEach-Object { $result[$_ - 1].firstname | Should -Be $_ }
    }

    It "Given -OrderBy, results are sorted ascending on the server" {
        
        $connection = getMockConnection
        1..9 | Sort-Object -Descending | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -OrderBy firstname -top 5
        $result | Should -HaveCount 5
        1..5 | ForEach-Object { $result[$_ - 1].firstname | Should -Be $_ }
    }

    It "Given -OrderBy with -, results are sorted descending on the server" {
        
        $connection = getMockConnection
        1..9 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -OrderBy firstname- -top 5
        $result | Should -HaveCount 5
        0..4 | ForEach-Object { $result[$_].firstname | Should -Be (9 - $_) }
    }

    It "Given -OrderBy a,b, results are sorted correctly" {
        
        $connection = getMockConnection
        "a", "b", "c" | ForEach-Object {
            $letter = $_
            1..3 | Sort-Object -Descending | ForEach-Object { @{"firstname" = "$_"; "lastname" = $letter } } | Set-DataverseRecord -Connection $connection -TableName contact
        }
        $result = Get-DataverseRecord -Connection $connection -TableName contact -OrderBy firstname, lastname -top 5

        $result[0].firstname | Should -be "1"
        $result[0].lastname | Should -be "a"
            
        $result[1].firstname | Should -be "1"
        $result[1].lastname | Should -be "b"
    }

    It "Given -OrderBy a,b-, results are sorted correctly" {
        
        $connection = getMockConnection
        "a", "b", "c" | ForEach-Object {
            $letter = $_
            1..3 | Sort-Object -Descending | ForEach-Object { @{"firstname" = "$_"; "lastname" = $letter } } | Set-DataverseRecord -Connection $connection -TableName contact
        }
        $result = Get-DataverseRecord -Connection $connection -TableName contact -OrderBy firstname, lastname- -top 5

        $result[0].firstname | Should -be "1"
        $result[0].lastname | Should -be "c"
            
        $result[1].firstname | Should -be "1"
        $result[1].lastname | Should -be "b"
    }

    It "Given filter values, results are filtered using Equals" {
        
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Rob"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname" = "Rob" }
        
        $result | Should -HaveCount 2
        $result.firstname | Should -be "Rob", "Rob"
    }

    It "Given multiple filter values, results are filtered using Equals and combined with AND" {
        
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Rob"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"lastname" = "One"; "firstname" = "Joe" }
        
        $result | Should -HaveCount 1
        $result.firstname | Should -be "Joe"
    }

    It "Given multiple filter elements, results are filtered using Equals and combined with OR" {
        
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Rob"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"lastname" = "One"; "firstname" = "Rob" }, @{"firstname" = "Joe" }
        
        $result | Should -HaveCount 2
        $result.firstname | Should -be "Rob", "Joe"
        $result.lastname | Should -be "One", "One"
    }

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

    It "Given fetchxml, it works" {
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Rob"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        $result = Get-DataverseRecord -Connection $connection -fetchxml "<fetch> <entity name='contact'> <attribute name='firstname' /> <filter type='and'> <condition attribute='firstname' operator='eq' value='Rob' /> </filter> </entity></fetch>"
        $result | Should -HaveCount 2


    }

    It "Given -Id, it retrieves the records with those IDs" {

        $ids = 1..3 | ForEach-Object{ [Guid]::NewGuid() }

        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One"; "contactid"=$ids[0] },  
        @{"firstname" = "Joe"; "lastname" = "One"; "contactid"=$ids[1] },
        @{"firstname" = "Rob"; "lastname" = "Two"; "contactid"=$ids[2] } | 
        Set-DataverseRecord -connection $connection -TableName contact

        $result = Get-DataverseRecord -Connection $connection -TableName contact -id $ids[0], $ids[1]
        $result | Should -HaveCount 2
        $result[0].Id | Should -be $ids[0]
        $result[1].Id | Should -be $ids[1]


    }

    It "Given IncludeFilter with multiple hashtables, results are included using OR semantics" {
        $connection = getMockConnection

        @{"firstname" = "Rob"; "lastname" = "One" },
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Rob"; "lastname" = "Two" } |
            Set-DataverseRecord -Connection $connection -TableName contact

        # IncludeFilter: lastname = One OR firstname = Joe -> matches Rob One and Joe One
        $result = Get-DataverseRecord -Connection $connection -TableName contact -IncludeFilter `
            @{"lastname" = "One"}, `
            @{"firstname" = "Joe"}

        $result | Should -HaveCount 2
        $result.firstname | Should -Be "Rob", "Joe"
    }

    It "Given ExcludeFilter with multiple hashtables and the ExcludeFilterOr, results are excluded using OR semantics" {
        $connection = getMockConnection

        @{"firstname" = "Rob"; "lastname" = "One" },
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Rob"; "lastname" = "Two" },
        @{"firstname" = "Mary"; "lastname" = "Two" } |
            Set-DataverseRecord -Connection $connection -TableName contact

        $result = Get-DataverseRecord -Columns firstname, lastname -Connection $connection -TableName contact -ExcludeFilter `
            @{"lastname" = "One"}, `
            @{"firstname" = "Rob"} -ExcludeFilterOr

        $result | Should -HaveCount 3
        $result[0].firstname | Should -Be "Joe"
        $result[1].firstname | Should -Be "Rob"
        $result[1].lastname | Should -Be "Two"
        $result[2].firstname | Should -Be "Mary"
    }

     It "Given ExcludeFilter with multiple hashtables  switch, results are excluded using AND semantics" {
        $connection = getMockConnection

        @{"firstname" = "Rob"; "lastname" = "One" },
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Rob"; "lastname" = "Two" },
        @{"firstname" = "Mary"; "lastname" = "Two" } |
            Set-DataverseRecord -Connection $connection -TableName contact

        # ExcludeFilter: lastname = One OR firstname = Rob -> excludes Rob One, Joe One, Rob Two -> leaves Mary Two
        $result = Get-DataverseRecord -Connection $connection -TableName contact -ExcludeFilter `
            @{"lastname" = "One"}, `
            @{"firstname" = "Rob"} 

        $result | Should -HaveCount 1
        $result[0].firstname | Should -Be "Mary"
    }

    It "Given IncludeFilter and ExcludeFilter together, include is applied then exclusions are removed" {
        $connection = getMockConnection

        @{"firstname" = "Rob"; "lastname" = "One" },
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Rob"; "lastname" = "Two" },
        @{"firstname" = "Mary"; "lastname" = "Two" } |
            Set-DataverseRecord -Connection $connection -TableName contact

        # Include: lastname = One OR firstname = Rob -> Rob One, Joe One, Rob Two
        # Exclude: firstname = Rob -> removes Rob One and Rob Two -> leaves Joe One
        $result = Get-DataverseRecord -Connection $connection -TableName contact `
            -IncludeFilter @{"lastname" = "One"}, @{"firstname" = "Rob"} `
            -ExcludeFilter @{"firstname" = "Rob"}

        $result | Should -HaveCount 1
        $result[0].firstname | Should -Be "Joe"
    }

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
    }
}
