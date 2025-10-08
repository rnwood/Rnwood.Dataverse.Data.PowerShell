. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAttribute Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAttribute SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAttribute executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveAttributeRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveAttribute"
                
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveAttributeResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveAttribute -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAttribute"
        }

    }
}
