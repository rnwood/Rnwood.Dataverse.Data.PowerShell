. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataversePickFromQueue Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "PickFromQueue SDK Cmdlet" {

        It "Invoke-DataversePickFromQueue picks item from queue" {
            $queueItemId = [Guid]::NewGuid()
            $workerId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.PickFromQueueRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.PickFromQueueRequest"
                $request.QueueItemId | Should -BeOfType [System.Guid]
                $request.WorkerId | Should -BeOfType [System.Guid]
                $request.RemoveQueueItem | Should -BeOfType [System.Boolean]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.PickFromQueueResponse
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataversePickFromQueue -Connection $script:conn -QueueItemId $queueItemId -WorkerId $workerId -RemoveQueueItem $false
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.PickFromQueueResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.QueueItemId | Should -Be $queueItemId
            $proxy.LastRequest.WorkerId | Should -Be $workerId
            $proxy.LastRequest.RemoveQueueItem | Should -Be $false
        }
    }

    }
}
