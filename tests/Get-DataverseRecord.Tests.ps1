Describe 'Get-DataverseRecord' {

    . $PSScriptRoot/Common.ps1

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
        $result.birthdate | Should -be $in["birthdate"]

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

    It "Given filter with explicit operator, gets all records matching using that operator" {
        
        $connection = getMockConnection
        1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname:Like"="1%"}
        $result | Should -HaveCount 2
    }

    It "Given filter with explicit operator not requiring a value, gets all records matching using that operator" {
        
        $connection = getMockConnection
        1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname:Null"=""}
        $result | Should -HaveCount 0
    }

    It "Given filter with implicit operator not requiring a value, gets all records matching using that operator" {
        
        $connection = getMockConnection
        1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        @{"firstname" = $null } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname"="$null"}
        $result | Should -HaveCount 1
    }

    It "Given filter with explicit operator requiring multiple values, gets all records matching using that operator" {
        
        $connection = getMockConnection
        1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
        $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname:In"=("1", "2")}
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

    It "Given exclude filter values, results are filtered using not qquals" {
        
        $connection = getMockConnection
        @{"firstname" = "Rob"; "lastname" = "One" },  
        @{"firstname" = "Joe"; "lastname" = "One" },
        @{"firstname" = "Rob"; "lastname" = "Two" } | 
        Set-DataverseRecord -connection $connection -TableName contact

        $result = Get-DataverseRecord -Connection $connection -TableName contact -ExcludeFilterValues @{"firstname" = "Rob" }
        
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
}
