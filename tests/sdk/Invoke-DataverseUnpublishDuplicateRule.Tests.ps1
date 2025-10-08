. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUnpublishDuplicateRule Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UnpublishDuplicateRule SDK Cmdlet" {

        It "Invoke-DataverseUnpublishDuplicateRule executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UnpublishDuplicateRuleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UnpublishDuplicateRule"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UnpublishDuplicateRuleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUnpublishDuplicateRule -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UnpublishDuplicateRule"
        }

    }
}
