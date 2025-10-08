. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRecalculatePrice Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RecalculatePrice SDK Cmdlet" {

        It "Invoke-DataverseRecalculatePrice executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RecalculatePriceRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RecalculatePrice"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RecalculatePriceResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRecalculatePrice -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RecalculatePrice"
        }

    }
}
