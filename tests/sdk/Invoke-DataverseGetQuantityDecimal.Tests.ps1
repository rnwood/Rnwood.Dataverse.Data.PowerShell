. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetQuantityDecimal Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetQuantityDecimal SDK Cmdlet" {

        It "Invoke-DataverseGetQuantityDecimal executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetQuantityDecimalRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetQuantityDecimal"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetQuantityDecimalResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetQuantityDecimal -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetQuantityDecimal"
        }

    }
}
