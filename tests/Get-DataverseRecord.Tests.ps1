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

}
