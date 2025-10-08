. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseFormatAddress Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "FormatAddress SDK Cmdlet" {

        It "Invoke-DataverseFormatAddress executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.FormatAddressRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "FormatAddress"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.FormatAddressResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseFormatAddress -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "FormatAddress"
        }

    }
}
