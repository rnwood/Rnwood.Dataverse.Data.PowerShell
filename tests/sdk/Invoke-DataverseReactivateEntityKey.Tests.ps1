. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseReactivateEntityKey Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ReactivateEntityKey SDK Cmdlet" {

        It "Invoke-DataverseReactivateEntityKey executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ReactivateEntityKeyRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ReactivateEntityKey"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ReactivateEntityKeyResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseReactivateEntityKey -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ReactivateEntityKey"
        }

    }
}
