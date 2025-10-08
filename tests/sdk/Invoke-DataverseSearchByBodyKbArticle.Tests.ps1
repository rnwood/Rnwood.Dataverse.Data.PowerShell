. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSearchByBodyKbArticle Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SearchByBodyKbArticle SDK Cmdlet" {

        It "Invoke-DataverseSearchByBodyKbArticle executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SearchByBodyKbArticleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "SearchByBodyKbArticle"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SearchByBodyKbArticleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSearchByBodyKbArticle -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SearchByBodyKbArticle"
        }

    }
}
