. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseAddToQueue Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "AddToQueue SDK Cmdlet" {

        It "Invoke-DataverseAddToQueue adds item to queue" {
            $queueId = [Guid]::NewGuid()
            $targetId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.AddToQueueRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.AddToQueueRequest"
                $request.DestinationQueueId | Should -BeOfType [System.Guid]
                $request.Target | Should -Not -BeNull
                $request.Target | Should -BeOfType [Microsoft.Xrm.Sdk.EntityReference]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.AddToQueueResponse
                $queueItemId = [Guid]::NewGuid()
                $response.Results["QueueItemId"] = $queueItemId
                return $response
            })
            
            # Call the cmdlet
            $target = New-Object Microsoft.Xrm.Sdk.EntityReference("email", $targetId)
            $response = Invoke-DataverseAddToQueue -Connection $script:conn -DestinationQueueId $queueId -Target $target
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.AddToQueueResponse"
            $response.QueueItemId | Should -Not -BeNullOrEmpty
            $response.QueueItemId | Should -BeOfType [System.Guid]
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.DestinationQueueId | Should -Be $queueId
            $proxy.LastRequest.Target.Id | Should -Be $targetId
        }
    }

    }
}
