. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreateKnowledgeArticleTranslation Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CreateKnowledgeArticleTranslation SDK Cmdlet" {

        It "Invoke-DataverseCreateKnowledgeArticleTranslation executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CreateKnowledgeArticleTranslationRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CreateKnowledgeArticleTranslationRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CreateKnowledgeArticleTranslationResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCreateKnowledgeArticleTranslation -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CreateKnowledgeArticleTranslationRequest"
        }

    }
}
