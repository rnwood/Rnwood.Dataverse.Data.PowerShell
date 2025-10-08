. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveGlobalOptionSet Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveGlobalOptionSet SDK Cmdlet" {

        It "Invoke-DataverseRetrieveGlobalOptionSet executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveGlobalOptionSetRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveGlobalOptionSet"
                
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveGlobalOptionSetResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveGlobalOptionSet -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveGlobalOptionSet"
        }

    }
}
