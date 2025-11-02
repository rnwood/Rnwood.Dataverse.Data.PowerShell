. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseRecord - FetchXml' {
    Context 'Basic FetchXml Queries' {
        It "Executes FetchXml query with filter and returns correct records" {
            $connection = getMockConnection
            
            # Create test data
            @(
                @{ firstname = "John"; lastname = "Doe"; emailaddress1 = "john@example.com" }
                @{ firstname = "Jane"; lastname = "Smith"; emailaddress1 = "jane@example.com" }
                @{ firstname = "Bob"; lastname = "Doe"; emailaddress1 = "bob@example.com" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Execute FetchXml query
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <attribute name="emailaddress1" />
    <filter type="and">
      <condition attribute="lastname" operator="eq" value="Doe" />
    </filter>
  </entity>
</fetch>
"@
            
            $results = Get-DataverseRecord -Connection $connection -FetchXml $fetchXml
            
            # Assert results
            $results | Should -Not -BeNullOrEmpty
            $results | Should -HaveCount 2
            $results | ForEach-Object { $_.lastname | Should -Be "Doe" }
            ($results | Where-Object { $_.firstname -eq "John" }) | Should -HaveCount 1
            ($results | Where-Object { $_.firstname -eq "Bob" }) | Should -HaveCount 1
            
            # Verify no side effects - all records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 3
        }

        It "Executes FetchXml with ordering and returns sorted results" {
            $connection = getMockConnection
            
            # Create test data in random order
            @(
                @{ firstname = "Charlie"; lastname = "Test"; donotbulkemail = $true }
                @{ firstname = "Alice"; lastname = "Test"; donotbulkemail = $false }
                @{ firstname = "Bob"; lastname = "Test"; donotbulkemail = $true }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Execute FetchXml with ordering
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <order attribute="firstname" descending="false" />
    <filter type="and">
      <condition attribute="lastname" operator="eq" value="Test" />
    </filter>
  </entity>
</fetch>
"@
            
            $results = Get-DataverseRecord -Connection $connection -FetchXml $fetchXml
            
            # Assert results are sorted
            $results | Should -HaveCount 3
            $results[0].firstname | Should -Be "Alice"
            $results[1].firstname | Should -Be "Bob"
            $results[2].firstname | Should -Be "Charlie"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 3
        }

        It "Executes FetchXml with multiple filter conditions using AND" {
            $connection = getMockConnection
            
            # Create test data
            @(
                @{ firstname = "Match"; lastname = "Both"; emailaddress1 = "match@example.com" }
                @{ firstname = "Match"; lastname = "Wrong"; emailaddress1 = "wrong@example.com" }
                @{ firstname = "Wrong"; lastname = "Both"; emailaddress1 = "wrong2@example.com" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Execute FetchXml with AND conditions
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <attribute name="emailaddress1" />
    <filter type="and">
      <condition attribute="firstname" operator="eq" value="Match" />
      <condition attribute="lastname" operator="eq" value="Both" />
    </filter>
  </entity>
</fetch>
"@
            
            $results = Get-DataverseRecord -Connection $connection -FetchXml $fetchXml
            
            # Assert only matching record returned
            $results | Should -Not -BeNullOrEmpty
            $results | Should -HaveCount 1
            $results[0].firstname | Should -Be "Match"
            $results[0].lastname | Should -Be "Both"
            $results[0].emailaddress1 | Should -Be "match@example.com"
            
            # Verify no side effects - all records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 3
        }
    }

    Context 'Advanced FetchXml Features' {
        It "Executes FetchXml with LIKE operator" {
            $connection = getMockConnection
            
            # Create test data
            @(
                @{ firstname = "TestUser1"; lastname = "Smith" }
                @{ firstname = "TestUser2"; lastname = "Smith" }
                @{ firstname = "OtherUser"; lastname = "Smith" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Execute FetchXml with LIKE
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <filter type="and">
      <condition attribute="firstname" operator="like" value="TestUser%" />
    </filter>
  </entity>
</fetch>
"@
            
            $results = Get-DataverseRecord -Connection $connection -FetchXml $fetchXml
            
            # Assert correct records returned
            $results | Should -HaveCount 2
            $results | ForEach-Object { $_.firstname | Should -BeLike "TestUser*" }
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 3
        }

        It "Executes FetchXml with TOP clause limiting results" {
            $connection = getMockConnection
            
            # Create test data
            1..10 | ForEach-Object {
                @{ firstname = "User$_"; lastname = "Test" } | 
                    Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            }
            
            # Execute FetchXml with TOP
            $fetchXml = @"
<fetch top="3">
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
  </entity>
</fetch>
"@
            
            $results = Get-DataverseRecord -Connection $connection -FetchXml $fetchXml
            
            # Assert limited results
            $results | Should -HaveCount 3
            
            # Verify no side effects - all 10 records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 10
        }

        It "Executes FetchXml with OR filter conditions" {
            $connection = getMockConnection
            
            # Create test data
            @(
                @{ firstname = "Alice"; lastname = "Smith" }
                @{ firstname = "Bob"; lastname = "Jones" }
                @{ firstname = "Charlie"; lastname = "Brown" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Execute FetchXml with OR conditions
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <filter type="or">
      <condition attribute="lastname" operator="eq" value="Smith" />
      <condition attribute="lastname" operator="eq" value="Jones" />
    </filter>
  </entity>
</fetch>
"@
            
            $results = Get-DataverseRecord -Connection $connection -FetchXml $fetchXml
            
            # Assert correct records returned
            $results | Should -HaveCount 2
            ($results | Where-Object { $_.lastname -eq "Smith" }) | Should -HaveCount 1
            ($results | Where-Object { $_.lastname -eq "Jones" }) | Should -HaveCount 1
            ($results | Where-Object { $_.lastname -eq "Brown" }) | Should -BeNullOrEmpty
            
            # Verify no side effects - all records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 3
        }
    }
}
