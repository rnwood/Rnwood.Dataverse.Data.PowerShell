. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetDefaultPriceLevel Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetDefaultPriceLevel SDK Cmdlet" {

        It "Invoke-DataverseGetDefaultPriceLevel executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetDefaultPriceLevelRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetDefaultPriceLevel"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetDefaultPriceLevelResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetDefaultPriceLevel -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetDefaultPriceLevel"
        }

    }
}
