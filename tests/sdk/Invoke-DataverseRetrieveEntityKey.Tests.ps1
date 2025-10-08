. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveEntityKey Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveEntityKey SDK Cmdlet" {

        It "Invoke-DataverseRetrieveEntityKey executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveEntityKeyRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveEntityKey"
                
                $responseType = "Microsoft.Xrm.Sdk.Messages.RetrieveEntityKeyResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveEntityKey -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveEntityKey"
        }

    }
}
