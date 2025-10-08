. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveUnpublished Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveUnpublished SDK Cmdlet" {

        It "Invoke-DataverseRetrieveUnpublished executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveUnpublished"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveUnpublished -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveUnpublished"
        }

    }
}
