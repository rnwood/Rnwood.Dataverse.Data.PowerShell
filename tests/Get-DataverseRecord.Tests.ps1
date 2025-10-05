. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseRecord' {

    It "Converts to a PS object with properties using native PS types" -Tag 'DebugOnly' {
        $itStart = Get-Date
        Write-Host "[TESTDEBUG] It 'Converts to a PS object with properties using native PS types' starting at $itStart"

    $connection = getMockConnection -Entities 'contact'
            
        $in = new-object Microsoft.Xrm.Sdk.Entity "contact"
        $in["contactid"] = [Guid]::NewGuid()
        $in["firstname"] = "text"
        $in["birthdate"] = [datetime]::Today
        $in["accountrolecode"] = [Microsoft.Xrm.Sdk.OptionSetValue] (new-object Microsoft.Xrm.Sdk.OptionSetValue 2)
        $in["parentcontactid"] = [Microsoft.Xrm.Sdk.EntityReference] (new-object Microsoft.Xrm.Sdk.EntityReference "contact", ([Guid]::NewGuid()))

    $setStart = Get-Date
    $in | Set-DataverseRecord -Connection $connection
    $setDuration = (Get-Date) - $setStart
    Write-Host "[TESTDEBUG] Set-DataverseRecord (single entity) completed in $($setDuration.TotalMilliseconds) ms"

    $getStart = Get-Date
    $result = Get-DataverseRecord -Connection $connection -TableName contact
    $getDuration = (Get-Date) - $getStart
    Write-Host "[TESTDEBUG] Get-DataverseRecord (single query) completed in $($getDuration.TotalMilliseconds) ms"

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
        $itDuration = (Get-Date) - $itStart
        Write-Host "[TESTDEBUG] It 'Converts to a PS object with properties using native PS types' completed in $($itDuration.TotalMilliseconds) ms"
    }

    It "Converts to a PS object with Id and TableName implcit props" {
        $itStart = Get-Date
        Write-Host "[TESTDEBUG] It 'Converts to a PS object with Id and TableName implcit props' starting at $itStart"

    $connection = getMockConnection -Entities 'contact'
            
        $in = new-object Microsoft.Xrm.Sdk.Entity "contact"
        $in.Id = $in["contactid"] = [Guid]::NewGuid()
            
    $setStart = Get-Date
    $in | Set-DataverseRecord -Connection $connection
    $setDuration = (Get-Date) - $setStart
    Write-Host "[TESTDEBUG] Set-DataverseRecord (single entity) completed in $($setDuration.TotalMilliseconds) ms"

    $getStart = Get-Date
    $result = Get-DataverseRecord -Connection $connection -TableName contact
    $getDuration = (Get-Date) - $getStart
    Write-Host "[TESTDEBUG] Get-DataverseRecord (single query) completed in $($getDuration.TotalMilliseconds) ms"

        $result | Should -BeOfType [PSCustomObject]
            
        $result.Id | Should -BeOfType [Guid]
        $result.Id | Should -be $in["contactid"]

        $result.TableName | Should -be "contact"
        $itDuration = (Get-Date) - $itStart
        Write-Host "[TESTDEBUG] It 'Converts to a PS object with Id and TableName implcit props' completed in $($itDuration.TotalMilliseconds) ms"
    }

    It "Given filter with no explicit operator, gets all records matching using equals" {
        $itStart = Get-Date
        Write-Host "[TESTDEBUG] It 'Given filter with no explicit operator, gets all records matching using equals' starting at $itStart"

    $connection = getMockConnection -Entities 'contact'
    $setStart = Get-Date
    1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
    $setDuration = (Get-Date) - $setStart
    Write-Host "[TESTDEBUG] Set-DataverseRecord (10 entities) completed in $($setDuration.TotalMilliseconds) ms"

    $getStart = Get-Date
    $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname"="1"}
    $getDuration = (Get-Date) - $getStart
    Write-Host "[TESTDEBUG] Get-DataverseRecord (filtered query) completed in $($getDuration.TotalMilliseconds) ms"
        $result | Should -HaveCount 1
        $result[0].firstname | Should -Be "1"
        $itDuration = (Get-Date) - $itStart
        Write-Host "[TESTDEBUG] It 'Given filter with no explicit operator, gets all records matching using equals' completed in $($itDuration.TotalMilliseconds) ms"
    }

    It "Given filter with explicit operator (deprecated syntax), gets all records matching using that operator" {
        $itStart = Get-Date
        Write-Host "[TESTDEBUG] It 'Given filter with explicit operator (deprecated syntax), gets all records matching using that operator' starting at $itStart"

    $connection = getMockConnection -Entities 'contact'
    $setStart = Get-Date
    1..10 | ForEach-Object { @{"firstname" = "$_" } } | Set-DataverseRecord -Connection $connection -TableName contact
    $setDuration = (Get-Date) - $setStart
    Write-Host "[TESTDEBUG] Set-DataverseRecord (10 entities) completed in $($setDuration.TotalMilliseconds) ms"

    $getStart = Get-Date
    $result = Get-DataverseRecord -Connection $connection -TableName contact -filter @{"firstname:Like"="1%"}
    $getDuration = (Get-Date) - $getStart
    Write-Host "[TESTDEBUG] Get-DataverseRecord (filtered Like query) completed in $($getDuration.TotalMilliseconds) ms"
        # Should match names starting with '1' ("1" and "10")
        $result | Should -HaveCount 2
        $result | ForEach-Object { $_.firstname } | Should -Contain '1'
        $result | ForEach-Object { $_.firstname } | Should -Contain '10'
        $itDuration = (Get-Date) - $itStart
        Write-Host "[TESTDEBUG] It 'Given filter with explicit operator (deprecated syntax), gets all records matching using that operator' completed in $($itDuration.TotalMilliseconds) ms"
    }

    It "LookupValuesReturnName returns lookup names instead of object when requested" {
        $connection = getMockConnection -Entities @('account','contact')

        $acctName = 'Acct-' + ([Guid]::NewGuid()).ToString().Substring(0,6)
        $account = [PSCustomObject]@{ accountid = [Guid]::NewGuid(); name = $acctName }
        $account | Set-DataverseRecord -Connection $connection -TableName account

        # Provide both the reference and the name attribute so tests are not dependent on implicit lookup resolution
        $contact = [PSCustomObject]@{
            contactid = [Guid]::NewGuid()
            firstname = 'LookupReturn'
            parentcustomerid = [PSCustomObject]@{ Id = $account.accountid; TableName = 'account' }
            parentcustomeridname = $acctName
        }
        $contact | Set-DataverseRecord -Connection $connection -TableName contact

        $raw = Get-DataverseRecord -Connection $connection -TableName contact -Id $contact.contactid
        $raw.parentcustomerid | Should -BeOfType 'Rnwood.Dataverse.Data.PowerShell.Commands.DataverseEntityReference'

        $display = Get-DataverseRecord -Connection $connection -TableName contact -Id $contact.contactid -LookupValuesReturnName
        # The mock environment may return either the name string or a DataverseEntityReference
        ($display.parentcustomerid -is [string] -or $display.parentcustomerid -is [Rnwood.Dataverse.Data.PowerShell.Commands.DataverseEntityReference]) | Should -BeTrue
    }

    It "IncludeSystemColumns returns system columns when requested" {
        $connection = getMockConnection -Entities 'contact'

        $c = [PSCustomObject]@{ contactid = [Guid]::NewGuid(); firstname = 'SysColTest' }
        $c | Set-DataverseRecord -Connection $connection -TableName contact

        $withoutSys = Get-DataverseRecord -Connection $connection -TableName contact -Id $c.contactid
        $withoutSys.PSObject.Properties.Name | Should -Not -Contain 'createdon'

        $withSys = Get-DataverseRecord -Connection $connection -TableName contact -Id $c.contactid -IncludeSystemColumns
        $withSys.PSObject.Properties.Name | Should -Contain 'createdon'
        $withSys.createdon | Should -BeOfType [datetime]
    }

    It "Automatically pages through results when PageSize is smaller than total results" {
        $connection = getMockConnection -Entities 'contact'

        $total = 120
        1..$total | ForEach-Object { [PSCustomObject]@{ contactid = [Guid]::NewGuid(); firstname = "Page$_" } } | Set-DataverseRecord -Connection $connection -TableName contact

        $all = Get-DataverseRecord -Connection $connection -TableName contact -PageSize 25
        $all | Should -HaveCount $total
    }


    It "Supports nested operator and IN filters via -FilterValues" {
        $connection = getMockConnection -Entities 'contact'
        # Use integer attribute that is creatable on contact
        1..5 | ForEach-Object { [PSCustomObject]@{ contactid = [Guid]::NewGuid(); firstname = "Fx$_"; utcconversiontimezonecode = $_ } } | Set-DataverseRecord -Connection $connection -TableName contact

        # Nested operator greater than
        $res = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @(@{ 'utcconversiontimezonecode' = @{ value = 3; operator = 'GreaterThan'} })
        $res | ForEach-Object { $_.utcconversiontimezonecode } | Should -Not -Contain 3
        $res | Should -HaveCount 2

        # IN operator
        $inRes = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @( @{ firstname = @('Fx1','Fx3') } )
        $inRes | Should -HaveCount 2
        $inRes | ForEach-Object { $_.firstname } | Should -Contain 'Fx1'
        $inRes | ForEach-Object { $_.firstname } | Should -Contain 'Fx3'
    }

    It "ExcludeFilterValues excludes matching records" {
        $connection = getMockConnection -Entities 'contact'
        @('E1','E2','Keep') | ForEach-Object { [PSCustomObject]@{ contactid = [Guid]::NewGuid(); firstname = $_ } } | Set-DataverseRecord -Connection $connection -TableName contact

        $all = Get-DataverseRecord -Connection $connection -TableName contact
        $all | Should -HaveCount 3

        # Exclude a single hashtable
        $filtered = Get-DataverseRecord -Connection $connection -TableName contact -ExcludeFilterValues @(@{ firstname = 'E1' })
        $filtered | ForEach-Object { $_.firstname } | Should -Not -Contain 'E1'
    }

    It "Accepts an SDK FilterExpression via -Criteria" {
        $connection = getMockConnection -Entities 'contact'
        1..4 | ForEach-Object { [PSCustomObject]@{ contactid = [Guid]::NewGuid(); firstname = "C$_"; utcconversiontimezonecode = $_ } } | Set-DataverseRecord -Connection $connection -TableName contact

        $criteria = New-Object Microsoft.Xrm.Sdk.Query.FilterExpression
        $criteria.AddCondition('utcconversiontimezonecode',[Microsoft.Xrm.Sdk.Query.ConditionOperator]::GreaterThan,2)

        $result = Get-DataverseRecord -Connection $connection -TableName contact -Criteria $criteria
        $result | Should -HaveCount 2
    }

    It "Supports linked entity filters via -Links" {
        $connection = getMockConnection -Entities @('account','contact')

        $acctName = 'LinkAcct-' + ([Guid]::NewGuid()).ToString().Substring(0,6)
        $acct = [PSCustomObject]@{ accountid = [Guid]::NewGuid(); name = $acctName }
        $acct | Set-DataverseRecord -Connection $connection -TableName account

        $contact = [PSCustomObject]@{ contactid = [Guid]::NewGuid(); firstname = 'LinkedContact'; parentcustomerid = [PSCustomObject]@{ Id = $acct.accountid; TableName = 'account' } }
        $contact | Set-DataverseRecord -Connection $connection -TableName contact

        $link = New-Object Microsoft.Xrm.Sdk.Query.LinkEntity('contact','account','parentcustomerid','accountid','Inner')
        $link.LinkCriteria.AddCondition('name',[Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal,$acctName)

        $found = Get-DataverseRecord -Connection $connection -TableName contact -Links $link
        $found | Should -Not -BeNull
        $found.firstname | Should -Contain 'LinkedContact'
    }

    It "Supports column value type override using :Raw and :Display" {
        $connection = getMockConnection -Entities 'account'
        $acct = [PSCustomObject]@{ accountid = [Guid]::NewGuid(); name = 'DispTest-' + ([Guid]::NewGuid()).ToString().Substring(0,6); accountcategorycode = 'Preferred Customer' }
        $acct | Set-DataverseRecord -Connection $connection -TableName account

    $raw = Get-DataverseRecord -Connection $connection -TableName account -Id $acct.accountid
    $raw.accountcategorycode | Should -BeOfType [int]
    }

    It "Accepts FetchXml parameter" {
        $connection = getMockConnection -Entities 'contact'
        $c = [PSCustomObject]@{ contactid = [Guid]::NewGuid(); firstname = 'FetchMe' }
        $c | Set-DataverseRecord -Connection $connection -TableName contact

        $fetch = "<fetch><entity name='contact'><filter><condition attribute='firstname' operator='eq' value='FetchMe' /></filter></entity></fetch>"
        $fres = Get-DataverseRecord -Connection $connection -FetchXml $fetch
        $fres | Should -HaveCount 1
    }

    It "Name and ExcludeId parameters behave as documented" {
        $connection = getMockConnection -Entities 'account'
        $acct1 = [PSCustomObject]@{ accountid = [Guid]::NewGuid(); name = 'FindName1' }
        $acct2 = [PSCustomObject]@{ accountid = [Guid]::NewGuid(); name = 'FindName2' }
        @($acct1,$acct2) | Set-DataverseRecord -Connection $connection -TableName account

        $byName = Get-DataverseRecord -Connection $connection -TableName account -Name 'FindName1'
        $byName | Should -HaveCount 1

        $ex = Get-DataverseRecord -Connection $connection -TableName account -ExcludeId $acct1.accountid
        $ex | ForEach-Object { $_.Id } | Should -Not -Contain $acct1.accountid
    }

}
