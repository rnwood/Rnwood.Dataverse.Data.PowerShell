. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseExecuteAsync Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ExecuteAsync SDK Cmdlet" {
        It "Invoke-DataverseExecuteAsync executes request asynchronously" {
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.ExecuteAsyncRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.ExecuteAsyncRequest"
                $request.Request | Should -Not -BeNull
                $request.Request | Should -BeOfType [Microsoft.Xrm.Sdk.OrganizationRequest]
                
                $response = New-Object Microsoft.Xrm.Sdk.Messages.ExecuteAsyncResponse
                $asyncJobId = [Guid]::NewGuid()
                $response.Results["AsyncJobId"] = $asyncJobId
                return $response
            })
            
            # Create an inner request
            $innerRequest = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
            
            # Call the cmdlet
            $response = Invoke-DataverseExecuteAsync -Connection $script:conn -Request $innerRequest
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Xrm.Sdk.Messages.ExecuteAsyncResponse"
            $response.AsyncJobId | Should -Not -BeNullOrEmpty
            $response.AsyncJobId | Should -BeOfType [System.Guid]
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.Request | Should -Not -BeNull
            $proxy.LastRequest.Request.GetType().Name | Should -Be "WhoAmIRequest"
        }
    }
}
