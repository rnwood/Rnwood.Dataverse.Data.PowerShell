. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveByTopIncidentProductKbArticle Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveByTopIncidentProductKbArticle SDK Cmdlet" {

        It "Invoke-DataverseRetrieveByTopIncidentProductKbArticle executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveByTopIncidentProductKbArticleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveByTopIncidentProductKbArticle"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveByTopIncidentProductKbArticleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveByTopIncidentProductKbArticle -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveByTopIncidentProductKbArticle"
        }

    }
}
