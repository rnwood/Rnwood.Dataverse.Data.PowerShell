. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSearchByKeywordsKbArticle Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SearchByKeywordsKbArticle SDK Cmdlet" {

        It "Invoke-DataverseSearchByKeywordsKbArticle executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SearchByKeywordsKbArticleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SearchByKeywordsKbArticle"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SearchByKeywordsKbArticleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSearchByKeywordsKbArticle -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SearchByKeywordsKbArticle"
        }

    }
}
