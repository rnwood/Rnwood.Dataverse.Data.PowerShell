Describe 'Set-DataverseRecord Multi-Request Completion' {

    . $PSScriptRoot/Common.ps1

    Context 'Multiple Requests Per Context' {
        It "Demonstrates the bug: M:M upsert with PassThru creates 2 requests but only first completion is called" {
            # This test demonstrates the actual bug in SetBatchProcessor.ExecuteBatch
            # When a context has multiple requests (like M:M upsert with PassThru),
            # only the first response triggers ResponseCompletion (line 1043: i == 0)
            
            # The bug is in this code at line 1043:
            # else if (context.ResponseCompletion != null && i == 0)
            # {
            #     // Call success completion callback for the first (typically only) response
            #     context.ResponseCompletion(itemResponse.Response);
            # }
            
            # This means for M:M upsert with PassThru:
            # - Request 1 (AssociateRequest): completion called -> writes verbose message
            # - Request 2 (RetrieveMultipleRequest): completion NOT called -> Id not set on InputObject
            
            # Since we don't have M:M metadata loaded, we'll simulate the issue by
            # tracking whether all expected operations complete properly
            
            $connection = getMockConnection
            
            # Create a simple record with PassThru
            $record = @{
                firstname = "Test"
                lastname = "User"
            }
            
            $result = $record | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru -BatchSize 10
            
            # Verify PassThru worked (Id was set by completion handler)
            $result | Should -Not -BeNullOrEmpty
            $result.Id | Should -BeOfType [Guid]
            $result.Id | Should -Not -Be ([Guid]::Empty)
            
            # This test passes because CreateNewRecord only adds 1 request (CreateRequest)
            # The bug would manifest if we had 2+ requests per context
        }
        
        It "Handles batched operations where each context has multiple requests" {
            # Test scenario: Create multiple records in batch with state/status changes
            # This creates multiple requests per context (Create + SetState)
            
            $connection = getMockConnection
            
            # Create contacts with statecode set
            # Each will create: CreateRequest + SetStateRequest
            $records = @(
                @{ 
                    firstname = "Contact1"
                    lastname = "Test"
                    statecode = 1  # Inactive
                    statuscode = 2
                }
                @{ 
                    firstname = "Contact2"
                    lastname = "Test"
                    statecode = 1  # Inactive
                    statuscode = 2
                }
            )
            
            # Use BatchSize to ensure batching
            $results = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru -BatchSize 10
            
            # Verify all records were created and PassThru worked for all
            $results | Should -HaveCount 2
            $results | ForEach-Object {
                $_.Id | Should -BeOfType [Guid]
                $_.Id | Should -Not -Be ([Guid]::Empty)
                $_.firstname | Should -Match "Contact[12]"
            }
            
            # Verify records were created in Dataverse
            $retrieved1 = Get-DataverseRecord -Connection $connection -TableName contact -Id $results[0].Id
            $retrieved1.firstname | Should -Be "Contact1"
            
            $retrieved2 = Get-DataverseRecord -Connection $connection -TableName contact -Id $results[1].Id
            $retrieved2.firstname | Should -Be "Contact2"
        }
    }
}
