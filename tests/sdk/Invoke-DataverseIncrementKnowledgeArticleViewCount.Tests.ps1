. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseIncrementKnowledgeArticleViewCount Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "IncrementKnowledgeArticleViewCount SDK Cmdlet" {

        It "Invoke-DataverseIncrementKnowledgeArticleViewCount executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.IncrementKnowledgeArticleViewCountRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "IncrementKnowledgeArticleViewCount"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.IncrementKnowledgeArticleViewCountResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseIncrementKnowledgeArticleViewCount -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "IncrementKnowledgeArticleViewCount"
        }

    }
}
