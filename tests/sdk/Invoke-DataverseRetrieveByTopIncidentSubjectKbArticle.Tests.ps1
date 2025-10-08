. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveByTopIncidentSubjectKbArticle Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveByTopIncidentSubjectKbArticle SDK Cmdlet" {

        It "Invoke-DataverseRetrieveByTopIncidentSubjectKbArticle executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveByTopIncidentSubjectKbArticleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveByTopIncidentSubjectKbArticle"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveByTopIncidentSubjectKbArticleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveByTopIncidentSubjectKbArticle -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveByTopIncidentSubjectKbArticle"
        }

    }
}
