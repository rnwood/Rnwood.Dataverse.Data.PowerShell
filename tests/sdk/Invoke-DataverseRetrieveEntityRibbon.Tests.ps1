. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveEntityRibbon Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveEntityRibbon SDK Cmdlet" {

        It "Invoke-DataverseRetrieveEntityRibbon executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveEntityRibbonRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveEntityRibbon"
                
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveEntityRibbonResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveEntityRibbon -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveEntityRibbon"
        }

    }
}
