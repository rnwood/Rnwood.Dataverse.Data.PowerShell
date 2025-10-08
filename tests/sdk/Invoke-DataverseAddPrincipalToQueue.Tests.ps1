. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseAddPrincipalToQueue Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "AddPrincipalToQueue SDK Cmdlet" {

        It "Invoke-DataverseAddPrincipalToQueue adds a principal to a queue" {
            $queueId = [Guid]::NewGuid()
            $principalId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.AddPrincipalToQueueRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.AddPrincipalToQueueRequest"
                $request.QueueId | Should -BeOfType [System.Guid]
                $request.Principal | Should -Not -BeNull
                $request.Principal | Should -BeOfType [Microsoft.Xrm.Sdk.EntityReference]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.AddPrincipalToQueueResponse
                return $response
            })
            
            # Call the cmdlet
            $principal = New-Object Microsoft.Xrm.Sdk.EntityReference("systemuser", $principalId)
            $response = Invoke-DataverseAddPrincipalToQueue -Connection $script:conn -QueueId $queueId -Principal $principal
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.AddPrincipalToQueueResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.QueueId | Should -Be $queueId
            $proxy.LastRequest.Principal.Id | Should -Be $principalId
        }
    }

    }
}
