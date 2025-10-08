. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseFullTextSearchKnowledgeArticle Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "FullTextSearchKnowledgeArticle SDK Cmdlet" {

        It "Invoke-DataverseFullTextSearchKnowledgeArticle executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.FullTextSearchKnowledgeArticleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "FullTextSearchKnowledgeArticle"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.FullTextSearchKnowledgeArticleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseFullTextSearchKnowledgeArticle -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "FullTextSearchKnowledgeArticle"
        }

    }
}
