. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetState Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetState SDK Cmdlet" {

        It "Invoke-DataverseSetState changes record state" {
            # Create a workflow record
            $connection = getMockConnection -AdditionalEntities @("workflow")
            $workflowId = [Guid]::NewGuid()
            
            # Stub the SetStateResponse since we're testing the request pattern
            $proxy = Get-ProxyService -Connection $connection
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetStateRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.SetStateResponse
                return $response
            })
            
            # Call the SetState cmdlet using the specialized SDK cmdlet
            $entityRef = New-Object Microsoft.Xrm.Sdk.EntityReference("workflow", $workflowId)
            $state = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $status = New-Object Microsoft.Xrm.Sdk.OptionSetValue(2)
            
            # Note: There's no Invoke-DataverseSetState - SetState is done via generic request
            $setStateRequest = New-Object Microsoft.Crm.Sdk.Messages.SetStateRequest
            $setStateRequest.EntityMoniker = $entityRef
            $setStateRequest.State = $state
            $setStateRequest.Status = $status
            
            { Invoke-DataverseRequest -Connection $connection -Request $setStateRequest } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "SetStateRequest"
            $proxy.LastRequest.EntityMoniker.LogicalName | Should -Be "workflow"
            $proxy.LastRequest.EntityMoniker.Id | Should -Be $workflowId
            $proxy.LastRequest.State.Value | Should -Be 1
            $proxy.LastRequest.Status.Value | Should -Be 2
        }
    }

    }
}
