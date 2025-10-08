. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveDependenciesForDelete Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveDependenciesForDelete SDK Cmdlet" {

        It "Invoke-DataverseRetrieveDependenciesForDelete executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveDependenciesForDeleteRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveDependenciesForDelete"
                
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveDependenciesForDeleteResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveDependenciesForDelete -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveDependenciesForDelete"
        }

    }
}
