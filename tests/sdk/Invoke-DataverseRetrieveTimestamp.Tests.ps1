. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveTimestamp Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveTimestamp SDK Cmdlet" {

        It "Invoke-DataverseRetrieveTimestamp retrieves server timestamp" {
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveTimestampRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RetrieveTimestampRequest"
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveTimestampResponse
                $response.Results["Timestamp"] = "2025-01-01T00:00:00Z"
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseRetrieveTimestamp -Connection $script:conn
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RetrieveTimestampResponse"
            $response.Timestamp | Should -Not -BeNullOrEmpty
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RetrieveTimestampRequest"
        }
    }

    }
}
