. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveApplicationRibbon Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveApplicationRibbon SDK Cmdlet" {

        It "Invoke-DataverseRetrieveApplicationRibbon executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveApplicationRibbonRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveApplicationRibbon"
                
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveApplicationRibbonResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveApplicationRibbon -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveApplicationRibbon"
        }

    }
}
