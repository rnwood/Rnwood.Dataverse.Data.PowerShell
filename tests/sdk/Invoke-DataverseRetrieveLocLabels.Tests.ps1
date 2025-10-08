. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveLocLabels Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveLocLabels SDK Cmdlet" {

        It "Invoke-DataverseRetrieveLocLabels executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveLocLabelsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveLocLabels"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveLocLabelsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveLocLabels -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveLocLabels"
        }

    }
}
