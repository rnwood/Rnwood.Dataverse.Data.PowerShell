. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveDependentComponents Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveDependentComponents SDK Cmdlet" {

        It "Invoke-DataverseRetrieveDependentComponents executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveDependentComponentsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveDependentComponents"
                
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveDependentComponentsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveDependentComponents -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveDependentComponents"
        }

    }
}
