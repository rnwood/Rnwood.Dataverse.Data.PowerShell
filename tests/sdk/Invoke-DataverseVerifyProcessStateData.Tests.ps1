. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseVerifyProcessStateData Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "VerifyProcessStateData SDK Cmdlet" {

        It "Invoke-DataverseVerifyProcessStateData executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.VerifyProcessStateDataRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "VerifyProcessStateData"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.VerifyProcessStateDataResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseVerifyProcessStateData -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "VerifyProcessStateData"
        }

    }
}
