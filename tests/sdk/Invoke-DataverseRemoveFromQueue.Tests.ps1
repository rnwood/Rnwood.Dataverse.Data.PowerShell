. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRemoveFromQueue Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RemoveFromQueue SDK Cmdlet" {

        It "Invoke-DataverseRemoveFromQueue removes item from queue" {
            $queueItemId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RemoveFromQueueRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RemoveFromQueueRequest"
                $request.QueueItemId | Should -BeOfType [System.Guid]
                $request.QueueItemId | Should -Be $queueItemId
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.RemoveFromQueueResponse
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseRemoveFromQueue -Connection $script:conn -QueueItemId $queueItemId
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RemoveFromQueueResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.QueueItemId | Should -Be $queueItemId
        }
    }

    }
}
