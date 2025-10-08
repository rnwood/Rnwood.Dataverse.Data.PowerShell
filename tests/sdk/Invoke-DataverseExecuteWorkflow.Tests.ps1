. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseExecuteWorkflow Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ExecuteWorkflow SDK Cmdlet" {

        It "Invoke-DataverseExecuteWorkflow triggers workflow execution" {
            $workflowId = [Guid]::NewGuid()
            $entityId = [Guid]::NewGuid()
            
            # Stub the response since FakeXrmEasy OSS doesn't support this
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ExecuteWorkflowRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.ExecuteWorkflowResponse
                $response.Results["Id"] = [Guid]::NewGuid()
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseExecuteWorkflow -Connection $script:conn -WorkflowId $workflowId -EntityId $entityId
            
            # Verify response
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "ExecuteWorkflowResponse"
            $response.Id | Should -Not -BeNullOrEmpty
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "ExecuteWorkflowRequest"
            $proxy.LastRequest.WorkflowId | Should -Be $workflowId
            $proxy.LastRequest.EntityId | Should -Be $entityId
        }
    }

    }
}
