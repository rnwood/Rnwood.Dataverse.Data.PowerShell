. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSearchByTitleKbArticle Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SearchByTitleKbArticle SDK Cmdlet" {

        It "Invoke-DataverseSearchByTitleKbArticle executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SearchByTitleKbArticleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SearchByTitleKbArticle"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SearchByTitleKbArticleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSearchByTitleKbArticle -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SearchByTitleKbArticle"
        }

    }
}
