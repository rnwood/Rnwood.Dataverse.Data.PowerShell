. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseInitializeModernFlowFromAsyncWorkflow Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "InitializeModernFlowFromAsyncWorkflow SDK Cmdlet" {

        It "Invoke-DataverseInitializeModernFlowFromAsyncWorkflow executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.InitializeModernFlowFromAsyncWorkflowRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "InitializeModernFlowFromAsyncWorkflow"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.InitializeModernFlowFromAsyncWorkflowResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseInitializeModernFlowFromAsyncWorkflow -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "InitializeModernFlowFromAsyncWorkflow"
        }

    }
}
